using System.Text;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using VanLife.Api.Data;
using VanLife.Api.Services;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your valid token in the text input below."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Jwt configuration
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key is not configured. Use user-secrets or environment variable.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "VanLife.Api";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "VanLife.Client";

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtKey))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddSingleton<JwtTokenService>();

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<VanService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<ReviewService>();
builder.Services.AddScoped<IncomeService>();
builder.Services.AddScoped<ImageService>();
builder.Services.AddScoped<RentalService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
// Repositories
builder.Services.AddScoped(typeof(VanLife.Api.Data.Repositories.IRepository<>), typeof(VanLife.Api.Data.Repositories.EfRepository<>));
builder.Services.AddScoped<VanLife.Api.Data.Repositories.IVanRepository, VanLife.Api.Data.Repositories.VanRepository>();

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
