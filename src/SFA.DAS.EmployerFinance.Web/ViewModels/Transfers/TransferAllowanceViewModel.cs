﻿namespace SFA.DAS.EmployerFinance.Web.ViewModels.Transfers;

public class TransferAllowanceViewModel
{
    public decimal RemainingTransferAllowance { get; set; }
    public decimal StartingTransferAllowance { get; set; }
    public decimal TransferAllowancePercentage { get => _transferAllowancePercentage * 100; set => _transferAllowancePercentage = value; }
    private decimal _transferAllowancePercentage;
}