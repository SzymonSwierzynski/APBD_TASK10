using APBD_TASK10_BLAZOR.Api;
using APBD_TASK10_BLAZOR.Components;
using APBD_TASK10_BLAZOR.Data;
using APBD_TASK10_BLAZOR.Services;
using Microsoft.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<InMemoryDataStore>();

builder.Services.AddScoped<ObservedStudentsState>();

builder.Services.AddScoped(serviceProvider =>
{
    var navigationManager = serviceProvider.GetRequiredService<NavigationManager>();
    var handler = new HttpClientHandler();
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }

    return new HttpClient(handler)
    {
        BaseAddress = new Uri(navigationManager.BaseUri),
    };
});
builder.Services.AddScoped<StudentsApiClient>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapStudentsApi();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
