# MINUIT2 C# Wrapper
This project creates a C# wrapper for MINUIT2 using SWIG.
SWIG is responsible to convert C# calls into unmanaged C calls and instantiate and manage the C++ code.
To facilitate the use of Minuit2, some wrapper classes have been introduced.

## FCNWrap
FCNWrap is responsible to convert the `Minuit2::FCNBase` class into a C# class.
SWIG uses a [Director](https://www.swig.org/Doc4.0/SWIGPlus.html#SWIGPlus_director_classes_introduction) to override the C++ methods of `FCNBase` from C# - allowing the implementation of custom cost functions..
Unfurtunately, SWIGs Director implementation for C# does not support propagating Exceptions. As FCNWrap is bidirectional (`Cost` and `Gradient` are called from the Minimizer on the C++ side), an exception raised in C# has to be propagated **THROUGH** the C++ code to work as expected.
If this this not properly handled, the exception will circumvent the C++ code and the bubble up on the C# without any handling of the exception on the C++ side. This can lead to memory leaks.

To facilitate correct exception handling behavior, the `CostFunctionAdapter` class in C# is responsible to catch any exception raised in C# and propagate them via the ``FCNWrap::Abort`` method. This method will than throw a C++ exception which will be caught by the MinimizationRunner.


## MinimizationRunner
The MinimizationRunner class provides a simple way to interact with the Minuit2 minimizers. As every Minuit2 minimizer inherits from `ROOT::Minuit2::MnApplication`,
the Runner can be attached to any Minimizer by creating a new Wrapper class which inherits from both the minimizer and the MinimizationRunner.
The Runner is required to ensure that Exceptions raised on the C++ side are correctly propagated to the C# code.
The Runner also does some minor state handling related to exceptions. This is especially important in Multithreading scenarios where multiple Minimizers are running in parallel and errors have to be associated with the correct minimizer.
Dont forget, the SWIG Wrapper is C and any state within that wrapper is shared between all Minimizers (which is why the standard exception handling of SWIG is not sufficient in this case).


## Example of exception propagation

Below is an example of how the exception propagation works. Between the C-Sharp and Minuit2 C++ are the SWIG C#, C and C++ wrapper classes facilitating communication.

``` mermaid
sequenceDiagram

box C-Sharp
    participant MnMinimizer
    participant ICostFunction
    participant CostFunctionAdapter
    end

box Minuit2 C++
    participant FCNWrap
    participant MinimizationRunner
    end


    MnMinimizer->>MinimizationRunner: start Minimization
    activate MnMinimizer
    activate MinimizationRunner
    MinimizationRunner->>FCNWrap: calls operator()
    activate FCNWrap
    FCNWrap->>CostFunctionAdapter: call ValueFor()
    CostFunctionAdapter->>ICostFunction: calls ValueFor()
    ICostFunction->>CostFunctionAdapter: throws Exception
    CostFunctionAdapter->>FCNWrap: catches Exception and calls Abort
    FCNWrap->>MinimizationRunner: throws Exception
    deactivate FCNWrap
    MinimizationRunner->>MnMinimizer: catches Exception and returns RunnerResult
    deactivate MinimizationRunner
    deactivate MnMinimizer
```
