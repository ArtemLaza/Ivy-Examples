﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace PuppeteerSharpDemo
{
    public class DownloadEndpointStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.UseRouting();
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/download/{id}", async context =>
                    {
                        var id = context.Request.RouteValues["id"]?.ToString();
                        var file = id != null ? MemoryFileStore.Get(id) : null;

                        if (file == null)
                        {
                            context.Response.StatusCode = 404;
                            await context.Response.WriteAsync("File not found or expired.");
                            return;
                        }

                        context.Response.ContentType = file.Value.ContentType;
                        context.Response.Headers["Content-Disposition"] = $"attachment; filename=\"{file.Value.FileName}\"";

                        await context.Response.Body.WriteAsync(file.Value.Data);
                        MemoryFileStore.Remove(id); // one-time download
                    });
                });

                next(app);
            };
        }
    }
}
