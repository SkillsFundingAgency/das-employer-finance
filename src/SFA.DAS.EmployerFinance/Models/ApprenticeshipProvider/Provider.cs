﻿namespace SFA.DAS.EmployerFinance.Models.ApprenticeshipProvider;

public class Provider
{
    public int Id { get; set; }
    public string Name { get; set; }
    public long Ukprn { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public bool NationalProvider { get; set; }
    public bool IsHistoricProviderName { get; set; }
}