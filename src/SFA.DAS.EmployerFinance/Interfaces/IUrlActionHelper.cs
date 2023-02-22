namespace SFA.DAS.EmployerFinance.Interfaces;

public interface IUrlActionHelper
{
    string EmployerAccountsAction(string path);
    string EmployerCommitmentsV2Action(string path);
    string LevyTransfersMatchingAccountAction(string path);
    
    string EmployerFinanceAction(string path);
    string EmployerProjectionsAction(string path);
    
    string LegacyEasAction(string path);
}