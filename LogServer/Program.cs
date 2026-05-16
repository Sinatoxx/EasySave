using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string logFile = "/app/logs/central.json";

string logDir = Path.GetDirectoryName(logFile)!;
if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);

object fileLock = new();

app.MapPost("/logs", async (HttpContext ctx) =>
{
    using StreamReader reader = new(ctx.Request.Body);
    string body = await reader.ReadToEndAsync();

    lock (fileLock)
    {
        List<object> entries = new();
        if (File.Exists(logFile))
        {
            string existing = File.ReadAllText(logFile);
            try
            {
                entries = JsonSerializer.Deserialize<List<object>>(existing) ?? new();
            }
            catch { }
        }

        var entry = JsonSerializer.Deserialize<object>(body);
        if (entry != null) entries.Add(entry);

        File.WriteAllText(logFile, JsonSerializer.Serialize(entries, new JsonSerializerOptions { WriteIndented = true }));
    }

    return Results.Ok();
});

app.MapGet("/logs", () =>
{
    if (!File.Exists(logFile)) return Results.Ok(new List<object>());
    return Results.Content(File.ReadAllText(logFile), "application/json");
});

app.Run("http://0.0.0.0:5000");