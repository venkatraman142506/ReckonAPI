using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ReckonAPI.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Logging;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddHttpClient<ApiService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
    client.BaseAddress = new Uri("https://join.reckon.com/");
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
})
.AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(new[]
{
    TimeSpan.FromSeconds(1),
    TimeSpan.FromSeconds(2),
    TimeSpan.FromSeconds(3)
}));
builder.Services.AddHttpClient<IApiService, ApiService>();
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.AddDebug();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
