﻿using SFA.DAS.EmployerFinance.Models.Levy;

namespace SFA.DAS.EmployerFinance.Models.HmrcLevy;

public class EmployerLevyData
{
    public EmployerLevyData()
    {
        Declarations = new DasDeclarations {Declarations = new List<DasDeclaration>()};
    }
    public string EmpRef { get; set; }

    public DasDeclarations Declarations { get; set; }
        
}