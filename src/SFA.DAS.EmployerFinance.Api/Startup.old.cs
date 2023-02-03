//MAP-192 Need to delete it

//using System.Configuration;
//using SFA.DAS.EmployerFinance.Api;

//namespace SFA.DAS.EmployerFinance.Api
//{
//    public class Startup
//    {
//        public void Configuration(IAppBuilder app)
//        {
//            _ = app.UseWindowsAzureActiveDirectoryBearerAuthentication(new WindowsAzureActiveDirectoryBearerAuthenticationOptions
//            {
//                Tenant = ConfigurationManager.AppSettings["idaTenant"],
//                TokenValidationParameters = new System.IdentityModel.Tokens.TokenValidationParameters
//                {
//                    RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
//                    ValidAudiences = ConfigurationManager.AppSettings["FinanceApiIdaAudience"].ToString().Split(',')
//                }
//            });
//        }
//    }
//}
