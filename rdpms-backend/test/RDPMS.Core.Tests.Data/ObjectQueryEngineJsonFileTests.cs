using System.Text.Json;
using Corvus.Json;
using RDPMS.Core.Contracts.Schemas;
using RDPMS.Core.QueryEngine;

namespace RDPMS.Core.Tests.Data;

[TestFixture]
public class ObjectQueryEngineJsonFileTests
{
    private const string StrictTopicQueryFixture = "query.strict-odom-and-image.json";
    private const string ContainsAllQueryFixture = "query.contains-all-topics.json";
    private const string AllElemMatchQueryFixture = "query.all-elem-match-topics.json";
    private const string MinTwoTopicsInFileQueryFixture = "query.min-two-topics-in-file.json";
    private const string TypedOdomQueryFixture = "query.typed-odom.json";
    private const string ImageTypedQueryFixture = "query.image-typed.json";

    private static readonly (string QueryFileName, string FileName, bool ShouldMatch)[] FileInformationCases =
    [
        (StrictTopicQueryFixture, "file-information.full.json", true),
        (StrictTopicQueryFixture, "file-information.missing-image.json", false),
        (StrictTopicQueryFixture, "file-information.no-timeseries.json", false),

        (ContainsAllQueryFixture, "file-information.full.json", true),
        (ContainsAllQueryFixture, "file-information.missing-image.json", false),
        (ContainsAllQueryFixture, "file-information.no-timeseries.json", false),

        (AllElemMatchQueryFixture, "file-information.full.json", true),
        (AllElemMatchQueryFixture, "file-information.missing-image.json", false),
        (AllElemMatchQueryFixture, "file-information.no-timeseries.json", false),

        (MinTwoTopicsInFileQueryFixture, "file-information.full.json", true),
        (MinTwoTopicsInFileQueryFixture, "file-information.missing-image.json", false),
        (MinTwoTopicsInFileQueryFixture, "file-information.no-timeseries.json", false),
    ];

    private static readonly (string QueryFileName, string FileName, bool ShouldMatch)[] TimeSeriesCases =
    [
        (TypedOdomQueryFixture, "time-series-container.full.json", true),
        (TypedOdomQueryFixture, "time-series-container.missing-image.json", true),

        (ImageTypedQueryFixture, "time-series-container.full.json", true),
        (ImageTypedQueryFixture, "time-series-container.missing-image.json", false),
    ];

    [Test]
    public void StrictQueryFixture_IsValidObjectDsl()
    {
        using var queryDoc = LoadFixture(StrictTopicQueryFixture);
        var query = new ObjectQueryDslV1Schema(queryDoc.RootElement);

        Assert.That(query.IsValid(), Is.True, "Query fixture must validate against ObjectQueryDslV1Schema.");
    }

    [Test]
    public void StrictQueryFixture_ParsesIntoAst()
    {
        using var queryDoc = LoadFixture(StrictTopicQueryFixture);
        var query = new ObjectQueryDslV1Schema(queryDoc.RootElement);
        var engine = ObjectQueryEngine.CreateDefault();

        var ast = engine.ParseToAst(query);

        Assert.That(ast, Is.Not.Null);
    }

    [TestCase(ContainsAllQueryFixture)]
    [TestCase(AllElemMatchQueryFixture)]
    public void ExtendedOperatorFixtures_ParseIntoAst(string queryFileName)
    {
        using var queryDoc = LoadFixture(queryFileName);
        var query = new ObjectQueryDslV1Schema(queryDoc.RootElement);
        var engine = ObjectQueryEngine.CreateDefault();

        var ast = engine.ParseToAst(query);

        Assert.That(ast, Is.Not.Null);
    }

    [TestCaseSource(nameof(FileInformationCases))]
    public void FileInformationCases_MatchExpectedDocuments((string QueryFileName, string FileName, bool ShouldMatch) testCase)
    {
        using var queryDoc = LoadFixture(testCase.QueryFileName);
        var query = new ObjectQueryDslV1Schema(queryDoc.RootElement);

        using var document = LoadFixture(testCase.FileName);
        var fileInformation = new FileInformationV1Schema(document.RootElement);

        Assert.That(fileInformation.IsValid(), Is.True,
            $"Fixture '{testCase.FileName}' must validate against FileInformationV1Schema.");

        var engine = ObjectQueryEngine.CreateDefault();
        var isMatch = engine.IsMatch(query, document.RootElement);

        Assert.That(isMatch, Is.EqualTo(testCase.ShouldMatch),
            $"Unexpected match result for query '{testCase.QueryFileName}' and fixture '{testCase.FileName}'.");
    }

    [TestCaseSource(nameof(TimeSeriesCases))]
    public void ImageTypedQueryFixture_MatchExpectedDocuments((string QueryFileName, string FileName, bool ShouldMatch) testCase)
    {
        using var queryDoc = LoadFixture(testCase.QueryFileName);
        var query = new ObjectQueryDslV1Schema(queryDoc.RootElement);

        using var document = LoadFixture(testCase.FileName);
        var fileInformation = new TimeSeriesContainerV1Schema(document.RootElement);

        Assert.That(fileInformation.IsValid(), Is.True,
            $"Fixture '{testCase.FileName}' must validate against TimeSeriesContainerV1Schema.");

        var engine = ObjectQueryEngine.CreateDefault();
        var isMatch = engine.IsMatch(query, document.RootElement);

        Assert.That(isMatch, Is.EqualTo(testCase.ShouldMatch),
            $"Unexpected match result for query '{testCase.QueryFileName}' and fixture '{testCase.FileName}'.");
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
