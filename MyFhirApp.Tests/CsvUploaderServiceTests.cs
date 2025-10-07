using Xunit;
using MyFhirApp;
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;

namespace MyFhirApp.Tests
{
    public class CsvUploaderServiceTests
    {
        [Fact]
        public async Task UploadAsync_ShouldRunDryRunWithoutErrors()
        {
            var csvPath = Path.GetTempFileName();
            File.WriteAllText(csvPath,
                "PATIENT_GIVENNAME,PATIENT_FAMILYNAME,PATIENT_GENDER,PATIENT_ID,TIMESTAMP,WBC,RBC,HB\n" +
                "John,Doe,M,12345,2025-09-23,5.5,4.2,13.1\n");

            var uploader = new CsvUploaderService(
                "https://server.fire.ly/r4",
                "v1.2",
                NullLogger<CsvUploaderService>.Instance,
                NullLogger<CsvParserService>.Instance,
                NullLogger<FhirMappingService>.Instance,
                NullLogger<FhirUploaderService>.Instance
            );

            // Act: run in dry-run mode
            await uploader.UploadFromCsvAsync(csvPath, dryRun: true);

            // Assert: if no exception is thrown, the dry-run succeeded
            // (you could extend this with a custom logger to capture messages)
            Assert.True(true);

            File.Delete(csvPath);
        }
    }
}
