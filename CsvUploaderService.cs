using System;
using System.Collections.Generic;
using System.Linq;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Logging;

namespace MyFhirApp
{
    public class CsvUploaderService
    {
        private readonly string _fhirEndpoint;
        private readonly string _pipelineVersion;
        private readonly ILogger<CsvUploaderService> _logger;
        private readonly ILogger<CsvParserService> _parserLogger;
        private readonly ILogger<FhirMappingService> _mappingLogger;
        private readonly ILogger<FhirUploaderService> _uploaderLogger;

        public CsvUploaderService(
            string fhirEndpoint,
            string pipelineVersion,
            ILogger<CsvUploaderService> logger,
            ILogger<CsvParserService> parserLogger,
            ILogger<FhirMappingService> mappingLogger,
            ILogger<FhirUploaderService> uploaderLogger)
        {
            _fhirEndpoint = fhirEndpoint;
            _pipelineVersion = pipelineVersion;
            _logger = logger;
            _parserLogger = parserLogger;
            _mappingLogger = mappingLogger;
            _uploaderLogger = uploaderLogger;
        }

        public async System.Threading.Tasks.Task UploadFromCsvAsync(string csvPath, bool dryRun = false)
        {
            var parser = new CsvParserService(_parserLogger);
            var records = parser.LoadAndValidate(csvPath);

            var mapper = new FhirMappingService(_pipelineVersion, _mappingLogger);

            int patientCount = 0;
            int obsCount = 0;

            foreach (var record in records)
            {
                try
                {
                    var patient = mapper.MapPatient(record);
                    var observations = mapper.MapObservations(record, record.SourcePatientId);

                    if (dryRun)
                    {
                        _logger.LogInformation("🧪 Dry-run Patient: {Given} {Family}",
                            patient.Name[0].Given.FirstOrDefault(), patient.Name[0].Family);

                        foreach (var obs in observations)
                        {
                            var quantity = obs.Value as Quantity;
                            _logger.LogInformation(
                                "🧪 Dry-run Observation: {Display} → LOINC: {Loinc}, Value: {Value} {Unit}, Timestamp: {Timestamp}, Version: {Version}",
                                obs.Code?.Text,
                                obs.Code?.Coding?.FirstOrDefault()?.Code,
                                quantity?.Value,
                                quantity?.Unit,
                                obs.Effective?.ToString(),
                                _pipelineVersion);
                        }
                    }
                    else
                    {
                        var uploader = new FhirUploaderService(_fhirEndpoint, _uploaderLogger);
                        await uploader.UploadAsync(record, patient, observations, dryRun: false);
                    }

                    patientCount++;
                    obsCount += observations.Count;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Error processing record {PatientId}", record.SourcePatientId);
                }
            }

            _logger.LogInformation("✅ Upload complete. Processed {PatientCount} patients and {ObsCount} observations.",
                patientCount, obsCount);

            await System.Threading.Tasks.Task.CompletedTask; // silence CS1998
        }
    }
}




// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Hl7.Fhir.Model;
// using Microsoft.Extensions.Logging;

// namespace MyFhirApp
// {
//     public class CsvUploaderService
//     {
//         private readonly string _fhirEndpoint;
//         private readonly string _pipelineVersion;
//         private readonly ILogger<CsvUploaderService> _logger;

//         public CsvUploaderService(string fhirEndpoint, string pipelineVersion, ILogger<CsvUploaderService> logger)
//         {
//             _fhirEndpoint = fhirEndpoint;
//             _pipelineVersion = pipelineVersion;
//             _logger = logger;
//         }

//         public async Task UploadFromCsvAsync(string csvPath, bool dryRun = false)
//         {
//             var parser = new CsvParserService(_logger); 
//             var records = parser.LoadAndValidate(csvPath);

//             var mapper = new FhirMappingService(_pipelineVersion, _logger);

//             int patientCount = 0;
//             int obsCount = 0;

//             foreach (var record in records)
//             {
//                 try
//                 {
//                     var patient = mapper.MapPatient(record);
//                     var observations = mapper.MapObservations(record, record.SourcePatientId);

//                     if (dryRun)
//                     {
//                         _logger.LogInformation("🧪 Dry-run Patient: {Given} {Family}",
//                             patient.Name[0].Given.FirstOrDefault(), patient.Name[0].Family);

//                         foreach (var obs in observations)
//                         {
//                             var quantity = obs.Value as Quantity;
//                             _logger.LogInformation(
//                                 "🧪 Dry-run Observation: {Display} → LOINC: {Loinc}, Value: {Value} {Unit}, Timestamp: {Timestamp}, Version: {Version}",
//                                 obs.Code?.Text,
//                                 obs.Code?.Coding?.FirstOrDefault()?.Code,
//                                 quantity?.Value,
//                                 quantity?.Unit,
//                                 obs.Effective?.ToString(),
//                                 _pipelineVersion);
//                         }
//                     }
//                     else
//                     {
//                         var uploader = new FhirUploaderService(_fhirEndpoint, _logger);
//                         await uploader.UploadAsync(record, patient, observations, dryRun: false);
//                     }

//                     patientCount++;
//                     obsCount += observations.Count;
//                 }
//                 catch (Exception ex)
//                 {
//                     _logger.LogWarning(ex, "⚠️ Error processing record {PatientId}", record.SourcePatientId);
//                 }
//             }

//             _logger.LogInformation("✅ Upload complete. Processed {PatientCount} patients and {ObsCount} observations.",
//                 patientCount, obsCount);

//             await Task.CompletedTask; // ✅ Silences CS1998 warning
//         }
//     }
// }