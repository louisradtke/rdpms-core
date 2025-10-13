using Microsoft.EntityFrameworkCore;
using RDPMS.Core.Infra;
using RDPMS.Core.Persistence.Model;

namespace RDPMS.Core.Persistence;

public static class DefaultValues
{
    static DefaultValues()
    {
        DefaultTypes =
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
                Abbreviation = "pdf", Name = "PDF", Description = "Portable Document Format",
                MimeType = "application/pdf",
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
                Abbreviation = "svg", Name = "SVG", Description = "Scalable Vector Graphics",
                MimeType = "image/svg+xml",
                Id = Guid.Parse("157c6891-3e43-46b6-930f-52749da6e632")
            },

            // ros-related
            new()
            {
                Abbreviation = "mcap", Name = "MCAP", Description = "Serialization-agnostic container file format.",
                MimeType = "application/octet-stream", Id = Guid.Parse("2b31bbd9-0049-45c0-a9a4-b4893fa1085c")
            }
        ];

        DummyS3Store = new S3DataStore("Dummy S3 Store")
        {
            Id = RDPMSConstants.DummyS3StoreId,
            Slug = "dummy-s3-store",
            Bucket = "dummy-bucket",
            EndpointUrl = "https://localhost:5002",
            KeyPrefix = "data/",
            AccessKeyReference = "direct://admin",
            SecretKeyReference = "direct://thisisasecret"
        };
    }

    public static async Task<DataCollectionEntity> GetDummyDataCollectionAsync(DbContext ctx, CancellationToken token)
    {
        var typeSet = ctx.Set<ContentType>();
        var imageType = await typeSet
            .FirstAsync(t => t.Id == Guid.Parse("0c52cf95-4d5c-4e04-89f7-6b9572b7d952"), token);
        var pdfType = await typeSet
            .FirstAsync(t => t.Id == Guid.Parse("3ae8c8ef-5cc4-45f8-81c6-6c603b1f4580"), token);
        var mcapType = await typeSet
            .FirstAsync(t => t.Id == Guid.Parse("2b31bbd9-0049-45c0-a9a4-b4893fa1085c"), token);
        var s3Store = await ctx.Set<DataStore>()
            .FindAsync(RDPMSConstants.DummyS3StoreId, token) as S3DataStore;
        var projectId = RDPMSConstants.GlobalProjectId;
        
        return BuildDataCollectionEntity(s3Store, projectId, mcapType, imageType, pdfType);
    }
    public static DataCollectionEntity GetDummyDataCollection(DbContext ctx)
    {
        var typeSet = ctx.Set<ContentType>();
        var imageType = typeSet.First(t => t.Id == Guid.Parse("0c52cf95-4d5c-4e04-89f7-6b9572b7d952"));
        var pdfType = typeSet.First(t => t.Id == Guid.Parse("3ae8c8ef-5cc4-45f8-81c6-6c603b1f4580"));
        var mcapType = typeSet.First(t => t.Id == Guid.Parse("2b31bbd9-0049-45c0-a9a4-b4893fa1085c"));
        var s3Store = ctx.Set<DataStore>().Find(RDPMSConstants.DummyS3StoreId) as S3DataStore;
        var projectId = RDPMSConstants.GlobalProjectId;

        return BuildDataCollectionEntity(s3Store, projectId, mcapType, imageType, pdfType);
    }

    private static DataCollectionEntity BuildDataCollectionEntity(S3DataStore? s3Store, Guid projectId,
        ContentType mcapType, ContentType imageType, ContentType pdfType)
    {
        return new DataCollectionEntity("Dummy Collection")
        {
            Id = RDPMSConstants.DummyDataCollectionId,
            Slug = "dummy-collection",
            Description = "This is a dummy for a data collection",
            DefaultDataStore = s3Store,
            ParentId = projectId,
            ContainedDatasets =
            [
                new DataSet("dummy-recording-01")
                {
                    Id = Guid.Parse("fae41903-b023-427e-93cf-1a7f9d6437e8"),
                    Slug = "dummy-recording-01",
                    CreatedStamp = DateTime.Parse("2025-09-11T22:57:38.000+02:00"),
                    State = DataSetState.Sealed,
                    Files =
                    [
                        new DataFile("demo_2025-09-11_22-57-38.mcap")
                        {
                            Id = Guid.Parse("40a447f0-0012-46e9-b882-3ab9a30ae187"),
                            FileType = mcapType,
                            SizeBytes = 6403,
                            SHA256Hash = "5608713a5fd3b40926ef5143b6f9b116683adc05c6612d32ad8646245b669c0b",
                            CreatedStamp = DateTime.Parse("2025-09-11T22:57:38.000+02:00"),
                            BeginStamp = DateTime.Parse("2025-09-11T22:57:31.626+02:00"),
                            EndStamp = DateTime.Parse("2025-09-11T22:57:35.623+02:00"),
                            Locations =
                            [
                                new StaticFileStorageReference()
                                {
                                    Id = Guid.Parse("0f5bd4e5-cdec-46c5-aae3-ea0242c7f160"),
                                    Algorithm = CompressionAlgorithm.Plain,
                                    SizeBytes = 6403,
                                    SHA256Hash = "5608713a5fd3b40926ef5143b6f9b116683adc05c6612d32ad8646245b669c0b",
                                    URL = "http://localhost:5001/dummy-recording-01/demo_2025-09-11_22-57-38.mcap"
                                }
                            ]
                        },
                        new DataFile("image.png")
                        {
                            Id = Guid.Parse("bf2256e3-697a-4ad0-863a-f5f11b24dc10"),
                            FileType = imageType,
                            SizeBytes = 51247,
                            SHA256Hash = "226e877255d61660811f1da3e461e6521660015bb708b64eb6771b27cd37a40d",
                            CreatedStamp = DateTime.Parse("2025-09-11T23:11:00.000+02:00"),
                            Locations =
                            [
                                new StaticFileStorageReference()
                                {
                                    Id = Guid.Parse("bcf7c002-851e-41e5-af9a-5aa595aa6904"),
                                    Algorithm = CompressionAlgorithm.Plain,
                                    SizeBytes = 51247,
                                    SHA256Hash = "226e877255d61660811f1da3e461e6521660015bb708b64eb6771b27cd37a40d",
                                    URL = "http://localhost:5001/dummy-recording-01/image.png"
                                }
                            ]
                        },
                        new DataFile("imu_tedaldi_calib.pdf")
                        {
                            Id = Guid.Parse("34f3d1a7-4b28-4493-accf-fa1b77003922"),
                            FileType = pdfType,
                            SizeBytes = 117949,
                            SHA256Hash = "2f77a8eee70538ae36c88f8b5eed63db4fbebac241eb7d872635858b8b2106f3",
                            CreatedStamp = DateTime.Parse("2025-09-11T23:11:00.000+02:00"),
                            Locations =
                            [
                                new StaticFileStorageReference()
                                {
                                    Id = Guid.Parse("a94b40b3-ef3f-495d-8f44-9aed5045b6c3"),
                                    Algorithm = CompressionAlgorithm.Plain,
                                    SizeBytes = 117949,
                                    SHA256Hash = "2f77a8eee70538ae36c88f8b5eed63db4fbebac241eb7d872635858b8b2106f3",
                                    URL = "http://localhost:5001/dummy-recording-01/imu_tedaldi_calib.pdf"
                                }
                            ]
                        }
                    ]
                },
                new DataSet("dummy-recording-02")
                {
                    Id = Guid.Parse("3b5a0c9b-d1da-42ae-92ac-4dc700224cda"),
                    Slug = "dummy-recording-02",
                    CreatedStamp = DateTime.Parse("2025-10-04T13:52:00.000+02:00"),
                    State = DataSetState.Sealed,
                    Files = []
                }
            ]
        };
    }

    public static S3DataStore DummyS3Store { get; }

    public static List<ContentType> DefaultTypes { get; }
}