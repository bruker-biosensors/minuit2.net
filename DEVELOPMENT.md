# Development Guide

This guide provides the necessary information for developers who want to build the Minuit2.NET library from source,
run tests, or contribute to the project.

## Project Structure

- **minuit2.net/**: Main C# library containing the .NET API
- **minuit2.net/msbuild/**: MSBuild targets for automated building of the .NET library incl. native components
- **minuit2.wrap/**: C++ wrapper layer around the native library using SWIG for interoperability
- **test/minuit2.net.UnitTests/**: Comprehensive unit tests for the .NET library
- **test/minuit2.net.Benchmarks/**: Benchmarks for the .NET library
- **Directory.Build.props**: Solution-wide MSBuild properties and defaults

## Prerequisites

### General (Platform-agnostic)

* **.NET SDK**: Version 10.0.105 or later
* **.NET Runtime**: Version 8.0
* **Build Tools** (must be available in the system PATH):
  *   [Git](https://git-scm.com/): Required to download [ROOT](https://github.com/root-project/root)/[Minuit2](https://root.cern.ch/doc/master/Minuit2Page.html) source code during the build.
  *   [CMake](https://cmake.org/): Used for building the C++ layer (v3.21 or later).
  *   [SWIG](https://www.swig.org/): Used to generate C# wrapper code. Can alternatively be specified via the `SwigExePath` build property.

### Windows

* **C++ Compiler**: MSVC (installed via [Visual Studio Build Tools](https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2022)) or LLVM/Clang for MinGW
* **Ninja** (optional): A build generator for CMake, required when using the `Windows.NinjaLLVM-MinGW` toolchain.

### macOS

* **C++ Compiler**: Apple Clang (installed via [Xcode Command Line Tools](https://developer.apple.com/xcode/resources/))
* **OpenMP** (optional): Required for `UseOpenMP=ON` builds. Can be installed via Homebrew: `brew install libomp`.

## Building the Project

### Build Steps

1. Clone the Minuit2.NET repository
2. (Optional) Customize your build by creating a `Directory.Build.props.user` file. Explore `Directory.Build.props` for available options.
3. Build the solution or project using `dotnet build` or your IDE. The build process is automated through MSBuild targets and will:
   1. Download [ROOT](https://github.com/root-project/root)/[Minuit2](https://root.cern.ch/doc/master/Minuit2Page.html) source code via Git
   2. Configure and build the C++ library
   3. Generate C# wrapper code using SWIG
   4. Compile the .NET library with the generated bindings

### Platform Configuration

The build system automatically detects the current platform and architecture (RID) and creates the corresponding native binary.
You can explicitly target a specific platform using the `--runtime` (or `-r`) flag.
