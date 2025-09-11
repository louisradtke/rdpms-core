using RDPMS.Core.Persistence.Model;

namespace RDPMS.Core.Persistence;

public static class DefaultValues
{
    public static List<ContentType> Types { get; }  =
    [
        // data
        new()
        {
            Abbreviation = "csv", Name = "CSV", Description = "Comma-separated values", MimeType = "text/csv",
            Id = Guid.Parse("c33c33d4-97fa-4fa7-b5c9-e6d9be422b3d")
        },
        new()
        {
            Abbreviation = "json", Name = "JSON", Description = "JavaScript Object Notation",
            MimeType = "application/json", Id = Guid.Parse("422f1f3c-ac58-41d4-906c-2c8b04dab258")
        },
        new()
        {
            Abbreviation = "txt", Name = "Text", Description = "Plain text", MimeType = "text/plain",
            Id = Guid.Parse("52fea9e9-6329-40a6-9f47-34b5765395d1")
        },
        new()
        {
            Abbreviation = "xml", Name = "XML", Description = "Extensible Markup Language",
            MimeType = "application/xml", Id = Guid.Parse("a211889e-d576-4abf-a098-07291f9f7964")
        },

        // human-readable documents
        new()
        {
            Abbreviation = "md", Name = "Markdown", Description = "Markdown", MimeType = "text/markdown",
            Id = Guid.Parse("cb54d809-bb90-4973-bed7-d23a9d572cf5")
        },
        new()
        {
            Abbreviation = "rtf", Name = "RTF", Description = "Rich Text Format", MimeType = "application/rtf",
            Id = Guid.Parse("1bfa807f-559a-476c-9026-e13562aa6b82")
        },
        new()
        {
            Abbreviation = "pdf", Name = "PDF", Description = "Portable Document Format", MimeType = "application/pdf",
            Id = Guid.Parse("3ae8c8ef-5cc4-45f8-81c6-6c603b1f4580")
        },

        // video
        new()
        {
            Abbreviation = "mp4", Name = "MP4", Description = "MP4 video", MimeType = "video/mp4",
            Id = Guid.Parse("75f26add-8102-475f-a408-613e236a514d")
        },
        new()
        {
            Abbreviation = "mpeg", Name = "MPEG", Description = "MPEG Video", MimeType = "video/mpeg",
            Id = Guid.Parse("14461714-bc25-4633-a444-91b00c4803e1")
        },
        new()
        {
            Abbreviation = "webm", Name = "WebM", Description = "WebM video", MimeType = "video/webm",
            Id = Guid.Parse("9995cc3f-fb27-41d8-abfa-000af7b1d8aa")
        },

        new()
        {
            Abbreviation = "gif", Name = "GIF", Description = "Graphics Interchange Format", MimeType = "image/gif",
            Id = Guid.Parse("be174835-b487-45a8-b047-4fa4c7c8b568")
        },

        // audio
        new()
        {
            Abbreviation = "mp3", Name = "MP3", Description = "MP3 audio", MimeType = "audio/mpeg",
            Id = Guid.Parse("cb8ffff4-4925-4fbf-8e2b-b1e32dc5d522")
        },

        new()
        {
            Abbreviation = "wav", Name = "WAV", Description = "Waveform Audio Format", MimeType = "audio/wav",
            Id = Guid.Parse("3a86ffd9-370b-416d-b974-1a7d80961b26")
        },

        // images
        new()
        {
            Abbreviation = "png", Name = "PNG", Description = "Portable Network Graphics", MimeType = "image/png",
            Id = Guid.Parse("0c52cf95-4d5c-4e04-89f7-6b9572b7d952")
        },
        new()
        {
            Abbreviation = "webp", Name = "WEBP", Description = "WebP image", MimeType = "image/webp",
            Id = Guid.Parse("25d06d64-7ba4-49b0-a90c-8d395f8d9301")
        },
        new()
        {
            Abbreviation = "jpeg", Name = "JPEG", Description = "JPEG image", MimeType = "image/jpeg",
            Id = Guid.Parse("5884dbad-bcd7-4a35-bc9b-2924f864ef03")
        },
        new()
        {
            Abbreviation = "svg", Name = "SVG", Description = "Scalable Vector Graphics", MimeType = "image/svg+xml",
            Id = Guid.Parse("157c6891-3e43-46b6-930f-52749da6e632")
        }
    ];
}