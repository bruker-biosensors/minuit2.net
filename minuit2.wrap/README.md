# Native Minuit2 Code Wrapping

[SWIG](https://www.swig.org/) is used to facilitate interoperability between C# and the native C++ code, allowing 
managed code to instantiate and interact with Minuit2 components. To streamline this integration, a set of wrapper 
classes has been introduced.

## Interoperability Core: The Cost Function

Native Minuit2 processes — minimization and error calculation algorithms — invoke implementations of the 
`Minuit2::FCNBase` cost function base class. Our implementation of this base class is `FCNWrap`, which acts as the 
C++-side adapter. It declares virtual methods intended to be overridden on the C# side. This cross-language override 
mechanism is enabled by [SWIG’s director feature](https://www.swig.org/Doc4.0/SWIGPlus.html#SWIGPlus_director_classes_introduction).
Additionally, `FCNWrap` provides abort functionality to terminate active processes (see below).

On the C# side, the corresponding implementation of `FCNWrap` — via its SWIG-generated proxy — is the 
`CostFunctionAdapter` class. It serves as the managed-side adapter and wraps the Minuit2.NET `ICostFunction` interface.

## Exception Handling

Unfortunately, SWIG's director implementation for C# does not support exception propagation from managed to unmanaged
code. Since `FCNWrap` operates bidirectionally — methods implemented in C# are invoked by the C++ processes — any
exception raised on the C# side must be explicitly handled within the native context. Failure to do so will result in
exceptions bypassing C++ cleanup routines, potentially leaving native objects in an inconsistent state and causing
memory leaks. This problem is solved in the following way:

The `CostFunctionAdapter` intercepts any exceptions that are thrown during the execution of C# callbacks, including 
cancellation requests. For all methods except `Value` (i.e., all derivative methods), such exceptions trigger 
`FCNWrap::Abort`, which raises an "abort exception" on the C++ side and terminates the active process. To safely return 
control to the managed environment, the exception is caught, and a corresponding "non-success indicator" is returned 
together with a null result.

For the `Value` method, which may be invoked concurrently, this approach is not viable because (OpenMP) parallel regions 
in C++ do not allow exceptions to propagate across threads. Instead, we rely on the fact that non-finite return values 
cause the active process to terminate gracefully, while still returning an actual (though invalid) result.

On the C# side, the intercepted exceptions are unpacked and either handled or rethrown as appropriate. The entire 
exception-handling mechanism is implemented in a thread-safe manner to ensure correct behavior in multithreaded 
execution contexts.

The following sequence diagram illustrates the control flow(s) for the example of a minimization process:

``` mermaid
sequenceDiagram

title Cost Function Value Exception Handling

box C-Sharp
    participant MigradMinimizer
    participant ICostFunction
    participant CostFunctionAdapter
    end

box C++
    participant FCNWrap
    participant MnMigrad
    end

    MigradMinimizer ->> MnMigrad: starts
    activate MigradMinimizer
    activate MnMigrad
    MnMigrad ->> FCNWrap: calls operator()(params)
    FCNWrap ->> CostFunctionAdapter: calls Value(params)
    CostFunctionAdapter ->> ICostFunction: calls ValueFor(params)
    ICostFunction ->> CostFunctionAdapter: throws Exception
    CostFunctionAdapter -->> CostFunctionAdapter: catches Exception
    CostFunctionAdapter ->> FCNWrap: returns NaN
    FCNWrap ->> MnMigrad: returns NaN
    MnMigrad ->> MigradMinimizer: returns <Success: true, Minimum: (invalid) FunctionMinimum>
    deactivate MnMigrad
    MigradMinimizer -->> MigradMinimizer: handles or rethrows Exception
    deactivate MigradMinimizer
```

``` mermaid
sequenceDiagram

title Cost Function Derivative Exception Handling

box C-Sharp
    participant MigradMinimizer
    participant ICostFunction
    participant CostFunctionAdapter
    end

box C++
    participant FCNWrap
    participant MnMigrad
    end

    MigradMinimizer ->> MnMigrad: starts
    activate MigradMinimizer
    activate MnMigrad
    MnMigrad ->> FCNWrap: calls Gradient(params)
    FCNWrap ->> CostFunctionAdapter: calls Gradient(params)
    CostFunctionAdapter ->> ICostFunction: calls GradientFor(params)
    ICostFunction ->> CostFunctionAdapter: throws Exception
    CostFunctionAdapter -->> CostFunctionAdapter: catches Exception
    CostFunctionAdapter ->> FCNWrap: calls Abort()
    FCNWrap ->> MnMigrad: throws AbortException
    MnMigrad ->> MigradMinimizer: returns <Success: false, Minimum: null>
    deactivate MnMigrad
    MigradMinimizer -->> MigradMinimizer: handles or rethrows Exception
    deactivate MigradMinimizer
```