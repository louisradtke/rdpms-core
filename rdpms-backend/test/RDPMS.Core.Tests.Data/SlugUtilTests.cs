using System.Text.RegularExpressions;
using RDPMS.Core.Persistence;
using RDPMS.Core.Server.Services;

namespace RDPMS.Core.Tests.Data;

[TestFixture]
public class SlugUtilTests
{
    [TestCase("my.project")]
    [TestCase("my-project")]
    [TestCase("MyProject")]
    [TestCase("my._project")] // underscore and dot mixed should be allowed too
    [TestCase("my_project")]
    public void SlugRegex_Allows_Expected_Valid_Slugs(string slug)
    {
        var regex = SlugUtil.SlugRegex;
        Assert.That(regex.IsMatch(slug), Is.True);
    }

    [TestCase]
    public void SlugRegex_Rejects()
    {
        var shortSlug = string.Join("", Enumerable.Range(0, 64).Select(_ => "a"));
        var longSlug = string.Join("", Enumerable.Range(0, 65).Select(_ => "a"));

        var regex = SlugUtil.SlugRegex;
        using (Assert.EnterMultipleScope())
        {
            Assert.That(regex.IsMatch(shortSlug), Is.True);
            Assert.That(regex.IsMatch(longSlug), Is.False);
        }
    }

}