using RDPMS.Core.Persistence.Model;

namespace RDPMS.Core.Tests.Data;

[TestFixture]
public class ModelTests
{
    [TestCase("https://s3.amazonaws.com")]
    [TestCase("http://localhost:9000")]
    [TestCase("http://127.0.0.1:9000")]
    public void S3Store_Regex_PositiveMatch_Test(string value)
    {
        Assert.That(S3DataStore.UrlRegex.IsMatch(value), Is.True);
    }

    [TestCase("s3.amazonaws.com")]
    [TestCase("ftp://localhost:9000")]
    [TestCase("http://:9000")]
    [TestCase("http://127.0.0.1:9000/")]
    public void S3Store_Regex_NoMatch_Test(string value)
    {
        Assert.That(S3DataStore.UrlRegex.IsMatch(value), Is.False);
    }
}