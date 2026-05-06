using System.Text.Json;
using RDPMS.Core.Infra;
using RDPMS.Core.Persistence.MetadataProjection;
using RDPMS.Core.Persistence.Model;

namespace RDPMS.Core.Tests.Data;

[TestFixture]
public class MetadataProjectionTests
{
    [Test]
    public void Registry_DiscoversDatasetTimeSeriesProjection()
    {
        var registry = new CachedMetadataProjectionRegistry();

        var descriptor = registry.GetForEntity(typeof(DataSet), RDPMSMetadataKeys.TimeSeriesData)
            .Single();

        Assert.Multiple(() =>
        {
            Assert.That(descriptor.Id, Is.EqualTo("dataset.timeseries.v1"));
            Assert.That(descriptor.SourceKey, Is.EqualTo(RDPMSMetadataKeys.TimeSeriesData));
            Assert.That(descriptor.RequiredSchemaId, Is.EqualTo(RDPMSSchemaIds.TimeSeriesContainerV1));
            Assert.That(descriptor.StrategyType, Is.EqualTo(typeof(TimeSeriesDatasetProjectionStrategy)));
            Assert.That(descriptor.Outputs.Keys, Is.EquivalentTo(new[]
            {
                TimeSeriesDatasetProjectionStrategy.IsTimeSeriesOutput,
                TimeSeriesDatasetProjectionStrategy.BeginStampOutput,
                TimeSeriesDatasetProjectionStrategy.EndStampOutput
            }));
        });
    }

    [Test]
    public async Task TimeSeriesStrategy_SourceMissing_ClearsProjection()
    {
        var dataSet = new DataSet("test")
        {
            IsTimeSeries = true,
            BeginStamp = DateTime.UtcNow.AddHours(-1),
            EndStamp = DateTime.UtcNow
        };
        var descriptor = GetTimeSeriesDescriptor();
        var writer = new MetadataProjectionWriter(descriptor);
        var strategy = new TimeSeriesDatasetProjectionStrategy();

        await strategy.RefreshAsync(dataSet, MetadataProjectionSource.Missing, writer, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(dataSet.IsTimeSeries, Is.False);
            Assert.That(dataSet.BeginStamp, Is.Null);
            Assert.That(dataSet.EndStamp, Is.Null);
        });
    }

    [Test]
    public async Task TimeSeriesStrategy_ValidDocument_ExtractsDatasetRange()
    {
        var dataSet = new DataSet("test");
        var descriptor = GetTimeSeriesDescriptor();
        var writer = new MetadataProjectionWriter(descriptor);
        var strategy = new TimeSeriesDatasetProjectionStrategy();

        using var document = JsonDocument.Parse("""
        {
          "topics": [
            {
              "name": "/late",
              "metadata": {
                "firstMessageTimestamp": "2026-01-01T10:00:00Z",
                "lastMessageTimestamp": "2026-01-01T10:30:00Z"
              },
              "messageType": {
                "name": "std_msgs/msg/String",
                "reference": "package://std_msgs/msg/String",
                "fields": []
              }
            },
            {
              "name": "/early",
              "metadata": {
                "firstMessageTimestamp": "2026-01-01T09:00:00Z",
                "lastMessageTimestamp": "2026-01-01T11:00:00Z"
              },
              "messageType": {
                "name": "std_msgs/msg/String",
                "reference": "package://std_msgs/msg/String",
                "fields": []
              }
            }
          ]
        }
        """);

        var source = new MetadataProjectionSource(
            true,
            document.RootElement,
            Guid.NewGuid(),
            DateTime.UtcNow);
        await strategy.RefreshAsync(dataSet, source, writer, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(dataSet.IsTimeSeries, Is.True);
            Assert.That(dataSet.BeginStamp, Is.EqualTo(DateTimeOffset.Parse("2026-01-01T09:00:00Z").UtcDateTime));
            Assert.That(dataSet.EndStamp, Is.EqualTo(DateTimeOffset.Parse("2026-01-01T11:00:00Z").UtcDateTime));
        });
    }

    [Test]
    public async Task TimeSeriesStrategy_InvalidDocument_KeepsTimeSeriesButClearsRange()
    {
        var dataSet = new DataSet("test")
        {
            BeginStamp = DateTime.UtcNow.AddHours(-1),
            EndStamp = DateTime.UtcNow
        };
        var descriptor = GetTimeSeriesDescriptor();
        var writer = new MetadataProjectionWriter(descriptor);
        var strategy = new TimeSeriesDatasetProjectionStrategy();

        using var document = JsonDocument.Parse("""{"topics":[{"name":"/missing-required-fields"}]}""");
        var source = new MetadataProjectionSource(
            true,
            document.RootElement,
            Guid.NewGuid(),
            DateTime.UtcNow);
        await strategy.RefreshAsync(dataSet, source, writer, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(dataSet.IsTimeSeries, Is.True);
            Assert.That(dataSet.BeginStamp, Is.Null);
            Assert.That(dataSet.EndStamp, Is.Null);
        });
    }

    private static CachedMetadataProjectionDescriptor GetTimeSeriesDescriptor()
    {
        return new CachedMetadataProjectionRegistry()
            .GetForEntity(typeof(DataSet), RDPMSMetadataKeys.TimeSeriesData)
            .Single();
    }
}
