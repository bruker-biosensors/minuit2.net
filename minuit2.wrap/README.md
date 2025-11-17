# Native Minuit2 Code Wrapping

[SWIG](https://www.swig.org/) is used to bridge C# with the native C++ code, enabling managed code to instantiate and 
interact with Minuit2 components. To simplify usage, a set of wrapper classes has been introduced.

## FCNWrap

`FCNWrap` serves as a wrapper for the native `Minuit2::FCNBase` class, enabling its integration and use within the C# code.
SWIG facilitates this interoperability through its [director feature](https://www.swig.org/Doc4.0/SWIGPlus.html#SWIGPlus_director_classes_introduction), 
which allows virtual methods in C++ to be overridden in C#.

Unfortunately, the director implementation for C# does not support exception propagation from managed to unmanaged code. 
Since `FCNWrap` operates bidirectionally — methods implemented in C# are invoked by the C++ minimizers — any exception 
raised on the C# side must be explicitly handled within the native context. Failure to do so will result in exceptions 
bypassing C++ cleanup routines, potentially leaving native objects in an inconsistent state and causing memory leaks.

## Exception Handling

Exception handling is implemented in the `CostFunctionAdapter` class, a C# utility that serves as an adapter between 
user-defined cost functions (`ICostFunction`) and `FCNWrap`. The `CostFunctionAdapter` is responsible for intercepting 
any exceptions raised during the execution of C# callbacks.

When such an exception occurs, it invokes the `FCNWrap::Abort` method on the C++ side. This mechanism terminates the 
minimization process by substituting return values in `FCNWrap` with non-finite values, thereby allowing the call stack 
to unwind and return control safely to the managed environment.

On the C# side, the captured exceptions are subsequently unpacked and either handled or rethrown as appropriate. 
The entire process is implemented in a thread-safe manner to ensure correct behavior in multithreaded execution contexts.

``` mermaid
sequenceDiagram

box Minuit2.NET (CSharp)
    participant IMinimizer
    participant ICostFunction
    participant CostFunctionAdapter
    end

box Minuit2 (C++)
    participant FCNWrap
    participant MnApplication (actual Minimizer)
    end

    IMinimizer ->> MnApplication (actual Minimizer): starts
    activate IMinimizer
    activate MnApplication (actual Minimizer)
    MnApplication (actual Minimizer) ->> FCNWrap: calls operator()
    activate FCNWrap
    FCNWrap ->> CostFunctionAdapter: calls CalculateValue()
    CostFunctionAdapter ->> ICostFunction: calls ValueFor()
    ICostFunction ->> CostFunctionAdapter: throws Exception
    CostFunctionAdapter -->> CostFunctionAdapter: catches Exception
    CostFunctionAdapter ->> FCNWrap: calls Abort()
    FCNWrap ->> MnApplication (actual Minimizer): returns NaN
    deactivate FCNWrap
    MnApplication (actual Minimizer) ->> IMinimizer: returns (invalid) FunctionMinimum
    deactivate MnApplication (actual Minimizer)
    IMinimizer -->> IMinimizer: handles or rethrows Exception
    deactivate IMinimizer
```
