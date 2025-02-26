﻿using SFA.DAS.EmployerFinance.Web.Extensions;

namespace SFA.DAS.EmployerFinance.Web.UnitTests.Extensions;

[TestFixture]
public class HtmlHelperExtensionsTests
{
    [TestCaseSource(nameof(LabelCases))]
    public void WhenICallSetZenDeskLabelsWithLabels_ThenTheKeywordsAreCorrect(string[] labels, string keywords)
    {
        // Arrange
        var expected = $"<script type=\"text/javascript\">zE('webWidget', 'helpCenter:setSuggestions', {{ labels: [{keywords}] }});</script>";

        // Act
        var actual = ZenDeskLabelExtensions.SetZenDeskLabels(null, labels).ToString();

        // Assert
        actual.Should().Be(expected);
    }

    private static readonly object[] LabelCases =
    {
        new object[] { new string[] { "a string with multiple words", "the title of another page" }, "'a string with multiple words','the title of another page'"},
        new object[] { new string[] { "eas-estimate-apprenticeships-you-could-fund" }, "'eas-estimate-apprenticeships-you-could-fund'"},
        new object[] { new string[] { "eas-apostrophe's" }, @"'eas-apostrophe\'s'"},
        new object[] { new string[] { null }, "''" }
    };
}