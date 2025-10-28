using RDPMS.Core.Persistence.Model;
using RDPMS.Core.Server.Model.Mappers;

namespace RDPMS.Core.Tests.Data;

[TestFixture]
public class MapperTests
{
    [TestCase("file.py")]
    [TestCase("dir/file.py")]
    public void PathSanitizationCheck_Pass_Test(string value)
    {
        Assert.That(FileCreateRequestDTOMapper.CheckIfPathIsValid(value), Is.True);
    }
    
    [TestCase("./dir/file.py")] // we don't allow '.' parts
    [TestCase("dir//file.py")]
    [TestCase("/dir/file.py")]
    [TestCase("./../my_secret.txt")]
    [TestCase("..")]
    public void PathSanitizationCheck_False_Test(string value)
    {
        Assert.That(FileCreateRequestDTOMapper.CheckIfPathIsValid(value), Is.False);
    }
}
