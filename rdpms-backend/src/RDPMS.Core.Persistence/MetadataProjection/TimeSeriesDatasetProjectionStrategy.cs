using System.Globalization;
using System.Text.Json;
using Corvus.Json;
using RDPMS.Core.Contracts.Schemas;
using RDPMS.Core.Persistence.Model;

namespace RDPMS.Core.Persistence.MetadataProjection;

public sealed class TimeSeriesDatasetProjectionStrategy : MetadataProjectionStrategy<DataSet>
{
    public const string IsTimeSeriesOutput = "isTimeSeries";
    public const string BeginStampOutput = "beginStamp";
    public const string EndStampOutput = "endStamp";

    public override string Version => "timeseries-dataset-projection.v1";

    public override ValueTask RefreshAsync(
        DataSet entity,
        MetadataProjectionSource source,
        MetadataProjectionWriter writer,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!source.Exists)
        {
            writer.SetOutput(entity, IsTimeSeriesOutput, false);
            writer.SetOutput(entity, BeginStampOutput, null);
            writer.SetOutput(entity, EndStampOutput, null);
            return ValueTask.CompletedTask;
        }

        writer.SetOutput(entity, IsTimeSeriesOutput, true);

        if (source.Document is not JsonElement document)
        {
            writer.SetOutput(entity, BeginStampOutput, null);
            writer.SetOutput(entity, EndStampOutput, null);
            return ValueTask.CompletedTask;
        }

        var container = new TimeSeriesContainerV1Schema(document);
        var validationResult = container.Validate(ValidationContext.ValidContext);
        if (!validationResult.IsValid)
        {
            writer.SetOutput(entity, BeginStampOutput, null);
            writer.SetOutput(entity, EndStampOutput, null);
            return ValueTask.CompletedTask;
        }

        var (beginStamp, endStamp) = ExtractRange(document);
        writer.SetOutput(entity, BeginStampOutput, beginStamp);
        writer.SetOutput(entity, EndStampOutput, endStamp);
        return ValueTask.CompletedTask;
    }

    private static (DateTime? BeginStamp, DateTime? EndStamp) ExtractRange(JsonElement document)
    {
        if (!document.TryGetProperty("topics", out var topics) ||
            topics.ValueKind != JsonValueKind.Array)
        {
            return (null, null);
        }

        DateTime? beginStamp = null;
        DateTime? endStamp = null;
        foreach (var topic in topics.EnumerateArray())
        {
            if (topic.ValueKind != JsonValueKind.Object ||
                !topic.TryGetProperty("metadata", out var metadata) ||
                metadata.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            if (TryReadUtcDateTime(metadata, "firstMessageTimestamp", out var firstMessageTimestamp))
            {
                beginStamp = beginStamp is null
                    ? firstMessageTimestamp
                    : Min(beginStamp.Value, firstMessageTimestamp);
            }

            if (TryReadUtcDateTime(metadata, "lastMessageTimestamp", out var lastMessageTimestamp))
            {
                endStamp = endStamp is null
                    ? lastMessageTimestamp
                    : Max(endStamp.Value, lastMessageTimestamp);
            }
        }

        return (beginStamp, endStamp);
    }

    private static bool TryReadUtcDateTime(JsonElement metadata, string propertyName, out DateTime value)
    {
        value = default;
        if (!metadata.TryGetProperty(propertyName, out var stampElement) ||
            stampElement.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        var stamp = stampElement.GetString();
        if (string.IsNullOrWhiteSpace(stamp) ||
            !DateTimeOffset.TryParse(
                stamp,
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind,
                out var parsed))
        {
            return false;
        }

        value = parsed.UtcDateTime;
        return true;
    }

    private static DateTime Min(DateTime lhs, DateTime rhs)
    {
        return lhs <= rhs ? lhs : rhs;
    }

    private static DateTime Max(DateTime lhs, DateTime rhs)
    {
        return lhs >= rhs ? lhs : rhs;
    }
}
