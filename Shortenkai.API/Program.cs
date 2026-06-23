using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shortenkai.Application.Services;
using Shortenkai.Infrastructure.Database;
using Shortenkai.Infrastructure.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.EnableDynamicJson();

var dataSource = dataSourceBuilder.Build();

await using var conn = dataSource.OpenConnection();

Console.WriteLine($"PostgreSQL version: {conn.PostgreSqlVersion}");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);

    options.AssumeDefaultVersionWhenUnspecified = true;

    options.ReportApiVersions = true;

    options.ApiVersionReader = ApiVersionReader.Combine(
        new QueryStringApiVersionReader("api-version"),
        new HeaderApiVersionReader("X-API-Version")
    );
});

// Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "Shortenkai:";
});
builder.Services.AddSingleton<CacheService>();


builder.Services.AddDbContext<ShortenkaiUrlDb>(options =>
    options.UseNpgsql(dataSource));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy
        //.WithOrigins(builder.Configuration["Frontend:Url"]!)
            .AllowAnyOrigin()      
            .AllowAnyHeader()
            .AllowAnyMethod()
        );
});


builder.Services.AddScoped<IShortenkaiService, ShortenkaiService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.MapControllers();

app.Run();
