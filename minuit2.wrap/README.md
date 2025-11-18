# Native Minuit2 Code Wrapping

[SWIG](https://www.swig.org/) is used to facilitate interoperability between C# and the native C++ code, allowing 
managed code to instantiate and interact with Minuit2 components. To streamline this integration, a set of wrapper 
classes has been introduced.

## Interoperability Core: The Cost Function

Native Minuit2 processes — minimization and error estimation algorithms — invoke implementations of the 
`Minuit2::FCNBase` cost function base class. Our implementation of this base class is `FCNWrap`, which acts as the 
C++-side adapter. It declares virtual methods intended to be overridden on the C# side. This cross-language override 
mechanism is enabled by [SWIG’s director feature](https://www.swig.org/Doc4.0/SWIGPlus.html#SWIGPlus_director_classes_introduction).
Additionally, `FCNWrap` provides abort functionality to terminate active processes safely (see [Exception Handling](#exception-handling)).

On the C# side, the corresponding implementation of `FCNWrap` — via its SWIG-generated proxy — is the 
`CostFunctionAdapter` class. It serves as the managed-side adapter and wraps the Minuit2.NET `ICostFunction` interface.

## Exception Handling

Unfortunately, SWIG's director implementation for C# does not support exception propagation from managed to unmanaged
code. Since `FCNWrap` operates bidirectionally — methods implemented in C# are invoked by the C++ processes — any
exception raised on the C# side must be explicitly handled within the native context. Failure to do so will result in
exceptions bypassing C++ cleanup routines, potentially leaving native objects in an inconsistent state and causing
memory leaks. This problem is solved in the following way:

The `CostFunctionAdapter` intercepts any exceptions that are thrown during the execution of C# callbacks, including 
cancellation requests. When such an exception occurs, it triggers `FCNWrap::Abort`, which modifies the state of 
`FCNWrap` to return non-finite values to the calling C++ process, resulting in its immediate termination. This 
mechanism allows the call stack to unwind and return control safely to the managed environment. On the C# side, the 
captured exceptions are unpacked and either handled or rethrown as appropriate. The entire exception handling is 
implemented in a thread-safe manner to ensure correct behavior in multithreaded execution contexts. The following 
sequence diagram illustrates the flow for the example of a minimization process:

``` mermaid
sequenceDiagram

box Minuit2.NET (CSharp)
    participant MigradMinimizer
    participant ICostFunction
    participant CostFunctionAdapter
    end

box Minuit2 (C++)
    participant FCNWrap
    participant MnMigrad
    end

    MigradMinimizer ->> MnMigrad: starts
    activate MigradMinimizer
    activate MnMigrad
    MnMigrad ->> FCNWrap: calls operator()
    activate FCNWrap
    FCNWrap ->> CostFunctionAdapter: calls CalculateValue()
    CostFunctionAdapter ->> ICostFunction: calls ValueFor()
    ICostFunction ->> CostFunctionAdapter: throws Exception
    CostFunctionAdapter -->> CostFunctionAdapter: catches Exception
    CostFunctionAdapter ->> FCNWrap: calls Abort()
    FCNWrap ->> MnMigrad: returns NaN
    deactivate FCNWrap
    MnMigrad ->> MigradMinimizer: returns (invalid) FunctionMinimum
    deactivate MnMigrad
    MigradMinimizer -->> MigradMinimizer: handles or rethrows Exception
    deactivate MigradMinimizer
```
