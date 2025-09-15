using backend.Security.Constants;

namespace backend.Security.Extensions;

public static class HttpContextExtensions
{
    public static long? GetUserId(this HttpContext ctx)
    {
        if (ctx.Items.TryGetValue(HttpContextItemKeys.UserId, out var val) && val is long id)
            return id;
        return null;
    }
}