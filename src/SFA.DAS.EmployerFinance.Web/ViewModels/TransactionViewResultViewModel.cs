﻿using SFA.DAS.EAS.Account.Api.Types;

namespace SFA.DAS.EmployerFinance.Web.ViewModels;

public class TransactionViewResultViewModel
{
    private readonly int _currentYear;
    private readonly int _currentMonth;

    public TransactionViewResultViewModel(DateTime? currentTime)
    {
        CurrentTime = currentTime ?? DateTime.Now;
        _currentMonth = currentTime?.Month ?? DateTime.Now.Month;
        _currentYear = currentTime?.Year ?? DateTime.Now.Year;
    }

    public DateTime CurrentTime { get; }
    public AccountDetailViewModel Account { get; set; }
    public TransactionViewModel Model { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }

    public bool IsLatestMonth => _currentYear <= Year && _currentMonth <= Month;
    public bool IsFirstMonthOfLevy => Year <= 2017 && Month <= 4;
    public bool AccountHasPreviousTransactions { get; set; }
}