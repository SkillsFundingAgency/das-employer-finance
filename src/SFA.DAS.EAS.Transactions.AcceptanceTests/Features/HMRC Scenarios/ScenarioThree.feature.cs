﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:2.1.0.0
//      SpecFlow Generator Version:2.0.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace SFA.DAS.EAS.Transactions.AcceptanceTests.Features.HMRCScenarios
{
    using TechTalk.SpecFlow;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "2.1.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("ScenarioThree - Multiple EPS submissions for month within submission window")]
    public partial class ScenarioThree_MultipleEPSSubmissionsForMonthWithinSubmissionWindowFeature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
#line 1 "ScenarioThree.feature"
#line hidden
        
        [NUnit.Framework.TestFixtureSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "ScenarioThree - Multiple EPS submissions for month within submission window", null, ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.TestFixtureTearDownAttribute()]
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
        [NUnit.Framework.DescriptionAttribute("Month three submission (Multiple submission month)")]
        public virtual void MonthThreeSubmissionMultipleSubmissionMonth()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Month three submission (Multiple submission month)", ((string[])(null)));
#line 3
this.ScenarioSetup(scenarioInfo);
#line 4
 testRunner.Given("I have an account", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                        "Paye_scheme",
                        "LevyDueYtd",
                        "Payroll_Year",
                        "Payroll_Month",
                        "English_Fraction",
                        "SubmissionDate",
                        "CreatedDate"});
            table1.AddRow(new string[] {
                        "123/ABC",
                        "10000",
                        "17-18",
                        "1",
                        "1",
                        "2017-05-15",
                        "2017-05-23"});
            table1.AddRow(new string[] {
                        "123/ABC",
                        "20000",
                        "17-18",
                        "2",
                        "1",
                        "2017-06-15",
                        "2017-06-23"});
            table1.AddRow(new string[] {
                        "123/ABC",
                        "35000",
                        "17-18",
                        "3",
                        "1",
                        "2017-07-15",
                        "2017-07-23"});
            table1.AddRow(new string[] {
                        "123/ABC",
                        "25000",
                        "17-18",
                        "3",
                        "1",
                        "2017-07-16",
                        "2017-07-23"});
            table1.AddRow(new string[] {
                        "123/ABC",
                        "30000",
                        "17-18",
                        "3",
                        "1",
                        "2017-07-17",
                        "2017-07-23"});
#line 5
 testRunner.When("I have the following submissions", ((string)(null)), table1, "When ");
#line 12
 testRunner.Then("the balance on 07/2017 should be 33000 on the screen", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 13
 testRunner.And("the total levy shown for month 07/2017 should be 11000", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 14
 testRunner.And("For month 07/2017 the levy declared should be 10000 and the topup should be 1000", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Month four submission (Month after multiple submissions)")]
        public virtual void MonthFourSubmissionMonthAfterMultipleSubmissions()
        {
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Month four submission (Month after multiple submissions)", ((string[])(null)));
#line 16
this.ScenarioSetup(scenarioInfo);
#line 17
 testRunner.Given("I have an account", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
            TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                        "Paye_scheme",
                        "LevyDueYtd",
                        "Payroll_Year",
                        "Payroll_Month",
                        "English_Fraction",
                        "SubmissionDate",
                        "CreatedDate"});
            table2.AddRow(new string[] {
                        "123/ABC",
                        "10000",
                        "17-18",
                        "1",
                        "1",
                        "2017-05-15",
                        "2017-05-23"});
            table2.AddRow(new string[] {
                        "123/ABC",
                        "20000",
                        "17-18",
                        "2",
                        "1",
                        "2017-06-15",
                        "2017-06-23"});
            table2.AddRow(new string[] {
                        "123/ABC",
                        "35000",
                        "17-18",
                        "3",
                        "1",
                        "2017-07-15",
                        "2017-07-23"});
            table2.AddRow(new string[] {
                        "123/ABC",
                        "25000",
                        "17-18",
                        "3",
                        "1",
                        "2017-07-16",
                        "2017-07-23"});
            table2.AddRow(new string[] {
                        "123/ABC",
                        "30000",
                        "17-18",
                        "3",
                        "1",
                        "2017-07-17",
                        "2017-07-23"});
            table2.AddRow(new string[] {
                        "123/ABC",
                        "40000",
                        "17-18",
                        "4",
                        "1",
                        "2017-08-17",
                        "2017-08-23"});
#line 18
 testRunner.When("I have the following submissions", ((string)(null)), table2, "When ");
#line 26
 testRunner.Then("the balance on 08/2017 should be 44000 on the screen", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line 27
 testRunner.And("the total levy shown for month 08/2017 should be 11000", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line 28
 testRunner.And("For month 08/2017 the levy declared should be 10000 and the topup should be 1000", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
