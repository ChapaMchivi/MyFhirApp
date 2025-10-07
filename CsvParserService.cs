using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Microsoft.Extensions.Logging;

namespace MyFhirApp
{
    public class CsvParserService
    {
        private readonly ILogger<CsvParserService> _logger;

        public CsvParserService(ILogger<CsvParserService> logger)
        {
            _logger = logger;
        }

        public List<CSVModel> LoadAndValidate(string path)
        {
            using var reader = new StreamReader(path);
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
                IgnoreBlankLines = true,
                Delimiter = ","
            };

            using var csv = new CsvReader(reader, config);

            // Register converters
            csv.Context.TypeConverterCache.AddConverter<decimal?>(new NullableDecimalConverter());
            csv.Context.TypeConverterCache.AddConverter<DateTime>(new FlexibleDateTimeConverter());

            csv.Read();
            csv.ReadHeader();

            // ✅ Null-safe join for headers
            _logger.LogInformation("📋 CSV Headers Detected: {Headers}",
                string.Join(", ", csv.HeaderRecord ?? Array.Empty<string>()));

            var records = csv.GetRecords<CSVModel>().ToList();

            var validRecords = records.Where(IsValid).ToList();
            _logger.LogInformation("✅ Loaded {Total} records, {Valid} passed validation, {Invalid} failed.",
                records.Count, validRecords.Count, records.Count - validRecords.Count);

            return validRecords;
        }

        private bool IsValid(CSVModel model)
        {
            var valid = !string.IsNullOrWhiteSpace(model.SourcePatientId)
                        && model.Timestamp != DateTime.MinValue
                        && model.WBC.HasValue && model.WBC > 0
                        && model.RBC.HasValue && model.RBC > 0
                        && model.HB.HasValue && model.HB > 0;

            if (!valid)
            {
                _logger.LogWarning("⚠️ Invalid record skipped. PatientId={PatientId}, Timestamp={Timestamp}",
                    model.SourcePatientId, model.Timestamp);
            }

            return valid;
        }
    }

    // ✅ Custom converter for nullable decimals
    public class NullableDecimalConverter : DefaultTypeConverter
    {
        public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                return value;

            return null;
        }
    }

    // ✅ Custom converter for flexible DateTime parsing
    public class FlexibleDateTimeConverter : DefaultTypeConverter
    {
        private readonly string[] _formats = new[]
        {
            "yyyy-MM-dd",
            "yyyy-MM-dd HH:mm:ss",
            "MM/dd/yyyy",
            "MM/dd/yyyy HH:mm:ss",
            "yyyyMMdd"
        };

        public override object? ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text))
                return DateTime.MinValue;

            if (DateTime.TryParseExact(text, _formats, CultureInfo.InvariantCulture,
                                       DateTimeStyles.AssumeLocal, out var date))
            {
                return date;
            }

            if (DateTime.TryParse(text, out var fallback))
                return fallback;

            throw new FormatException($"Invalid date format: {text}");
        }
    }
}