using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;

namespace MyFhirApp
{
    public class FhirMappingService
    {
        private readonly string _pipelineVersion;
        private readonly ILogger<FhirMappingService> _logger;

        public FhirMappingService(string pipelineVersion, ILogger<FhirMappingService> logger)
        {
            _pipelineVersion = pipelineVersion;
            _logger = logger;
        }

        public Patient MapPatient(CSVModel model)
        {
            var patient = new Patient
            {
                Identifier = new List<Identifier>
                {
                    new Identifier("https://hospital.smart.org", model.SourcePatientId)
                },
                Name = new List<HumanName>
                {
                    new HumanName
                    {
                        Family = model.LastName,
                        Given = new[] { model.FirstName }
                    }
                },
                Gender = model.Gender?.Trim().ToUpper() switch
                {
                    "M" or "MALE" => AdministrativeGender.Male,
                    "F" or "FEMALE" => AdministrativeGender.Female,
                    "OTHER" => AdministrativeGender.Other,
                    "UNKNOWN" => AdministrativeGender.Unknown,
                    _ => AdministrativeGender.Unknown
                },
                Meta = new Meta
                {
                    Tag = new List<Coding>
                    {
                        new Coding("https://myorg.org/fhir/tags", "nhanes-upload", "NHANES Upload"),
                        new Coding("https://myorg.org/fhir/version", _pipelineVersion, "Pipeline Version")
                    }
                }
            };

            _logger.LogInformation("🧩 Mapped Patient {PatientId}: {First} {Last}, Gender={Gender}, Version={Version}",
                model.SourcePatientId, model.FirstName, model.LastName, patient.Gender, _pipelineVersion);

            return patient;
        }

        public List<Observation> MapObservations(CSVModel model, string patientId)
        {
            var timestamp = new FhirDateTime(model.Timestamp);
            var subjectRef = new ResourceReference($"Patient/{patientId}");

            var observations = new List<Observation>();

            if (model.WBC.HasValue)
                observations.Add(CreateObservation("6690-2", "White blood cells", model.WBC.Value, "10^3/uL", timestamp, subjectRef));

            if (model.RBC.HasValue)
                observations.Add(CreateObservation("789-8", "Red blood cells", model.RBC.Value, "10^6/uL", timestamp, subjectRef));

            if (model.HB.HasValue)
                observations.Add(CreateObservation("718-7", "Hemoglobin", model.HB.Value, "g/dL", timestamp, subjectRef));

            _logger.LogInformation("🧩 Mapped {Count} Observations for Patient {PatientId} at {Timestamp}",
                observations.Count, patientId, model.Timestamp);

            return observations;
        }

        private Observation CreateObservation(string loincCode, string display, decimal value, string unit, FhirDateTime timestamp, ResourceReference subject)
        {
            var obs = new Observation
            {
                Status = ObservationStatus.Final,
                Code = new CodeableConcept("https://loinc.org", loincCode, display),
                Subject = subject,
                Effective = timestamp,
                Value = new Quantity(value, unit, "https://unitsofmeasure.org"),
                Category = new List<CodeableConcept>
                {
                    new CodeableConcept("https://terminology.hl7.org/CodeSystem/observation-category", "laboratory")
                },
                Meta = new Meta
                {
                    Tag = new List<Coding>
                    {
                        new Coding("https://myorg.org/fhir/tags", "nhanes-upload", "NHANES Upload"),
                        new Coding("https://myorg.org/fhir/version", _pipelineVersion, "Pipeline Version")
                    }
                }
            };

            _logger.LogDebug("   ↳ Created Observation {Display} (LOINC {Code}) Value={Value}{Unit} Timestamp={Timestamp}",
                display, loincCode, value, unit, timestamp);

            return obs;
        }
    }
}