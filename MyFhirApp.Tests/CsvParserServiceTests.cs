using Xunit;
using System.IO;
using MyFhirApp;
using Microsoft.Extensions.Logging.Abstractions;

namespace MyFhirApp.Tests
{
    public class CsvParserServiceTests
    {
        [Fact]
        public void LoadAndValidate_ShouldSkipInvalidRows()
        {
            var csvPath = Path.GetTempFileName();
            File.WriteAllText(csvPath,
                "PATIENT_GIVENNAME,PATIENT_FAMILYNAME,PATIENT_GENDER,PATIENT_ID,TIMESTAMP,WBC,RBC,HB\n" +
                "John,Doe,M,12345,2025-09-23,5.5,4.2,13.1\n" +
                "Jane,Doe,F,,2025-09-23,5.5,4.2,13.1\n");

            // ✅ Inject a NullLogger so the constructor compiles
            var parser = new CsvParserService(NullLogger<CsvParserService>.Instance);

            var result = parser.LoadAndValidate(csvPath);

            Assert.Single(result);
            Assert.Equal("John", result[0].FirstName);

            File.Delete(csvPath);
        }
    }
}

