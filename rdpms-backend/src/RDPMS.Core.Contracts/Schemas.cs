// ReSharper disable ClassNeverInstantiated.Global
namespace RDPMS.Core.Contracts;

/// <summary>
/// Meta-information about a file. If it is a container in some way, it features special fields for this.
/// </summary>
public record FileInformation
{
    public ulong? SizeBytes { get; set; }
    public FileMetadata? Metadata { get; set; }
    
    /// <summary>
    /// Information about the compression algorithm used.
    /// Can be left empty for zip, but fill out <see cref="FileMetadata"/>.
    /// </summary>
    public string? CompressionType { get; set; }

    // below are several container formats

    /// <summary>
    /// If this file is just the compressed variant of some file, put the metadata of the contained file here.
    /// Put information about the compression information in <see cref="CompressionType"/>.
    /// If you got a zip, put the contained files inside <see cref="Container"/>
    /// </summary>
    public FileInformation? CompressedFile { get; set; }
    
    /// <summary>
    /// If this file is an archive (containing multiple files), put the metadata of the contained files here.
    /// </summary>
    public FileContainer? Container { get; set; }
    
    /// <summary>
    /// If this file contains time series data, put the metadata of the contained topics here.
    /// </summary>
    public TimeSeriesContainer? TimeSeriesContainer { get; set; }
}

/// <summary>
/// Meta-information about a file.
/// </summary>
public record FileMetadata
{
    /// <summary>
    /// The file-ending the file commonly has on disk.
    /// </summary>
    public string? CanonicalEnding { get; set; }
    
    /// <summary>
    /// The MIME type of the file.
    /// </summary>
    public string? MimeType { get; set; }
}

/// <summary>
/// If a file itself contains other files, this is the container for them.
/// </summary>
public record FileContainer
{
    /// <summary>
    /// e.g. "zip", "tar", "7z"
    /// </summary>
    public string? ContainerType { get; set; }
    
    /// <summary>
    /// List of files/paths in this container.
    /// </summary>
    public List<ContainedFileInformation>? Files { get; set; }
}

/// <summary>
/// Information about a single file inside a container.
/// </summary>
public record ContainedFileInformation
{
    /// <summary>
    /// Unix-style path relative to the root of the container.
    /// </summary>
    public string? Path { get; set; }

    /// <summary>
    /// Meta-information about the file.
    /// </summary>
    FileInformation? File { get; set; }
}

/// <summary>
/// Container for information about multiple topics (data streams)
/// </summary>
public record TimeSeriesContainer
{
    /// <summary>
    /// List of topics (data streams) in this container.
    /// </summary>
    public List<Topic>? Topics { get; set; }
}

/// <summary>
/// Information about a typed single topic (data stream) in a time series.
/// </summary>
public record Topic
{
    /// <summary>
    /// The name/path of the topic.
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Meta-information about the topic, that might accelerate queries.
    /// </summary>
    public TopicMetadata? Metadata { get; set; }
    
    /// <summary>
    /// Type of the messages inside the topic.
    /// </summary>
    public MessageType? MessageType { get; set; }
}

/// <summary>
/// Type of messages in a topic.
/// </summary>
public record MessageType
{
    /// <summary>
    /// Human-readable name of the type.
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Human-readable description of the type.
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Reference to the definition, e.g. "https://docs.ros2.org/foxy/api/nav_msgs/msg/Odometry.html".
    /// </summary>
    // TODO: How to treat messages equal across different ROS versions?
    public string? Reference { get; set; }
    
    /// <summary>
    /// Maps a field, identified by a dot-separated path, to a <see cref="FieldType"/>."/>
    /// </summary>
    // TODO: this should be recursively MessageType or a primitive
    public Dictionary<string, FieldType>? Fields { get; set; }
}

public record FieldType
{
    /// <summary>
    /// Human-readable description of the field.
    /// </summary>
    public string? Descriptor { get; set; }
    
    /// <summary>
    /// Vocabulary, used for the <see cref="Descriptor"/>.
    /// </summary>
    public string? VocabularyReference { get; set; }
    
    /// <summary>
    /// Type-primitive, e.g. int, float, string, byte[]
    /// </summary>
    public string? Type { get; set; }
    
    /// <summary>
    /// In formation on the usage of e.g. a string. E.g. a string can be JSON, or a byte-array can be a pointcloud.
    /// </summary>
    public string? Format { get; set; }
}

/// <summary>
/// Meta-information about a topic, that might accelerate queries.
/// </summary>
public record TopicMetadata
{
    /// <summary>
    /// Number of messages in the topic.
    /// </summary>
    public ulong? MessageCount { get; set; }
    
    /// <summary>
    /// Timestamp of the first message in the topic.
    /// </summary>
    public DateTime? FirstMessageTimestamp { get; set; }

    /// <summary>
    /// Timestamp of the first message in the topic.
    /// </summary>
    public DateTime? LastMessageTimestamp { get; set; }
}
