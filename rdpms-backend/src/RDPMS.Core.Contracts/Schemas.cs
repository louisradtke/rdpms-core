// ReSharper disable ClassNeverInstantiated.Global
namespace RDPMS.Core.Contracts;

/// <summary>
/// Meta-information about a file. If it is a container in some way, it features special fields for this.
/// </summary>
public record FileInformation
{
    public ulong SizeBytes { get; set; } /* required: true */
    public FileMetadata? Metadata { get; set; } /* optional */

    // below are several fields related to subordinated contents
    // dev notes:
    // - the system should be able to store arbitrary files. these files can be
    //   - plain files containing arbitrary data
    //   - [compressed] tarballs, thus a tarball/archive inside a compressed "archive"
    //   - [compressed] zips, but with microsoft it is archive and compression in one
    //   - containers for time series
    //   - arbitrary documents, but I have only vague ideas how to integrate that.
    //     I am also not sure whether this is covered by the "tarball" case.
    // some files might also be containers for files and time series
    // TODO: consider modeling file representations as a recursive chain (e.g. compressed -> container)
    //       instead of multiple optional fields that can conflict.

    /// <summary>
    /// Information about the compression algorithm used.
    /// Can be left empty for zip, but fill out <see cref="FileMetadata"/>.
    /// </summary>
    public string? CompressionType { get; set; } /* optional */

    /// <summary>
    /// If this file is just the compressed variant of some file, put the metadata of the contained file here.
    /// Put information about the compression information in <see cref="CompressionType"/>.
    /// If you got a zip, put the contained files inside <see cref="Container"/>
    /// </summary>
    public FileInformation? CompressedFile { get; set; } /* optional */
    
    /// <summary>
    /// If this file is an archive (containing multiple files), put the metadata of the contained files here.
    /// </summary>
    public FileContainer? Container { get; set; } /* optional */
    
    /// <summary>
    /// If this file contains time series data, put the metadata of the contained topics here.
    /// </summary>
    public TimeSeriesContainer? TimeSeriesContainer { get; set; } /* optional */
}

/// <summary>
/// Meta-information about a file.
/// </summary>
public record FileMetadata
{
    /// <summary>
    /// The file-ending the file commonly has on disk.
    /// </summary>
    public string? CanonicalEnding { get; set; } /* optional */
    
    /// <summary>
    /// The MIME type of the file.
    /// </summary>
    public string? MimeType { get; set; } /* optional */
}

/// <summary>
/// If a file itself contains other files, this is the container for them.
/// </summary>
public record FileContainer
{
    /// <summary>
    /// e.g. "zip", "tar", "7z"
    /// </summary>
    public string ContainerType { get; set; } /* required */
    
    /// <summary>
    /// List of files/paths in this container.
    /// </summary>
    public List<ContainedFileInformation> Files { get; set; } /* required */
}

/// <summary>
/// Information about a single file inside a container.
/// </summary>
public record ContainedFileInformation
{
    /// <summary>
    /// Unix-style path relative to the root of the container.
    /// </summary>
    public string Path { get; set; } /* required */

    /// <summary>
    /// Meta-information about the file.
    /// </summary>
    public FileInformation File { get; set; } /* required */
}

/// <summary>
/// Container for information about multiple topics (data streams)
/// </summary>
public record TimeSeriesContainer
{
    /// <summary>
    /// List of topics (data streams) in this container.
    /// </summary>
    public List<Topic> Topics { get; set; } /* required */
}

/// <summary>
/// Information about a typed single topic (data stream) in a time series.
/// </summary>
public record Topic
{
    /// <summary>
    /// The name/path of the topic.
    /// </summary>
    public string Name { get; set; } /* required */
    
    /// <summary>
    /// Meta-information about the topic, that might accelerate queries.
    /// </summary>
    public TopicMetadata Metadata { get; set; } /* required */
    
    /// <summary>
    /// Type of the messages inside the topic.
    /// </summary>
    public MessageType MessageType { get; set; } /* required */
}

/// <summary>
/// Type of messages in a topic.
/// </summary>
public record MessageType
{
    /// <summary>
    /// Human-readable name of the type.
    /// </summary>
    public string Name { get; set; } /* required */
    
    /// <summary>
    /// Human-readable description of the type.
    /// </summary>
    public string? Description { get; set; } /* optional */
    
    /// <summary>
    /// Reference to the definition, e.g. "https://docs.ros2.org/foxy/api/nav_msgs/msg/Odometry.html".
    /// </summary>
    // TODO: How to treat messages equal across different ROS versions?
    public string Reference { get; set; } /* required */
    
    /// <summary>
    /// Maps a field, identified by a dot-separated path, to a <see cref="FieldType"/>."/>
    /// </summary>
    // TODO: this should be recursively MessageType or a primitive
    // TODO: Essentially, I am mimicking JSON schema ...
    // TODO: dictionaries are bad for enforcing the keys to follow certain rules.
    // TODO: represent Fields as a recursive structure (properties/items/oneOf) instead of dot-path keys.
    public Dictionary<string, FieldType> Fields { get; set; } /* required */
}

public record FieldType
{
    /// <summary>
    /// Human-readable description of the field.
    /// </summary>
    public string Descriptor { get; set; } /* required */
    
    /// <summary>
    /// Vocabulary, used for the <see cref="Descriptor"/>.
    /// </summary>
    public string? VocabularyReference { get; set; } /* optional */
    
    /// <summary>
    /// Type-primitive, e.g. int, float, string, byte[]
    /// </summary>
    public string Type { get; set; } /* required */
    
    /// <summary>
    /// Information on the usage of e.g. a string. E.g. a string can be JSON, or a byte-array can be a pointcloud.
    /// </summary>
    public string? Format { get; set; } /* optional */
}

/// <summary>
/// Meta-information about a topic, that might accelerate queries.
/// </summary>
public record TopicMetadata
{
    /// <summary>
    /// Number of messages in the topic.
    /// </summary>
    public ulong? MessageCount { get; set; } /* optional */

    /// <summary>
    /// Timestamp of the first message in the topic.
    /// </summary>
    public DateTimeOffset? FirstMessageTimestamp { get; set; } /* optional */

    /// <summary>
    /// Timestamp of the first message in the topic.
    /// </summary>
    public DateTimeOffset? LastMessageTimestamp { get; set; } /* optional */

    // TODO: we might additional info for certain fields, like min, max
}

// TODO: consider adding controlled vocab/enums for Type, Format, MimeType, ContainerType, CompressionType.
// TODO: consider schema metadata ($id, $schema, version) for future JSON schema generation.
