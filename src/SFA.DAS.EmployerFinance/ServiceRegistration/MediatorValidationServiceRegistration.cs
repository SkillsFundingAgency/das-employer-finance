using SFA.DAS.EmployerFinance.Queries.FindAccountCoursePayments;
using SFA.DAS.EmployerFinance.Queries.GetEmployerAccountDetail;
using SFA.DAS.EmployerFinance.Queries.GetPayeSchemeByRef;
using SFA.DAS.EmployerFinance.Validation;

namespace SFA.DAS.EmployerFinance.ServiceRegistration;

public static class MediatorValidationServiceRegistration
{
    public static IServiceCollection AddMediatorValidators(this IServiceCollection services)
    {
        services.AddTransient<IValidator<GetPayeSchemeByRefQuery>, GetPayeSchemeByRefValidator>();
        services.AddTransient<IValidator<GetEmployerAccountDetailByHashedIdQuery>, GetEmployerAccountDetailByHashedIdValidator>();
        services.AddTransient<IValidator<FindAccountCoursePaymentsQuery>, FindAccountCoursePaymentsQueryValidator>();
            
        return services;
    }
}