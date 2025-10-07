using Xunit;
using MyFhirApp;
using Hl7.Fhir.Model;
using System.Collections.Generic;
using Microsoft.Extensions.Logging.Abstractions;
using Task = System.Threading.Tasks.Task;   // ✅ Alias to avoid ambiguity

namespace MyFhirApp.Tests
{
    public class FhirUploaderServiceTests
    {
        [Fact]
        public async Task UploadAsync_ShouldRunDryRunWithoutErrors()
        {
            var record = new CSVModel { SourcePatientId = "P001" };
            var patient = new Patient { Id = "P001" };
            var observations = new List<Observation>
            {
                new Observation { Code = new CodeableConcept("https://loinc.org", "718-7", "Hemoglobin") }
            };

            var uploader = new FhirUploaderService(
                "https://server.fire.ly/r4",
                NullLogger<FhirUploaderService>.Instance
            );

            await uploader.UploadAsync(record, patient, observations, dryRun: true);

            Assert.True(true);
        }
    }
}


