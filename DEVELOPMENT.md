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
* **.NET Runtime**: Version 8.0 or later
* **IDE**: Visual Studio 2022 or JetBrains Rider (recommended)
* **Build Tools**:
  *   [Git](https://git-scm.com/): Required to download [ROOT](https://github.com/root-project/root)/[Minuit2](https://root.cern.ch/doc/master/Minuit2Page.html) source code during the build.
  *   [CMake](https://cmake.org/): Used for building the C++ layer. Version 3.21 or later must be in the system PATH.
  *   [SWIG](https://www.swig.org/): Used to generate C# wrapper code. Must be in the system PATH or specified via the `SWIG_EXECUTABLE` build property.

### Windows

* **C++ Compiler**: [Visual Studio Build Tools](https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2022) (standard) or LLVM/Clang for MinGW
* **Ninja** (optional): A build generator for CMake, required when using the `Windows.NinjaLLVM-MinGW` toolchain.

### macOS

* **C++ Compiler**: [Xcode Command Line Tools](https://developer.apple.com/xcode/resources/) (standard; typically preinstalled)
* **OpenMP** (optional): Required for `minuit2.net.openmp` builds. Can be installed via Homebrew: `brew install libomp`.

## Building the Project

### Build Steps

1. Clone the Minuit2.NET repository
2. (Optional) Create or update `Directory.Build.props.user` to configure your build (e.g., set a custom `Toolchain`)
3. Build the solution or project using `dotnet build` or your IDE. The build process is automated through MSBuild targets and will:
   1. Download [ROOT](https://github.com/root-project/root)/[Minuit2](https://root.cern.ch/doc/master/Minuit2Page.html) source code via git
   2. Configure and build the C++ library
   3. Generate C# wrapper code using SWIG
   4. Compile the .NET library with the generated bindings

### Platform Configuration

The build system automatically detects the current platform and architecture (RID) and builds the corresponding native library.
You can explicitly target a specific platform using the `--runtime` (or `-r`) flag.
