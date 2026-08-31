using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
using StudyNotes.Components;
using StudyNotes.Data;
using StudyNotes.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Blazor Interactive Server services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 2. Add MudBlazor Component Library Services
builder.Services.AddMudServices();

// 3. Register SQLite Database Context Factory
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=studynotes.db"));

// 4. Register Application Domain Services
builder.Services.AddScoped<NoteService>();
builder.Services.AddScoped<SubjectService>();

var app = builder.Build();

// 5. Automatically Ensure Database Created & Seed Data on Launch
using (var scope = app.Services.CreateScope())
{
    var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
    using var dbContext = factory.CreateDbContext();
    dbContext.Database.EnsureCreated();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();