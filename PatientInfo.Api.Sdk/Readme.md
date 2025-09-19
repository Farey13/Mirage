Of course. Here is a complete README file in Markdown format that explains how to use the SDK.

-----

# PatientInfo API SDK

A simple and strongly-typed .NET client library for the PatientInfo API, built using [Refit](https://github.com/reactiveui/refit).

This SDK provides an easy-to-use interface to interact with the PatientInfo API endpoints from any .NET application (ASP.NET Core, WPF, Console, etc.) that supports dependency injection.

-----

## 🚀 Features

  * **Strongly-Typed:** No more magic strings for URLs or manually handling JSON.
  * **Async-First:** All API calls are asynchronous using `Task`.
  * **Dependency Injection Ready:** Includes a simple extension method for easy registration in your `IServiceCollection`.
  * **Lightweight:** Built on top of the fast and popular Refit library.

-----

## 📋 Prerequisites

  * .NET 8 or later.
  * A reference to the `PatientInfo.Api.Sdk` project in your main application.

-----

## ⚙️ Setup and Configuration

Follow these three simple steps to get the SDK configured in your application.

### 1\. Add Configuration

First, add the base URL of your PatientInfo API to your application's configuration file (e.g., `appsettings.json`).

```json
{
  "PatientInfoApi": {
    "BaseUrl": "https://your-api-hostname.com"
  }
}
```

### 2\. Register the Client

Next, register the Refit client in your application's dependency injection container. In an ASP.NET Core application, you would do this in your `Program.cs` file.

```csharp
// Program.cs

using PatientInfo.Api.Sdk;

var builder = WebApplication.CreateBuilder(args);

// Add other services here...

// Register the PatientInfo API client
builder.Services.AddPatientInfoApiClient(builder.Configuration);

var app = builder.Build();

// ...
```

### 3\. Inject and Use

Finally, inject the `IPatientInfoApi` interface into any service, controller, or view model where you need to make API calls.

```csharp
public class MyPatientService
{
    private readonly IPatientInfoApi _patientInfoApi;

    // The interface is injected via the constructor
    public MyPatientService(IPatientInfoApi patientInfoApi)
    {
        _patientInfoApi = patientInfoApi;
    }

    // Now you can use it!
    public async Task DoSomethingWithPatient()
    {
        // ...
    }
}
```

-----

## 💻 Usage Examples

Once injected, you can call the API methods directly.

### Get Patient by Hospital Number

To get a patient's details using their hospital number, create a `HospitalNumber` record and pass it to the `GetByHospitalNumberAsync` method.

```csharp
public async Task<PatientInfoDto?> GetPatientByHospitalId(int id)
{
    try
    {
        var request = new PatientInfo.Api.Sdk.Models.HospitalNumber(id);
        var patient = await _patientInfoApi.GetByHospitalNumberAsync(request);
        
        Console.WriteLine($"Found Patient: {patient.PatientName}");
        return patient;
    }
    catch (Refit.ApiException ex)
    {
        // Handle API errors (e.g., 404 Not Found, 500 Server Error)
        Console.WriteLine($"API Error: {ex.StatusCode} - {ex.Message}");
        return null;
    }
}
```

### Get Patient by National ID

Similarly, to get a patient's details using their national ID, create a `NationalId` record and pass it to the `GetByNationalIdAsync` method.

```csharp
public async Task<PatientInfoDto?> GetPatientByNationalIdentifier(string nationalId)
{
    try
    {
        var request = new PatientInfo.Api.Sdk.Models.NationalId(nationalId);
        var patient = await _patientInfoApi.GetByNationalIdAsync(request);

        Console.WriteLine($"Found Patient: {patient.PatientName} with Hospital #: {patient.HospitalNumber}");
        return patient;
    }
    catch (Refit.ApiException ex)
    {
        Console.WriteLine($"API Error: {ex.StatusCode} - {await ex.GetContentAsAsync<string>()}");
        return null;
    }
}
```

-----

## 📦 Models

### Request Models

  * `HospitalNumber(int Value)`
  * `NationalId(string? Value)`

### Response Model

  * `PatientInfoDto(string? NationalID, string? HospitalNumber, string? PatientName)`

-----

## ⚠️ Error Handling

Refit automatically throws an `ApiException` for any non-successful HTTP status code (i.e., not 2xx). You should wrap your API calls in a `try-catch` block to handle potential API errors like `404 Not Found` or `500 Internal Server Error`. The `ApiException` object contains valuable information, including the `StatusCode` and response content.