using System.Text;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using VanLife.Api.Data;
using VanLife.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT authentication removed - authentication/authorization middleware is not used.

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<VanService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<IncomeService>();
builder.Services.AddScoped<ImageService>();
builder.Services.AddScoped<RentalService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// health checks
builder.Services.AddHealthChecks();

// Configure automatic ModelState -> ProblemDetails behavior
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        return new BadRequestObjectResult(context.ModelState);
    };
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == StatusCodes.Status415UnsupportedMediaType && !context.Response.HasStarted)
    {
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc9110#section-15.5.16",
            title = "Unsupported Media Type",
            status = StatusCodes.Status415UnsupportedMediaType,
            traceId = Activity.Current?.Id ?? context.TraceIdentifier,
            detail = "Request body must be valid JSON with Content-Type: application/json."
        });
    }
});

app.MapControllers();

app.MapHealthChecks("/health");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);
    await DbSeeder.EnsureExistingUsersAsync(db);
}

app.MapFallback(() =>
{
    return Results.NotFound(new
    {
        statusCode = 404,
        message = "Endpoint not found. Please check your URL."
    });
});

app.Run();
