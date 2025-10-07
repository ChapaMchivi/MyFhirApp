using Xunit;
using MyFhirApp;
using Hl7.Fhir.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;

namespace MyFhirApp.Tests
{
    public class FhirMappingServiceTests
    {
        [Fact]
        public void MapPatient_ShouldSetCorrectGender()
        {
            var model = new CSVModel
            {
                FirstName = "Alice",
                LastName = "Smith",
                Gender = "F",
                SourcePatientId = "A123"
            };

            var mapper = new FhirMappingService("v1.2", NullLogger<FhirMappingService>.Instance);
            var patient = mapper.MapPatient(model);

            Assert.Equal(AdministrativeGender.Female, patient.Gender);
            Assert.Equal("Smith", patient.Name[0].Family);
            Assert.Equal("Alice", patient.Name[0].Given.First());
            Assert.Equal("A123", patient.Identifier[0].Value);
            Assert.NotNull(patient.Meta);
            Assert.Contains(patient.Meta.Tag, t => t.Code == "nhanes-upload");
        }

        [Fact]
        public void MapObservations_ShouldReturnThreeObservations()
        {
            var model = new CSVModel
{
    SourcePatientId = "A123",
    FirstName = "Alice",
    LastName = "Smith",
    Gender = "F",
    Timestamp = DateTime.Parse("2025-09-23"),   // ✅ Fix
    WBC = 5.5m,
    RBC = 4.2m,
    HB = 13.1m
};

var mapper = new FhirMappingService("v1.2", NullLogger<FhirMappingService>.Instance);  // ✅ Fix
            var obs = mapper.MapObservations(model, "A123");

            Assert.Equal(3, obs.Count);

            Assert.Contains(obs, o => o.Code.Coding[0].Code == "6690-2"); // WBC
            Assert.Contains(obs, o => o.Code.Coding[0].Code == "789-8");  // RBC
            Assert.Contains(obs, o => o.Code.Coding[0].Code == "718-7");  // HB

            Assert.All(obs, o => Assert.Equal("Patient/A123", o.Subject.Reference));
            Assert.Contains(obs, o => o.Value is Quantity q && q.Unit == "g/dL" && q.Value == 13.1m);
            Assert.All(obs, o => Assert.NotNull(o.Meta));
            Assert.All(obs, o => Assert.Contains(o.Meta.Tag, t => t.Code == "nhanes-upload"));
        }
    }
}


