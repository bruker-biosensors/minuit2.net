# minuit2.net

A .NET wrapper for the MINUIT2 minimization library from CERN's ROOT project. This project provides C# bindings for the powerful MINUIT2 numerical minimization and error analysis library, enabling .NET developers to perform sophisticated mathematical optimization tasks.

## Overview

minuit2.net is a C# wrapper around the MINUIT2 library, which is a widely-used tool for function minimization and error analysis in scientific computing. MINUIT2 is part of the ROOT data analysis framework developed at CERN and is commonly used in high-energy physics for fitting experimental data.

This wrapper provides:
- **Function Minimization**: Advanced algorithms for finding function minima
- **Error Analysis**: Sophisticated error calculation and parameter uncertainty estimation
- **Multiple Minimization Strategies**: Support for different optimization algorithms including MIGRAD
- **Composite Cost Functions**: Ability to combine multiple cost functions for complex optimization problems
- **Parameter Management**: Flexible parameter configuration with bounds and constraints

## Key Features

- **MIGRAD Minimizer**: Implementation of the robust MIGRAD minimization algorithm
- **Hesse Error Calculator**: Precise error analysis using the Hesse matrix
- **Least Squares Fitting**: Built-in support for least squares optimization
- **Cost Function Composition**: Combine multiple cost functions for complex scenarios
- **Flexible Parameter Configuration**: Set bounds, fix parameters, and configure optimization strategies
- **Cross-Platform**: Supports x64, x86, and ARM64 architectures

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
- Visual C++ Redistributable (for the native MINUIT2 library)

## Building the Project

The build process is automated through MSBuild targets and will:

1. Download ROOT/MINUIT2 source code (v6.34.04) via CMake FetchContent
2. Configure and build the C++ MINUIT2 library
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
   - Download and compile MINUIT2 from ROOT
   - Generate C# bindings
   - Build the complete .NET library

### Platform Configuration

**Important**: You must specify a platform target (x64, x86, or ARM64). AnyCPU is not supported due to the native library dependencies.

In Visual Studio/Rider:
- Select x64, x86, or ARM64 from the platform dropdown
- AnyCPU will produce a build error

## Dependencies

### NuGet Packages
- `swigwintools` (4.2.0) - SWIG interface generator

### External Dependencies (automatically handled)
- ROOT/MINUIT2 library (v6.34.04) - downloaded and built during compilation

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