using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyFhirApp;

// Build the DI container
var services = new ServiceCollection();

// Add logging (console output)
services.AddLogging(config =>
{
    config.ClearProviders();
    config.AddConsole();
    config.SetMinimumLevel(LogLevel.Information);
});

// Register your services
services.AddTransient<CsvParserService>();
services.AddTransient<FhirMappingService>(sp =>
{
    // pipeline version comes from config or hardcoded for now
    var logger = sp.GetRequiredService<ILogger<FhirMappingService>>();
    return new FhirMappingService("1.0.0", logger);
});
services.AddTransient<FhirUploaderService>();
services.AddTransient<CsvUploaderService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<CsvUploaderService>>();
    var parserLogger = sp.GetRequiredService<ILogger<CsvParserService>>();
    var mappingLogger = sp.GetRequiredService<ILogger<FhirMappingService>>();
    var uploaderLogger = sp.GetRequiredService<ILogger<FhirUploaderService>>();

    return new CsvUploaderService(
        "https://your.fhir.server/baseR4",
        "1.0.0",
        logger,
        parserLogger,
        mappingLogger,
        uploaderLogger
    );
});

// Build provider
var provider = services.BuildServiceProvider();

// Resolve the top‑level service
var uploader = provider.GetRequiredService<CsvUploaderService>();

// Run the pipeline
await uploader.UploadFromCsvAsync(
    @"C:\Users\Salom\DevProjects\MyFhirApp\data\nhanes_sample.csv",
    dryRun: true
);




