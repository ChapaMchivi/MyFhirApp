using CsvHelper.Configuration.Attributes;
using System;

namespace MyFhirApp
{
    public class CSVModel
    {
        [Name("PATIENT_GIVENNAME")]
        public string FirstName { get; set; } = string.Empty;

        [Name("PATIENT_FAMILYNAME")]
        public string LastName { get; set; } = string.Empty;

        [Name("PATIENT_GENDER")]
        public string Gender { get; set; } = string.Empty;

        [Name("PATIENT_ID")]
        public string SourcePatientId { get; set; } = string.Empty;

        [Name("TIMESTAMP")]
        public DateTime Timestamp { get; set; }   // safer for mapping

        [Name("WBC")]
        public decimal? WBC { get; set; }

        [Name("RBC")]
        public decimal? RBC { get; set; }

        [Name("HB")]
        public decimal? HB { get; set; }
    }
}