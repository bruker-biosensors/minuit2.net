#ifndef MINIMIZATIONRUNNER_H_
#define MINIMIZATIONRUNNER_H_

#include "Minuit2/MnApplication.h"
#include "minuit2/FunctionMinimum.h"
#include<optional>

/***
The MinimizationRunner acts as the Bridge used by the C# API to the Minuit2 C++ API.
Minimizers should inherit from this class and implement the GetApplication() method.
Developers on the C# side of the API must make sure not to call the GetFunctionMinimum() method if the RunnerResult is not Successful
**/
class MinimizationRunner {
    public:
        enum RunnerResult {
            Cancelled,
            Error,
            Success,
        };

        /***
            Run the minimizer with the given arguments.
            If RunnerResult is Successful, the FunctionMinimum is available in GetFunctionMinimum()
            If the Runner is Cancelled or Stopped due to an Error (i.e., exception of the minimization function) the error message can be retrieved with GetErrorMessage()
        **/
        RunnerResult Run(unsigned int maximumFunctionCalls = 0, double tolerance = 0.1);

        /***
            Returns the FunctionMinimum if the RunnerResult was Successful.
        **/
        ROOT::Minuit2::FunctionMinimum GetFunctionMinimum() const {
            if (!minimum.has_value()) throw std::exception("No FunctionMinimum available");
            return minimum.value();
        }

        /***
            Returns the Error Message if the RunnerResult was Cancelled or Error.
        **/
        std::string GetErrorMessage() const { return errorMessage; }

        ~MinimizationRunner() {}

    protected:
        /***
            Returns the Minuit2 Application used by the Minimizer.
            Typically the Minimizer inherits from this runner, so GetApplication only needs to return the instance of the object.
        **/
        virtual ROOT::Minuit2::MnApplication& GetApplication() = 0;

    private:
        std::optional<ROOT::Minuit2::FunctionMinimum> minimum;
        std::string errorMessage;


};

#endif
