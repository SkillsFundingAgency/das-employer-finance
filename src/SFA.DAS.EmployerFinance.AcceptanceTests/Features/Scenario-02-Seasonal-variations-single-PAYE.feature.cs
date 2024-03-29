﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (https://www.specflow.org/).
//      SpecFlow Version:3.9.0.0
//      SpecFlow Generator Version:3.9.0.0
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
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.9.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("HMRC-Scenario-02-Seasonal-variations-single-PAYE")]
    public partial class HMRC_Scenario_02_Seasonal_Variations_Single_PAYEFeature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
        private static string[] featureTags = ((string[])(null));
        
#line 1 "Scenario-02-Seasonal-variations-single-PAYE.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "Features", "HMRC-Scenario-02-Seasonal-variations-single-PAYE", null, ProgrammingLanguage.CSharp, featureTags);
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.OneTimeTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public void TestTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<NUnit.Framework.TestContext>(NUnit.Framework.TestContext.CurrentContext);
        }
        
        public void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Month-02-submission")]
        public void Month_02_Submission()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Month-02-submission", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 3
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 4
 testRunner.Given("We have an account with a paye scheme", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
                TechTalk.SpecFlow.Table table19 = new TechTalk.SpecFlow.Table(new string[] {
                            "Id",
                            "LevyDueYtd",
                            "Payroll_Year",
                            "Payroll_Month",
                            "English_Fraction",
                            "SubmissionDate"});
                table19.AddRow(new string[] {
                            "999000201",
                            "10000",
                            "17-18",
                            "1",
                            "1",
                            "2017-05-15"});
                table19.AddRow(new string[] {
                            "999000202",
                            "20000",
                            "17-18",
                            "2",
                            "1",
                            "2017-06-15"});
                table19.AddRow(new string[] {
                            "999000203",
                            "18750",
                            "17-18",
                            "3",
                            "1",
                            "2017-07-15"});
#line 5
 testRunner.And("Hmrc return the following submissions for paye scheme", ((string)(null)), table19, "And ");
#line hidden
#line 10
 testRunner.When("we refresh levy data for paye scheme on the 08/2017", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 11
 testRunner.And("all the transaction lines in this scenario have had their transaction date update" +
                        "d to their created date", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 12
 testRunner.Then("we should see a level 1 screen with a balance of 20625 on the 07/2017", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 13
 testRunner.And("we should see a level 1 screen with a total levy of -1375 on the 07/2017", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 14
 testRunner.And("we should see a level 2 screen with a levy declared of -1250 on the 07/2017", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 15
 testRunner.And("we should see a level 2 screen with a top up of -125 on the 07/2017", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Month-02-submission-Checking-2nd-negative-declaration")]
        public void Month_02_Submission_Checking_2Nd_Negative_Declaration()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Month-02-submission-Checking-2nd-negative-declaration", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 17
this.ScenarioInitialize(scenarioInfo);
#line hidden
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 18
 testRunner.Given("We have an account with a paye scheme", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Given ");
#line hidden
                TechTalk.SpecFlow.Table table20 = new TechTalk.SpecFlow.Table(new string[] {
                            "Id",
                            "LevyDueYtd",
                            "Payroll_Year",
                            "Payroll_Month",
                            "English_Fraction",
                            "SubmissionDate"});
                table20.AddRow(new string[] {
                            "999000204",
                            "10000",
                            "17-18",
                            "1",
                            "1",
                            "2017-05-15"});
                table20.AddRow(new string[] {
                            "999000205",
                            "20000",
                            "17-18",
                            "2",
                            "1",
                            "2017-06-15"});
                table20.AddRow(new string[] {
                            "999000206",
                            "18750",
                            "17-18",
                            "3",
                            "1",
                            "2017-07-15"});
                table20.AddRow(new string[] {
                            "999000207",
                            "28750",
                            "17-18",
                            "4",
                            "1",
                            "2017-08-15"});
                table20.AddRow(new string[] {
                            "999000208",
                            "38750",
                            "17-18",
                            "5",
                            "1",
                            "2017-09-15"});
                table20.AddRow(new string[] {
                            "999000209",
                            "48750",
                            "17-18",
                            "6",
                            "1",
                            "2017-10-15"});
                table20.AddRow(new string[] {
                            "999000210",
                            "58750",
                            "17-18",
                            "7",
                            "1",
                            "2017-11-15"});
                table20.AddRow(new string[] {
                            "999000211",
                            "68750",
                            "17-18",
                            "8",
                            "1",
                            "2017-12-15"});
                table20.AddRow(new string[] {
                            "999000212",
                            "67500",
                            "17-18",
                            "9",
                            "1",
                            "2018-01-15"});
                table20.AddRow(new string[] {
                            "999000213",
                            "77500",
                            "17-18",
                            "10",
                            "1",
                            "2018-02-15"});
                table20.AddRow(new string[] {
                            "999000214",
                            "87500",
                            "17-18",
                            "11",
                            "1",
                            "2018-03-15"});
                table20.AddRow(new string[] {
                            "999000215",
                            "97500",
                            "17-18",
                            "12",
                            "1",
                            "2018-04-15"});
#line 19
 testRunner.And("Hmrc return the following submissions for paye scheme", ((string)(null)), table20, "And ");
#line hidden
#line 33
 testRunner.When("we refresh levy data for paye scheme on the 05/2018", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 34
 testRunner.And("all the transaction lines in this scenario have had their transaction date update" +
                        "d to their created date", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 35
 testRunner.Then("we should see a level 1 screen with a balance of 107250 on the 04/2018", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
#line 36
 testRunner.And("we should see a level 1 screen with a total levy of -1375 on the 01/2018", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 37
 testRunner.And("we should see a level 2 screen with a levy declared of -1250 on the 01/2018", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
#line 38
 testRunner.And("we should see a level 2 screen with a top up of -125 on the 01/2018", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "And ");
#line hidden
            }
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
