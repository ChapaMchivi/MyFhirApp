# MyFhirApp

A modular .NET 8 pipeline for transforming CSV health data into FHIR resources.  
**Part One milestone:** dry‑run validation with structured logging and unit test coverage.

---

## 🧠 About This Project

This repository contains modular C# services for uploading and validating FHIR resources using the [Firely .NET SDK](https://github.com/firely-net/firely-net-sdk). It is designed for public health data automation workflows, with a focus on:

- 🔄 **CSV-to-FHIR mapping** using structured `Patient` and `Observation` resources  
- 🧪 **Dry-run validation** of transaction bundles before committing to a FHIR server  
- 🧼 **Null-safe logging** and robust error handling for HL7/FHIR operations  
- 🧩 **Modular pipeline design** for future integration with duplicate detection, coding validation, and batch processing

The code is structured for clarity, reproducibility, and operational transparency — ideal for analysts and developers working in health informatics, medical coding, or FHIR-based data exchange.

---

## 🔍 Firely .NET SDK Integration

This project uses the [Firely .NET SDK](https://github.com/firely-net/firely-net-sdk), the official open-source toolkit for building HL7 FHIR applications in .NET environments. It provides a developer-friendly way to model, manipulate, and validate FHIR resources using POCO classes.

### 🔑 Key Concepts

- **POCO Classes**: Each FHIR resource (e.g., `Patient`, `Observation`, `Immunization`) is represented as a Plain Old CLR Object — lightweight C# classes with properties and no framework dependencies.

  Example:
  ```csharp
  var patient = new Patient();
  patient.Name.Add(new HumanName { Family = "Scherer", Given = new[] { "Salome" } });
  patient.BirthDate = "1990-01-01";
  ```

- **FHIR Data Model Support**: Full coverage of FHIR resource types across multiple versions (STU3, R4, R4B, R5).
- **RESTful Client**: `FhirClient` enables interaction with FHIR servers using standard HTTP verbs.
- **Serialization**: Built-in support for JSON and XML formats.
- **Validation Engine**: Validates resources against FHIR profiles and constraints.
- **FHIRPath Evaluation**: Enables querying and transformation using FHIRPath expressions.
- **Snapshot Generator**: Supports creation of custom profiles with differential and snapshot views.
- **Terminology Server (lightweight)**: In-memory terminology support for basic use cases.

### 🔧 Project Modules

1. Building HL7 FHIR applications using the Firely .NET SDK  
2. Understanding how the HL7 FHIR model is represented in POCOs  
3. Working with core components of the Firely SDK (client, serializer, validator)

---

## Overview

MyFhirApp is designed to take CSV input (e.g., patient demographics, lab results) and map it into FHIR resources such as `Patient` and `Observation`.  
This first phase focuses on:
- Mapping CSV records into FHIR models
- Dry‑run uploads with structured logging (no live server required)
- Unit tests validating mapping and uploader services

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [xUnit](https://xunit.net/) (already included as a test dependency)
- Git (for version control)

*(Docker is not required until Part Two when integration testing is added.)*

---

## ⚙️ Setup

Clone the repository:
```bash
git clone https://github.com/<your-username>/MyFhirApp.git
cd MyFhirApp
```

Restore dependencies:
```bash
dotnet restore
```

---

## Build & Test

Build the solution:
```bash
dotnet build
```

Run all tests:
```bash
dotnet test
```

For faster feedback during development:
```bash
dotnet watch test
```

---

## Project Structure

```
MyFhirApp/
├── src/
│   └── MyFhirApp/                # Core pipeline code
├── tests/
│   └── MyFhirApp.Tests/          # Unit tests
├── .gitignore
├── README.md
└── MyFhirApp.sln
```

---

## Roadmap (Part One)

- [x] CSV → FHIR resource mapping
- [x] Dry‑run upload with structured logging
- [x] Unit test coverage for mapping and uploader services
- [ ] Expand test assertions (logger output, edge cases)
- [ ] Document integration test strategy (planned for Part Two)

---

## 🚀 Usage Example

Here’s a minimal example of how to invoke the uploader service in dry-run mode:

```csharp
var logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<FhirUploaderService>();
var uploader = new FhirUploaderService("https://example-fhir-server.com", logger);

var patient = new Patient
{
    Identifier = new List<Identifier> { new Identifier("urn:system", "12345") },
    Name = new List<HumanName> { new HumanName { Family = "Doe", Given = new[] { "John" } } },
    Gender = AdministrativeGender.Male,
    BirthDate = "1980-01-01"
};

var observations = new List<Observation>
{
    new Observation
    {
        Code = new CodeableConcept("http://loinc.org", "718-7", "Hemoglobin"),
        Value = new Quantity(13.5m, "g/dL"),
        Status = ObservationStatus.Final
    }
};

await uploader.UploadAsync(
    new CSVModel { SourcePatientId = "12345" },
    patient,
    observations,
    dryRun: true);
```

---

## 📄 License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.

It uses the [Firely .NET SDK](https://github.com/firely-net/firely-net-sdk) for FHIR resource modeling and interaction. The Firely SDK is open source and licensed under the [Apache 2.0 License](https://github.com/firely-net/firely-net-sdk/blob/develop/LICENSE).

---

## 🤝 Contributing Guide

We welcome contributions to improve modularity, test coverage, and integration workflows.

### 🧩 How to Contribute

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/my-improvement`)
3. Commit your changes with clear messages
4. Push to your fork and open a pull request

### ✅ Contribution Tips

- Keep commits focused and atomic
- Add unit tests for new logic
- Document public methods and edge cases
- Use structured logging (`ILogger`) for diagnostics

---

## 🐳 Docker Setup (Part Two Preview)

Docker support will be added in Part Two for integration testing and containerized FHIR server simulation.

### Planned Docker Services

- 🔹 `fhir-server`: HAPI or Firely FHIR server container
- 🔹 `test-runner`: .NET test harness with uploader integration
- 🔹 `data-loader`: Optional CSV ingestion service

### Placeholder `docker-compose.yml`

```yaml
version: '3.8'
services:
  fhir-server:
    image: hapiproject/hapi:latest
    ports:
      - "8080:8080"
    environment:
      - HAPI_FHIR_VERSION=R4
```

*(This section will be expanded once integration tests are scoped.)*

---

## 🧭 Coming Soon

> ⚠️ This section outlines future work and is not part of the current milestone.

- 🔄 Integration test harness with Docker-based FHIR server
- 🧪 Automated validation of server responses
- 🧼 Duplicate detection and patient matching
- 📊 Operational dashboards via Power BI or Grafana
- 📦 CI/CD pipeline for test + deploy

