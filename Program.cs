using Asp.Versioning;
using Azure.Storage.Blobs;
using FileValidation.API.Services;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IFileValidationRequestService, FileValidationRequestService>();
builder.Services.AddScoped<IFileValidator, FileValidator>();
builder.Services.AddScoped<IAzureStorageService, AzureStorageService>();
builder.Services.AddScoped(_ => new BlobServiceClient(builder.Configuration["StorageAccount"]));
builder.Services.AddApiVersioning(config =>
{
    config.DefaultApiVersion = new ApiVersion(1, 0);
    config.AssumeDefaultVersionWhenUnspecified = true;
    config.ReportApiVersions = true;
});
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
