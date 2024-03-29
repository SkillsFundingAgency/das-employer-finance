﻿namespace SFA.DAS.EmployerFinance.Interfaces;

public interface IUrlActionHelper
{
    string EmployerAccountsAction(string path, bool withAccountContext = true);
    string EmployerCommitmentsV2Action(string path);
    string LevyTransfersMatchingAccountAction(string path, bool withAccountContext = true);
    
    string EmployerFinanceAction(string path);
    string EmployerProjectionsAction(string path);
    
    string LegacyEasAction(string path);
}