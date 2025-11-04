# Minuit2.NET

[![NuGet Version](https://img.shields.io/nuget/v/minuit2.net.svg)](https://www.nuget.org/packages/minuit2.net/)

A .NET wrapper for the [Minuit2](https://root.cern.ch/doc/master/Minuit2Page.html) library from CERN's [ROOT project](https://root.cern/), enhanced with practical extensions.

> [!NOTE]
> ROOT is licensed under the GNU Lesser General Public License v2.1 (LGPL v2.1).
> Use or distribution of this library in combination with Minuit2 requires adherence to the LGPL terms.

## NuGet Packages

- [minuit2.net](https://www.nuget.org/packages/minuit2.net/) – The standard version that executes C++ backend processes 
  synchronously. Recommended for lightweight tasks where deterministic execution is preferred.

- [minuit2.net.openmp](https://www.nuget.org/packages/minuit2.net.openmp/) – A performance-enhanced variant that 
  leverages OpenMP-based multi-threading for C++ backend processes. Ideal for compute-intensive tasks that benefit from 
  parallel execution.

Both packages share the same API and can be used interchangeably depending on your performance and threading requirements.

## Overview

[Minuit2](https://root.cern.ch/doc/master/Minuit2Page.html) is a well-established nonlinear minimization C++ library that has been used in high-energy physics for decades.
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

Unlike the original library, which exposes a stateful and imperative API, Minuit2.NET adopts a functional approach 
centered on statelessness and referential transparency.

## Project Structure

- **minuit2.net/**: Main C# library containing the .NET API
- **minuit2.wrap/**: C++ wrapper layer using SWIG for interoperability
- **minuit2.UnitTests/**: Comprehensive unit tests for the library
- **Build.targets**: MSBuild targets for automated building of native components
- **Directory.Build.props**: MSBuild properties used to enable/disable OpenMP-based multi-threading support

## Prerequisites

### For Building the Project

1. **Development Environment**:
   - .NET 8.0 SDK or later
   - Visual Studio 2022 or JetBrains Rider (recommended for development)
   - C++ compiler (Visual Studio Build Tools or equivalent)

2. **Build Tools**:
   - **CMake** (version 3.18 or later) - for building the C++ layer
   - **SWIG** (version 4.2.0) - automatically installed via NuGet package `swigwintools`
   - **GIT** - will be called from Build.targets

3. **Platform Requirements**:
   - Windows (x64, x86, ARM64)

### Runtime Requirements

- .NET 8.0 runtime
- Visual C++ Redistributable (for the native [Minuit2](https://root.cern.ch/doc/master/Minuit2Page.html) library)

## Building the Project

The build process is automated through MSBuild targets and will:

1. Download [ROOT](https://github.com/root-project/root)/[Minuit2](https://root.cern.ch/doc/master/Minuit2Page.html) source code (v6.34.04) via git
2. Configure and build the C++ Minuit2 library
3. Generate C# wrapper code using SWIG
4. Compile the .NET library with the generated bindings

### Build Steps

1. Clone the repository
2. The build system will automatically:
   - Install SWIG tools via NuGet
   - Download and compile Minuit2 from [ROOT](https://github.com/root-project/root)
   - Generate C# bindings
   - Build the complete .NET library

### Platform Configuration

**Important**: You must specify a platform target (x64, x86 or ARM64). 
AnyCPU will default to the x64 version of the C++ dll.

## Dependencies

### NuGet Packages
- `swigwintools` (4.2.0) - SWIG interface generator

### External Dependencies (automatically handled)
- [ROOT](https://github.com/root-project/root)/[Minuit2](https://root.cern.ch/doc/master/Minuit2Page.html) library (v6.34.04) - downloaded and built during compilation

## Usage

The following basic example shows how to fit an exponential decay model to observed data:

```csharp
// Define the cost function
var cost = CostFunction.LeastSquares( 
    x: [...],
    y: [...],
    parameters: ["amplitude", "rate", "offset"],
    model: (x, p) => p[0] * Math.Exp(-p[1] * x) + p[2]);

// Configure parameters with initial values and constraints
var parameterConfigurations = new[]
{
    ParameterConfiguration.Variable("amplitude", 1),
    ParameterConfiguration.Variable("rate", 0.1, lowerLimit: 0),
    ParameterConfiguration.Fixed("offset", 0)
};

// Specify the minimizer
var minimizer = Minimizer.Migrad;

// Minimize the cost function for the given configuration
var minimum = minimizer.Minimize(cost, parameterConfigurations);
```
