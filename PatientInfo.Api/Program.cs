using PatientInfo.Api.Services;
using Microsoft.Data.SqlClient;
using System.Data;
using Serilog;
using Microsoft.AspNetCore.Hosting.WindowsServices;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) => 
    config.ReadFrom.Configuration(context.Configuration));

builder.Host.UseWindowsService();

Log.Information("PatientInfo.Api starting up...");

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IPatientInfoService, PatientInfoService>();
builder.Services.AddScoped<IDbConnection>(sp => 
    new SqlConnection(builder.Configuration.GetConnectionString("HisConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

Log.Information("PatientInfo.Api started successfully");

app.Run();

Log.Information("PatientInfo.Api shutting down...");
