// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DefaultRegistry.cs" company="Web Advanced">
// Copyright 2012 Web Advanced (www.webadvanced.com)
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Web;
using AutoMapper;
using MediatR;
using Microsoft.Azure;
using SFA.DAS.Audit.Client;
using SFA.DAS.Commitments.Api.Client;
using SFA.DAS.Commitments.Api.Client.Configuration;
using SFA.DAS.Commitments.Api.Client.Interfaces;
using SFA.DAS.Configuration;
using SFA.DAS.Configuration.AzureTableStorage;
using SFA.DAS.Configuration.FileStorage;
using SFA.DAS.CookieService;
using SFA.DAS.EAS.Application.Validation;
using SFA.DAS.EAS.Domain.Configuration;
using NotificationsApiClientConfiguration = SFA.DAS.EAS.Domain.Configuration.NotificationsApiClientConfiguration;
using SFA.DAS.EAS.Domain.Data.Repositories;
using SFA.DAS.EAS.Domain.Interfaces;
using IConfiguration = SFA.DAS.EAS.Domain.Interfaces.IConfiguration;
using SFA.DAS.EAS.Domain.Models.UserProfile;
using SFA.DAS.EAS.Infrastructure.Caching;
using SFA.DAS.EAS.Infrastructure.Data;
using SFA.DAS.EAS.Infrastructure.Factories;
using SFA.DAS.EAS.Infrastructure.Interfaces.REST;
using SFA.DAS.EAS.Infrastructure.Services;
using SFA.DAS.EAS.Web.App_Start;
using SFA.DAS.EAS.Web.Authentication;
using SFA.DAS.EAS.Web.ViewModels;
using SFA.DAS.Events.Api.Client;
using SFA.DAS.Events.Api.Client.Configuration;
using SFA.DAS.HashingService;
using SFA.DAS.Http;
using SFA.DAS.Http.TokenGenerators;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Notifications.Api.Client;
using SFA.DAS.Notifications.Api.Client.Configuration;
using SFA.DAS.Tasks.API.Client;
using StructureMap;
using StructureMap.TypeRules;

namespace SFA.DAS.EAS.Web.DependencyResolution
{
    public class DefaultRegistry : Registry
    {
        private const string ServiceName = "SFA.DAS.EmployerApprenticeshipsService";
        private const string ServiceNamespace = "SFA.DAS";

        public DefaultRegistry()
        {
            var config = GetConfiguration();
            var notificationsApiConfig = Infrastructure.DependencyResolution.ConfigurationHelper.GetConfiguration<NotificationsApiClientConfiguration>($"{ServiceName}.Notifications");
            var taskApiConfig = Infrastructure.DependencyResolution.ConfigurationHelper.GetConfiguration<TaskApiConfiguration>($"SFA.DAS.Tasks.Api");

            Scan(scan =>
            {
                scan.AssembliesFromApplicationBaseDirectory(a => a.GetName().Name.StartsWith(ServiceNamespace));
                scan.RegisterConcreteTypesAgainstTheFirstInterface();
                scan.ConnectImplementationsToTypesClosing(typeof(IValidator<>)).OnAddedPluginTypes(c => c.Singleton());
            });
            
            For<HttpContextBase>().Use(() => new HttpContextWrapper(HttpContext.Current));
            For<IApprenticeshipApi>().Use<ApprenticeshipApi>().Ctor<ICommitmentsApiClientConfiguration>().Is(config.CommitmentsApi);
            For<ICache>().Use<InMemoryCache>(); //RedisCache
            For<IConfiguration>().Use<EmployerApprenticeshipsServiceConfiguration>();
            For(typeof(ICookieService<>)).Use(typeof(HttpCookieService<>));
            For(typeof(ICookieStorageService<>)).Use(typeof(CookieStorageService<>));
            For<ICurrentUserService>().Use<CurrentUserService>();
            For<IEventsApi>().Use<EventsApi>().Ctor<IEventsApiClientConfiguration>().Is(config.EventsApi).SelectConstructor(() => new EventsApi(null)); // The default one isn't the one we want to use.;
            For<IEmployerCommitmentApi>().Use<EmployerCommitmentApi>().Ctor<ICommitmentsApiClientConfiguration>().Is(config.CommitmentsApi);
            For<IHashingService>().Use(x => new HashingService.HashingService(config.AllowedHashstringCharacters, config.Hashstring));
            For<ITaskApiConfiguration>().Use(taskApiConfig);
            For<ITaskService>().Use<TaskService>();
            For<IUserRepository>().Use<UserRepository>();
            For<IValidationApi>().Use<ValidationApi>().Ctor<ICommitmentsApiClientConfiguration>().Is(config.CommitmentsApi);
            For<CurrentUser>().Use(c => c.GetInstance<ICurrentUserService>().GetCurrentUser());
            
            ConfigureNotificationsApi(notificationsApiConfig);
            RegisterMapper();
            RegisterMediator();
            RegisterAuditService();            
            RegisterPostCodeAnywhereService();
            RegisterExecutionPolicies();
            RegisterLogger();
        }

        private void ConfigureNotificationsApi(NotificationsApiClientConfiguration config)
        {
            HttpClient httpClient;

            if (string.IsNullOrWhiteSpace(config.ClientId))
            {
                httpClient = new HttpClientBuilder().WithBearerAuthorisationHeader(new JwtBearerTokenGenerator(config)).Build();
            }
            else
            {
                httpClient = new HttpClientBuilder().WithBearerAuthorisationHeader(new AzureADBearerTokenGenerator(config)).Build();
            }

            For<INotificationsApi>().Use<NotificationsApi>().Ctor<HttpClient>().Is(httpClient);
            For<INotificationsApiClientConfiguration>().Use(config);
        }

        private EmployerApprenticeshipsServiceConfiguration GetConfiguration()
        {
            var environment = Environment.GetEnvironmentVariable("DASENV");

            if (string.IsNullOrEmpty(environment))
            {
                environment = CloudConfigurationManager.GetSetting("EnvironmentName");
            }

            if (environment.Equals("LOCAL") || environment.Equals("AT") || environment.Equals("TEST"))
            {
                PopulateSystemDetails(environment);
            }

            var configurationRepository = GetConfigurationRepository();
            var configurationService = new ConfigurationService(configurationRepository, new ConfigurationOptions(ServiceName, environment, "1.0"));
            var configuration = configurationService.Get<EmployerApprenticeshipsServiceConfiguration>();

            return configuration;
        }

        private IConfigurationRepository GetConfigurationRepository()
        {
            IConfigurationRepository configurationRepository;

            if (bool.Parse(ConfigurationManager.AppSettings["LocalConfig"]))
            {
                configurationRepository = new FileStorageConfigurationRepository();
            }
            else
            {
                configurationRepository = new AzureTableStorageConfigurationRepository(CloudConfigurationManager.GetSetting("ConfigurationStorageConnectionString"));
            }

            return configurationRepository;
        }

        private void PopulateSystemDetails(string envName)
        {
            SystemDetailsViewModel.EnvironmentName = envName;
            SystemDetailsViewModel.VersionNumber = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        private void RegisterAuditService()
        {
            var environment = Environment.GetEnvironmentVariable("DASENV");

            if (string.IsNullOrEmpty(environment))
            {
                environment = CloudConfigurationManager.GetSetting("EnvironmentName");
            }

            For<IAuditMessageFactory>().Use<AuditMessageFactory>().Singleton();

            if (environment.Equals("LOCAL"))
            {
                For<IAuditApiClient>().Use<StubAuditApiClient>();
            }
            else
            {
                For<IAuditApiClient>().Use<AuditApiClient>();
            }
        }

        private void RegisterExecutionPolicies()
        {
            For<Infrastructure.ExecutionPolicies.ExecutionPolicy>()
                .Use<Infrastructure.ExecutionPolicies.CompaniesHouseExecutionPolicy>()
                .Named(Infrastructure.ExecutionPolicies.CompaniesHouseExecutionPolicy.Name);

            For<Infrastructure.ExecutionPolicies.ExecutionPolicy>()
                .Use<Infrastructure.ExecutionPolicies.HmrcExecutionPolicy>()
                .Named(Infrastructure.ExecutionPolicies.HmrcExecutionPolicy.Name);

            For<Infrastructure.ExecutionPolicies.ExecutionPolicy>()
                .Use<Infrastructure.ExecutionPolicies.IdamsExecutionPolicy>()
                .Named(Infrastructure.ExecutionPolicies.IdamsExecutionPolicy.Name);
        }

        private void RegisterLogger()
        {
            For<IRequestContext>().Use(c => new RequestContext(new HttpContextWrapper(HttpContext.Current)));
            For<ILog>().Use(c => new NLogLogger(c.ParentType, c.GetInstance<IRequestContext>(), null)).AlwaysUnique();
        }

        private void RegisterMapper()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(a => a.FullName.StartsWith("SFA.DAS.EAS"));
            var mappingProfiles = new List<Profile>();

            foreach (var assembly in assemblies)
            {
                var profiles = Assembly.Load(assembly.FullName).GetTypes()
                    .Where(t => typeof(Profile).IsAssignableFrom(t))
                    .Where(t => t.IsConcrete() && t.HasConstructors())
                    .Select(t => (Profile)Activator.CreateInstance(t));

                mappingProfiles.AddRange(profiles);
            }

            var config = new MapperConfiguration(cfg =>
            {
                mappingProfiles.ForEach(cfg.AddProfile);
            });

            var mapper = config.CreateMapper();

            For<IConfigurationProvider>().Use(config).Singleton();
            For<IMapper>().Use(mapper).Singleton();
        }

        private void RegisterMediator()
        {
            For<SingleInstanceFactory>().Use<SingleInstanceFactory>(ctx => t => ctx.GetInstance(t));
            For<MultiInstanceFactory>().Use<MultiInstanceFactory>(ctx => t => ctx.GetAllInstances(t));
            For<IMediator>().Use<Mediator>();
        }

        private void RegisterPostCodeAnywhereService()
        {
            For<IAddressLookupService>().Use<AddressLookupService>();
            For<IRestClientFactory>().Use<RestClientFactory>();
            For<IRestServiceFactory>().Use<RestServiceFactory>();
        }
    }
}