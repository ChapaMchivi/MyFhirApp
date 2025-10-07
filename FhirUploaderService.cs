using System;
using System.Collections.Generic;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using Microsoft.Extensions.Logging;

namespace MyFhirApp
{
    public class FhirUploaderService
    {
        private readonly FhirClient _client;
        private readonly FhirJsonSerializer _serializer = new();
        private readonly ILogger<FhirUploaderService> _logger;

        public FhirUploaderService(string endpoint, ILogger<FhirUploaderService> logger)
        {
            _logger = logger;

            var settings = new FhirClientSettings
            {
                Timeout = 30000,
                PreferredFormat = ResourceFormat.Json
            };

            _client = new FhirClient(endpoint, settings);
        }

        public async System.Threading.Tasks.Task UploadAsync(
            CSVModel record,
            Patient patient,
            List<Observation>? observations,
            bool dryRun)
        {
            // Build a transaction bundle (atomic upload)
            var bundle = new Bundle
            {
                Type = Bundle.BundleType.Transaction,
                Entry = new List<Bundle.EntryComponent>()
            };

            var patientUuid = $"urn:uuid:{Guid.NewGuid()}";

            // Add Patient
            bundle.Entry.Add(new Bundle.EntryComponent
            {
                FullUrl = patientUuid,
                Resource = patient,
                Request = new Bundle.RequestComponent
                {
                    Method = Bundle.HTTPVerb.POST,
                    Url = "Patient"
                }
            });

            // ✅ Prepare a safe patient ID for all logging
            var safePatientId = string.IsNullOrWhiteSpace(record?.SourcePatientId)
                ? "<missing>"
                : record.SourcePatientId!;

            // Add Observations (if any)
            if (observations != null && observations.Count > 0)
            {
                foreach (var obs in observations)
                {
                    obs.Subject = new ResourceReference(patientUuid);

                    bundle.Entry.Add(new Bundle.EntryComponent
                    {
                        Resource = obs,
                        Request = new Bundle.RequestComponent
                        {
                            Method = Bundle.HTTPVerb.POST,
                            Url = "Observation"
                        }
                    });
                }
            }
            else
            {
                _logger.LogWarning("⚠️ No observations provided for patient {PatientId}", safePatientId);
            }

            if (dryRun)
            {
                _logger.LogInformation("🧪 Dry-run Transaction Bundle:\n{Bundle}",
                    _serializer.SerializeToString(bundle));
                return;
            }

            try
            {
                var result = await _client.TransactionAsync(bundle);

                // ✅ Null-safe count: result or Entry may be null
                var returnedCount = result?.Entry?.Count ?? 0;

                _logger.LogInformation(
                    "✅ Transaction completed. Server returned {Count} resources.",
                    returnedCount);
            }
            catch (FhirOperationException ex)
            {
                _logger.LogError(ex,
                    "❌ FHIR error while uploading patient {PatientId}",
                    safePatientId);

                // ✅ Null-safe handling of Outcome
                if (ex.Outcome is OperationOutcome outcome)
                {
                    _logger.LogError("OperationOutcome: {Outcome}",
                        _serializer.SerializeToString(outcome));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Unexpected error while uploading patient {PatientId}",
                    safePatientId);
            }
        }
    }
}