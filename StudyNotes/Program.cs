using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using Npgsql;
using StudyNotes.Components;
using StudyNotes.Data;
using StudyNotes.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Blazor Interactive Server services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 2. Add MudBlazor Component Library Services
builder.Services.AddMudServices();

// 3. Register Database Context Factory
//    Local development uses SQLite; production (Render) uses PostgreSQL.
//    Switch via the "Database:Provider" setting — set to "Postgres" on Render.
var dbProvider = builder.Configuration["Database:Provider"] ?? "Sqlite";
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
{
    if (dbProvider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
    {
        options.UseNpgsql(ResolveConnectionString(builder.Configuration));
    }
    else
    {
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=studynotes.db");
    }
});

// 4. Register Application Domain Services
builder.Services.AddScoped<NoteService>();
builder.Services.AddScoped<SubjectService>();

// 5. Listen on the port Render injects (defaulting to 8080)
var port = builder.Configuration["PORT"] ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

var app = builder.Build();

// 6. Automatically Ensure Database Created & Seed Data on Launch
//    Retried briefly so Render's database is ready on the first deploy.
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
    var startupLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    for (var attempt = 1; ; attempt++)
    {
        try
        {
            await using var dbContext = await factory.CreateDbContextAsync();
            await dbContext.Database.EnsureCreatedAsync();
            break;
        }
        catch (Exception ex) when (attempt < 10)
        {
            startupLogger.LogWarning("Database not ready (attempt {Attempt}/10): {Message}", attempt, ex.Message);
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();

    // Trust Render's reverse proxy so HTTPS redirection and client IPs work correctly.
    var forwardedHeadersOptions = new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    };
    forwardedHeadersOptions.KnownNetworks.Clear();
    forwardedHeadersOptions.KnownProxies.Clear();
    app.UseForwardedHeaders(forwardedHeadersOptions);
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

// Use the configured connection string, or fall back to Render's auto-injected DATABASE_URL.
static string ResolveConnectionString(IConfiguration configuration)
{
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        return NormalizeConnectionString(connectionString);
    }

    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrWhiteSpace(databaseUrl))
    {
        return NormalizeConnectionString(databaseUrl);
    }

    throw new InvalidOperationException("PostgreSQL connection string 'DefaultConnection' or the 'DATABASE_URL' environment variable is not set.");
}

// Render injects PostgreSQL connection strings in URI form (postgres://user:pass@host:port/db),
// which Npgsql cannot parse. Convert to Npgsql's key=value format.
static string NormalizeConnectionString(string value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return value;
    }

    var trimmed = value.Trim();

    if (!(trimmed.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) ||
          trimmed.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase)))
    {
        return value;
    }

    var uri = new Uri(trimmed);

    var username = "";
    var password = "";
    if (!string.IsNullOrEmpty(uri.UserInfo))
    {
        var userInfo = uri.UserInfo.Split(':', 2);
        username = Uri.UnescapeDataString(userInfo[0]);
        if (userInfo.Length > 1)
        {
            password = Uri.UnescapeDataString(userInfo[1]);
        }
    }

    var builder = new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Database = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/')),
        Username = username,
        Password = password
    };

    foreach (var pair in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
    {
        var kv = pair.Split('=', 2);
        var key = kv[0].ToLowerInvariant();
        var val = kv.Length > 1 ? Uri.UnescapeDataString(kv[1]) : "";

        switch (key)
        {
            case "sslmode":
            case "ssl-mode":
                if (Enum.TryParse<SslMode>(val.Replace("-", ""), true, out var sslMode))
                {
                    builder.SslMode = sslMode;
                }
                break;
            case "sslrootcert":
                builder.RootCertificate = val;
                break;
            case "trust_server_certificate":
            case "trustservercertificate":
                builder.TrustServerCertificate = val.Equals("true", StringComparison.OrdinalIgnoreCase);
                break;
            case "connect_timeout":
                if (int.TryParse(val, out var timeout))
                {
                    builder.Timeout = timeout;
                }
                break;
            case "application_name":
                builder.ApplicationName = val;
                break;
        }
    }

    return builder.ConnectionString;
}