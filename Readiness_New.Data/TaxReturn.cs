namespace Readiness_New.Data;

public class TaxReturn
{
    public string TaxpayerId { get; set; } 
    public int TaxYear { get; set; } 
    public string FilingStatus { get; set; }
    public decimal TotalIncome { get; set; } 
    public decimal TotalDeductions { get; set; }
    public decimal TaxableIncome { get; set; } 
    public decimal TotalTax { get; set; } 
    public decimal WithholdingAmount { get; set; }
    public bool HasDependents { get; set; }
    public int NumberOfDependents { get; set; }
    public Form1040 Form1040 { get; set; } 
    public ScheduleA ScheduleA { get; set; }
    public ScheduleC ScheduleC { get; set; }
    public List<W2Form> W2Forms { get; set; }
    public DateTime? FilingDate { get; set; }
}

public class Form1040
{
    public string Name { get; set; }
    public string SSN { get; set; }
    public string Address { get; set; }
    public decimal Line1_Wages { get; set; } 
    public decimal Line2_Interest { get; set; }
    public decimal Line3_Dividends { get; set; }
    public decimal Line8_TotalIncome { get; set; }
}

public class ScheduleA
{
    public decimal MedicalExpenses { get; set; } 
    public decimal StateAndLocalTaxes { get; set; }
    public decimal MortgageInterest { get; set; }
    public decimal CharitableContributions { get; set; } 
    public decimal TotalItemizedDeductions { get; set; }
}

public class ScheduleC
{
    public string BusinessName { get; set; }
    public decimal GrossReceipts { get; set; } 
    public decimal TotalExpenses { get; set; }
    public decimal NetProfit { get; set; }
}

public class W2Form
{
    public string EmployerName { get; set; }
    public string EmployerEIN { get; set; }
    public decimal Wages { get; set; } 
    public decimal FederalTaxWithheld { get; set; }
    public decimal SocialSecurityWages { get; set; }
}