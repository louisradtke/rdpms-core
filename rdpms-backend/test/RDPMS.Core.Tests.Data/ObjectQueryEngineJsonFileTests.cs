using System.Text.Json;
using Corvus.Json;
using RDPMS.Core.Contracts.Schemas;
using RDPMS.Core.QueryEngine;

namespace RDPMS.Core.Tests.Data;

[TestFixture]
public class ObjectQueryEngineJsonFileTests
{
    private const string QueryFixture = "query.strict-odom-and-image.json";

    private static readonly (string FileName, bool ShouldMatch)[] MatchCases =
    [
        ("file-information.match.json", true),
        ("file-information.missing-image.json", false),
        ("file-information.no-timeseries.json", false),
    ];

    [Test]
    public void StrictQueryFixture_IsValidObjectDsl()
    {
        using var queryDoc = LoadFixture(QueryFixture);
        var query = new ObjectQueryDslV1Schema(queryDoc.RootElement);

        Assert.That(query.IsValid(), Is.True, "Query fixture must validate against ObjectQueryDslV1Schema.");
    }

    [Test]
    public void StrictQueryFixture_ParsesIntoAst()
    {
        using var queryDoc = LoadFixture(QueryFixture);
        var query = new ObjectQueryDslV1Schema(queryDoc.RootElement);
        var engine = ObjectQueryEngine.CreateDefault();

        var ast = engine.ParseToAst(query);

        Assert.That(ast, Is.Not.Null);
    }

    [TestCaseSource(nameof(MatchCases))]
    public void StrictQueryFixture_MatchesExpectedDocuments((string FileName, bool ShouldMatch) testCase)
    {
        using var queryDoc = LoadFixture(QueryFixture);
        var query = new ObjectQueryDslV1Schema(queryDoc.RootElement);

        using var document = LoadFixture(testCase.FileName);
        var fileInformation = new FileInformationV1Schema(document.RootElement);

        Assert.That(fileInformation.IsValid(), Is.True,
            $"Fixture '{testCase.FileName}' must validate against FileInformationV1Schema.");

        var engine = ObjectQueryEngine.CreateDefault();
        var isMatch = engine.IsMatch(query, document.RootElement);

        Assert.That(isMatch, Is.EqualTo(testCase.ShouldMatch),
            $"Unexpected match result for fixture '{testCase.FileName}'.");
    }

    private static JsonDocument LoadFixture(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "QueryEngineFixtures", fileName);
        if (!File.Exists(path))
        {
            Assert.Fail($"Fixture file not found: {path}");
        }

        var json = File.ReadAllText(path);
        return JsonDocument.Parse(json);
    }
}
