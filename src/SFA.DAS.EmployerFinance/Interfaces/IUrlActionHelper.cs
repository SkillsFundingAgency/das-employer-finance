namespace SFA.DAS.EmployerFinance.Interfaces
{
    public interface IUrlActionHelper
    {
        string EmployerAccountsAction(string path);
        string EmployerCommitmentsV2Action(string path);
        string LevyTransfersMatchingAction(string path);
        string ReservationsAction(string path);
        string EmployerFinanceAction(string path);
        string EmployerProjectionsAction(string path);
        string EmployerRecruitAction(string path = "");
        string LegacyEasAccountAction(string path);
        string LegacyEasAction(string path);
    }
}
