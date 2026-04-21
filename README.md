# Minuit2.NET

[![NuGet Version](https://img.shields.io/nuget/v/minuit2.net.svg)](https://www.nuget.org/packages/minuit2.net/)

A .NET wrapper for the [Minuit2](https://root.cern.ch/doc/master/Minuit2Page.html) library from CERN's [ROOT project](https://root.cern/), enhanced with practical extensions.

> [!NOTE]
> ROOT is licensed under the GNU Lesser General Public License v2.1 (LGPL v2.1).
> Use or distribution of this library in combination with Minuit2 requires adherence to the LGPL terms.

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

## NuGet Packages

- [minuit2.net](https://www.nuget.org/packages/minuit2.net/) – The standard version that executes C++ backend processes sequentially.
  Recommended for normal tasks where deterministic execution is preferred.

- [minuit2.net.openmp](https://www.nuget.org/packages/minuit2.net.openmp/) – A performance-enhanced variant that leverages OpenMP-based multithreading for C++
  backend processes. Ideal for compute-intensive tasks that benefit from parallel execution.

Both packages share the same API and can be used interchangeably depending on your performance and threading requirements.
They include native binaries for the following runtimes:
- **Windows**: `win-x86`, `win-x64`, `win-arm64`
- **macOS** (12.0 or later): `osx-x64`, `osx-arm64`

## Building from Source

If you want to build the library locally or contribute to the project, please refer to the [Development Guide](DEVELOPMENT.md) for
prerequisites and build instructions.

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
