using Microsoft.EntityFrameworkCore;
using Npgsql;
using Shortenkai.Database;
using Shortenkai.Services;
using Shortenkai.Services.Interfaces;
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
