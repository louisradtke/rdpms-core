using Minio;
using Minio.DataModel.Args;
using RDPMS.Core.Persistence;

var storeConfig = DefaultValues.DummyS3Store;
var accessKey = storeConfig.AccessKeyReference?.Replace("direct://", "");
var secretKey = storeConfig.SecretKeyReference?.Replace("direct://", "");

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IMinioClient>(_ => new MinioClient()
    .WithEndpoint(storeConfig.EndpointAddress)
    .WithCredentials(accessKey, secretKey)
    .Build());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/buckets", async (IMinioClient minioClient) =>
    {
        var buckets = await minioClient.ListBucketsAsync();
        return Results.Ok(buckets.Buckets.Select(b => new { b.Name, b.CreationDate }));
    })
    .WithName("ListBuckets")
    .WithDescription("List all MinIO buckets");

app.MapGet("/buckets/{bucketName}/objects", async (string bucketName, IMinioClient minioClient) =>
    {
        var objects = new List<string>();
        var listArgs = new ListObjectsArgs()
            .WithBucket(bucketName)
            .WithRecursive(true);
        
        await foreach (var item in minioClient.ListObjectsEnumAsync(listArgs))
        {
            objects.Add(item.Key);
        }
        
        return Results.Ok(objects);
    })
    .WithName("ListObjects")
    .WithDescription("List all objects in a specific bucket");

app.MapPost("/buckets/{bucketName}", async (string bucketName, IMinioClient minioClient) =>
    {
        var beArgs = new BucketExistsArgs()
            .WithBucket(bucketName);
        
        bool found = await minioClient.BucketExistsAsync(beArgs);
        
        if (!found)
        {
            var mbArgs = new MakeBucketArgs()
                .WithBucket(bucketName);
            await minioClient.MakeBucketAsync(mbArgs);
            return Results.Created($"/buckets/{bucketName}", new { bucketName });
        }
        
        return Results.Conflict(new { message = "Bucket already exists" });
    })
    .WithName("CreateBucket")
    .WithDescription("Create a new bucket");

app.MapPost("/buckets/{bucketName}/upload", async (string bucketName, string objectPath, IMinioClient minioClient) =>
    {
        var beArgs = new BucketExistsArgs()
            .WithBucket(bucketName);
        
        bool found = await minioClient.BucketExistsAsync(beArgs);
        
        if (!found)
        {
            return Results.NotFound(new { message = "Bucket not found" });
        }
        
        var presignedArgs = new PresignedPutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectPath)
            .WithExpiry(3600); // 1 hour expiry
        
        string presignedUrl = await minioClient.PresignedPutObjectAsync(presignedArgs);
        
        return Results.Ok(new { uploadUrl = presignedUrl, bucketName, objectPath, expiresInSeconds = 3600 });
    })
    .WithName("GetUploadUrl")
    .WithDescription("Get a presigned URL for uploading a file");

app.MapGet("/buckets/{bucketName}/download/{*objectPath}", async (string bucketName, string objectPath, IMinioClient minioClient) =>
    {
        var beArgs = new BucketExistsArgs()
            .WithBucket(bucketName);
        
        bool found = await minioClient.BucketExistsAsync(beArgs);
        
        if (!found)
        {
            return Results.NotFound(new { message = "Bucket not found" });
        }
        
        var presignedArgs = new PresignedGetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectPath)
            .WithExpiry(3600); // 1 hour expiry
        
        string presignedUrl = await minioClient.PresignedGetObjectAsync(presignedArgs);
        
        return Results.Redirect(presignedUrl);
    })
    .WithName("DownloadFile")
    .WithDescription("Download a file (redirects to MinIO presigned URL)");

app.UseSwagger();
app.UseSwaggerUI();

app.Run();
