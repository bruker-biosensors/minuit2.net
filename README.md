# Minuit2.NET

A .NET wrapper for the Minuit2 library from CERN's [ROOT project](https://root.cern/), enhanced with practical extensions.

## Overview

Minuit2 is a well-established nonlinear minimization C++ library that has been used in high-energy physics for decades. 
It is conceived as a tool to minimize multi-parameter cost functions and analyze their behavior near the minimum to 
compute optimum parameter values and associated uncertainties — efficiently and with statistical rigor. Minuit2.NET 
brings this powerful tool to the .NET ecosystem.

The library provides:
- **Flexible Problem Setup**: Define cost functions with ease (for least squares problems and beyond)
- **Reliable Minimization**: Find cost function minima using robust algorithms like MIGRAD
- **Accurate Error Analysis**: Obtain sound parameter uncertainty estimates
- **Simple Parameter Control**: Configure parameter bounds and constraints intuitively

It extends the original library with:
- Composite cost functions combining parameter-sharing individual cost functions into one
- Support for data with unknown uncertainties 
- Support for cancelling active minimization processes

## Project Structure

- **minuit2.net/**: Main C# library containing the .NET API
- **minuit2.wrap/**: C++ wrapper layer using SWIG for interoperability
- **minuit2.UnitTests/**: Comprehensive unit tests for the library
- **Build.targets**: MSBuild targets for automated building of native components

## Prerequisites

### For Building the Project

1. **Development Environment**:
   - .NET 8.0 SDK or later
   - Visual Studio 2022 or JetBrains Rider (recommended for development)
   - C++ compiler (Visual Studio Build Tools or equivalent)

2. **Build Tools**:
   - **CMake** (version 3.18 or later) - for building the C++ layer
   - **SWIG** (version 4.2.0) - automatically installed via NuGet package `swigwintools`

3. **Platform Requirements**:
   - Windows (x64, x86, or ARM64)
   - AnyCPU configuration is **not supported** - you must use a specific platform target

### Runtime Requirements

- .NET 8.0 runtime
- Visual C++ Redistributable (for the native Minuit2 library)

## Building the Project

The build process is automated through MSBuild targets and will:

1. Download ROOT/Minuit2 source code (v6.34.04) via CMake FetchContent
2. Configure and build the C++ Minuit2 library
3. Generate C# wrapper code using SWIG
4. Compile the .NET library with the generated bindings

### Build Steps

1. Clone the repository
2. Open the solution in your IDE or use command line:
   ```bash
   dotnet build --configuration Release --arch x64
   ```
3. The build system will automatically:
   - Install SWIG tools via NuGet
   - Download and compile Minuit2 from ROOT
   - Generate C# bindings
   - Build the complete .NET library

### Platform Configuration

**Important**: You must specify a platform target (x64, x86, or ARM64). AnyCPU is not supported due to the native 
library dependencies.

In Visual Studio/Rider:
- Select x64, x86, or ARM64 from the platform dropdown
- AnyCPU will produce a build error

## Dependencies

### NuGet Packages
- `swigwintools` (4.2.0) - SWIG interface generator

### External Dependencies (automatically handled)
- ROOT/Minuit2 library (v6.34.04) - downloaded and built during compilation

## Usage

The library provides a high-level C# API for function minimization:

```csharp
// Create a cost function
var costFunction = new MyCostFunction();

// Configure parameters
var parameters = new ParameterConfiguration()
    .AddParameter("param1", initialValue: 1.0, error: 0.1)
    .AddParameter("param2", initialValue: 2.0, error: 0.1);

// Create and configure minimizer
var minimizer = new MigradMinimizer();
var result = minimizer.Minimize(costFunction, parameters);

// Check results
if (result.IsValid)
{
    Console.WriteLine($"Minimum found at: {result.UserState}");
    Console.WriteLine($"Function value: {result.Fval}");
}
```