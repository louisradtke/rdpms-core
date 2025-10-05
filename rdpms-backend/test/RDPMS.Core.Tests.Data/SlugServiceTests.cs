using System.Text.RegularExpressions;
using RDPMS.Core.Server.Services;

namespace RDPMS.Core.Tests.Data;

[TestFixture]
public class SlugServiceTests
{
    [TestCase("my.project")]
    [TestCase("my-project")]
    [TestCase("MyProject")]
    [TestCase("my._project")] // underscore and dot mixed should be allowed too
    [TestCase("my_project")]
    public void SlugRegex_Allows_Expected_Valid_Slugs(string slug)
    {
        var regex = SlugService.SlugRegex;
        Assert.That(regex.IsMatch(slug), Is.True);
    }
}