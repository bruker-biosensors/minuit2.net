# MINUIT2 C# Wrapper
This project creates a C# wrapper for MINUIT2 using SWIG.
SWIG is responsible to convert C# calls into unmanaged C calls and instantiate and manage the C++ code.
To facilitate the use of Minuit, some wrapper classes have been introduced.

## FCNWrap
This class is responsible to convert the `Minuit2::FCNBase` class into a C# class.
SWIG uses a [Director](https://www.swig.org/Doc4.0/SWIGPlus.html#SWIGPlus_director_classes_introduction) to allow C# to override C++ methods.
Unfurtunately, SWIGs Director implementation for C# does not support propagating Exceptions. As FCNWrap is bidirectional (`Cost` and `Gradient` are called from the Minimizer on the C++ side), an exception raised in C# has to be propagated **THROUGH** the C++ code to work as expected.
If this this not properly handled, the excpetion will circumvent the C++ code and the bubble up on the C# side with unexpected side effects as the C++ code is unaware of the exception.

To facilitate this behavior, the FCNWrap class is responsible to catch any exception raised in C# and propagate them via the ``FCNWrap::Abort`` method. This method will than throw a C++ exception which will be caught by the MinimizationRunner.


## MinimizationRunner
The MinimizationRunner class provides a simple way to interact with the Minuit2 minimizers. Every Minuit2 minimizer inherits from `ROOT::Minuit2::MnApplication`.
The Runner is required to ensure that Exceptions raised on the C++ side are correctly propagated through the C# code.
The Runner also does some minor state handling related to exceptions. This is especially important in MultiThreading scenarios where multiple Minimizers are running in parallel.
Dont forget, the SWIG Wrapper is C and any state within that wrapper is shared between all Minimizers (which is why the standard exception handling of SWIG is not sufficient in this case).


## Example of exception propagation

``` mermaid
sequenceDiagram

    participant MnMinimizer
    participant ICostFunction
    participant CostFunctionAdapter
    participant SWIGLayer
    participant FCNWrap
    participant MinimizationRunner

    MnMinimizer->>MinimizationRunner: start Minimization
    MinimizationRunner->>FCNWrap: calls operator()
    FCNWrap-->>FCNWrap: calls Cost()
    FCNWrap->>ICostFunction: call ValueFor()
    ICostFunction->>CostFunctionAdapter: throws Exception
    CostFunctionAdapter->>FCNWrap: catches Exception and calls Abort
    FCNWrap->>MinimizationRunner: throws Exception
    MinimizationRunner->>MnMinimizer: catches Exception and returns RunnerResult
    MnMinimizer-->>MnMinimizer: evaluates Result and throws Exception
```
