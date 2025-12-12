using Microsoft.AspNetCore.Mvc.Formatters;

namespace RDPMS.Core.Server.Util;

/// <summary>
/// Simple input formatter that reads the raw request body into a byte array.
/// Supports application/json and application/octet-stream so controllers can accept raw documents.
/// </summary>
internal sealed class RawBodyInputFormatter : InputFormatter
{
    public RawBodyInputFormatter()
    {
        SupportedMediaTypes.Add("application/json");
        SupportedMediaTypes.Add("application/octet-stream");
    }

    protected override bool CanReadType(Type type) => type == typeof(byte[]);

    public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
    {
        using var ms = new MemoryStream();
        await context.HttpContext.Request.Body.CopyToAsync(ms, context.HttpContext.RequestAborted);
        return await InputFormatterResult.SuccessAsync(ms.ToArray());
    }
}