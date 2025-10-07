## 📌 Overview
This PR introduces the initial foundation of the MyFhirApp project, delivering the core pipeline for transforming CSV health data into FHIR resources using the Firely .NET SDK.

## 🔧 Key Additions
- **Core Services**
  - `CSVModel.cs` for structured CSV data representation
  - `CsvParserService.cs` and `CsvUploaderService.cs` for parsing and ingestion
  - `FhirMappingService.cs` and `FhirUploaderService.cs` for mapping and uploading to FHIR
- **Configuration & Assets**
  - `appsettings.json` for environment configuration
  - Sample `observation.json` and `observation.xml` for validation
- **Solution Setup**
  - `MyFhirApp.sln` and `MyFhirApp.csproj` for project structure
  - `Program.cs` entry point
- **Unit Tests**
  - Test scaffolding for parser, uploader, and mapping services
  - `MyFhirApp.Tests.csproj` with initial test cases

## ✅ Validation
- Dry-run validation of FHIR resources
- Unit test scaffolding in place for core services
- Builds successfully with .NET 8

## 🎯 Next Steps
- Expand integration testing
- Add Docker support for containerized deployment (Part Two milestone)
- Automate CI/CD with GitHub Actions

