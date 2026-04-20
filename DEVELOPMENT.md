# Development Guide

This guide provides the necessary information for developers who want to build the `minuit2.net` library from source, run tests, or contribute to the project.

## Project Structure

- **minuit2.net/**: Main C# library containing the .NET API
- **minuit2.wrap/**: C++ wrapper layer using SWIG for interoperability
- **test/minuit2.net.UnitTests/**: Comprehensive unit tests for the .NET library
- **test/minuit2.net.Benchmarks/**: Benchmarks for the .NET library
- **msbuild/**: MSBuild targets for automated building of native components
- **Directory.Build.props**: Solution-wide MSBuild properties and defaults

## Prerequisites

### For Building the Project

1. **Development Environment**:
   - .NET 10.0.105 SDK or later patch
   - Visual Studio 2022 or JetBrains Rider (recommended for development)
   - C++ compiler: Visual Studio Build Tools or LLVM/Clang for MinGW

2. **Build Tools**:
   - [CMake](https://cmake.org/): Used for building the C++ layer. Must be installed and available in the system PATH (version 3.21 or later).
   - [SWIG](https://www.swig.org/): Used to generate C# wrapper code from native C++ headers. Must be in the system PATH or specified via the `SWIG_EXECUTABLE` build property.
   - [Git](https://git-scm.com/): Used to download [ROOT](https://github.com/root-project/root)/[Minuit2](https://root.cern.ch/doc/master/Minuit2Page.html) source code during the build process.
   - [Ninja](https://ninja-build.org/) (optional): A build generator for CMake, required when using the `Windows.NinjaLLVM-MinGW` toolchain.

### Runtime Requirements

- .NET 8.0 runtime

## Building the Project

The build process is automated through MSBuild targets and will:

1. Download [ROOT](https://github.com/root-project/root)/[Minuit2](https://root.cern.ch/doc/master/Minuit2Page.html) source code via git
2. Configure and build the C++ Minuit2 library
3. Generate C# wrapper code using SWIG
4. Compile the .NET library with the generated bindings

### Build Steps

1. Clone the repository
2. (Optional) Create or update `Directory.Build.props.user` to configure your build (e.g., set `Toolchain`)
3. Build the solution or project using `dotnet build` or your IDE. The build system will automatically:
   - Download and compile Minuit2 from [ROOT](https://github.com/root-project/root)
   - Generate C# bindings
   - Build the complete .NET library

### Platform Configuration

The build system automatically detects the current platform and architecture (RID) and builds the corresponding native library.
You can explicitly target a specific platform using the `--runtime` (or `-r`) flag.

### External Dependencies (automatically handled)
- [ROOT](https://github.com/root-project/root)/[Minuit2](https://root.cern.ch/doc/master/Minuit2Page.html) library - downloaded and built during compilation
