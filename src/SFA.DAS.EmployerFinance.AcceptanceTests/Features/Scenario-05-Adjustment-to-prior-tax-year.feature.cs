﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:2.3.0.0
//      SpecFlow Generator Version:2.3.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace SFA.DAS.EmployerFinance.AcceptanceTests.Features
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.3.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("HMRC-Scenario-05-Adjustment-to-prior-tax-year")]
    public partial class HMRC_Scenario_05_Adjustment_To_Prior_Tax_YearFeature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "Scenario-05-Adjustment-to-prior-tax-year.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "HMRC-Scenario-05-Adjustment-to-prior-tax-year", null, ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.OneTimeTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void ScenarioTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioSetup(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioStart(scenarioInfo);
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("End-of-year-adjustment")]
        public virtual void End_Of_Year_Adjustment()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("End-of-year-adjustment", ((string[])(null)));
#line 3
this.ScenarioSetup(scenarioInfo);
#line 4
 testRunner.Given("We have an account with a paye scheme", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Id",
                        "LevyDueYtd",
                        "Payroll_Year",
                        "Payroll_Month",
                        "English_Fraction",
                        "SubmissionDate",
                        "CreatedDate"});
            table1.AddRow(new string[] {
                        "999000501",
                        "11250",
                        "17-18",
                        "1",
                        "1",
                        "2017-05-15",
                        "2017-05-23"});
            table1.AddRow(new string[] {
                        "999000502",
                        "22500",
                        "17-18",
                        "2",
                        "1",
                        "2017-06-15",
                        "2017-06-23"});
            table1.AddRow(new string[] {
                        "999000503",
                        "33750",
                        "17-18",
                        "3",
                        "1",
                        "2017-07-15",
                        "2017-07-23"});
            table1.AddRow(new string[] {
                        "999000504",
                        "45000",
                        "17-18",
                        "4",
                        "1",
                        "2017-08-15",
                        "2017-08-23"});
            table1.AddRow(new string[] {
                        "999000505",
                        "56250",
                        "17-18",
                        "5",
                        "1",
                        "2017-09-15",
                        "2017-09-23"});
            table1.AddRow(new string[] {
                        "999000506",
                        "67500",
                        "17-18",
                        "6",
                        "1",
                        "2017-10-15",
                        "2017-10-23"});
            table1.AddRow(new string[] {
                        "999000507",
                        "78750",
                        "17-18",
                        "7",
                        "1",
                        "2017-11-15",
                        "2017-11-23"});
            table1.AddRow(new string[] {
                        "999000508",
                        "90000",
                        "17-18",
                        "8",
                        "1",
                        "2017-12-15",
                        "2017-12-23"});
            table1.AddRow(new string[] {
                        "999000509",
                        "101250",
                        "17-18",
                        "9",
                        "1",
                        "2018-01-15",
                        "2018-01-23"});
            table1.AddRow(new string[] {
                        "999000510",
                        "112500",
                        "17-18",
                        "10",
                        "1",
                        "2018-02-15",
                        "2018-02-23"});
            table1.AddRow(new string[] {
                        "999000511",
                        "123750",
                        "17-18",
                        "11",
                        "1",
                        "2018-03-15",
                        "2018-03-23"});
            table1.AddRow(new string[] {
                        "999000512",
                        "135000",
                        "17-18",
                        "12",
                        "1",
                        "2018-04-15",
                        "2018-04-23"});
            table1.AddRow(new string[] {
                        "999000513",
                        "10000",
                        "18-19",
                        "1",
                        "1",
                        "2018-05-15",
                        "2018-05-23"});
#line 5
 testRunner.And("Hmrc return the following submissions for paye scheme", ((string)(null)), table1, "And ");
#line 20
 testRunner.When("we refresh levy data for paye scheme", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 21
 testRunner.And("all the transaction lines in this scenario have had their transaction date update" +
                    "d to the specified created date", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "Id",
                        "LevyDueYtd",
                        "Payroll_Year",
                        "Payroll_Month",
                        "English_Fraction",
                        "SubmissionDate",
                        "CreatedDate"});
            table2.AddRow(new string[] {
                        "999000514",
                        "120000",
                        "17-18",
                        "12",
                        "1",
                        "2018-06-10",
                        "2018-06-23"});
            table2.AddRow(new string[] {
                        "999000515",
                        "20000",
                        "18-19",
                        "2",
                        "1",
                        "2018-06-15",
                        "2018-06-23"});
#line 22
 testRunner.Given("Hmrc return the following submissions for paye scheme", ((string)(null)), table2, "Given ");
#line 26
 testRunner.When("we refresh levy data for paye scheme", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line 27
 testRunner.And("all the transaction lines in this scenario have had their transaction date update" +
                    "d to the specified created date", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 28
 testRunner.Then("we should see a level 1 screen with a balance of 154000 on the 06/2018", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 29
 testRunner.And("we should see a level 1 screen with a total levy of -5500 on the 06/2018", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 30
 testRunner.And("we should see a level 2 screen with a levy declared of -5000 on the 06/2018", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 31
 testRunner.And("we should see a level 2 screen with a top up of -500 on the 06/2018", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
