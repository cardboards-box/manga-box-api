using Microsoft.AspNetCore.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace MangaBox.WebApi;

public static class DiExtensions
{
    public static WebApplication UseRequesting(this WebApplication app)
    {
        app.UseExceptionHandler(err =>
        {
            err.Run(async ctx =>
            {
                Exception? resolveException(WebApplication app)
                {
                    if (!app.Environment.IsDevelopment()) return null;

                    var feature = ctx.Features.Get<IExceptionHandlerFeature>();
                    if (feature != null && feature.Error != null)
                        return feature.Error;

                    feature = ctx.Features.Get<IExceptionHandlerPathFeature>();
                    return feature?.Error;
                };

                ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
                ctx.Response.ContentType = Application.Json;
                ctx.Response.Headers.Add("Access-Control-Allow-Origin", "*");
                ctx.Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");
                var error = resolveException(app) ?? new Exception("An error has occurred, please contact an administrator for more information");

                await ctx.Response.WriteAsJsonAsync(Requests.Exception(error));
            });
        });

        app.Use(async (ctx, next) =>
        {
            await next();

            if (ctx.Response.StatusCode == StatusCodes.Status401Unauthorized)
            {
                ctx.Response.ContentType = Application.Json;
                await ctx.Response.WriteAsJsonAsync(Requests.Unauthorized());
            }
        });
        return app;
    }
}
