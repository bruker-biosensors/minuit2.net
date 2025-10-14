#ifndef MINIMIZATIONRUNNER_H_
#define MINIMIZATIONRUNNER_H_

#include "Minuit2/MnApplication.h"
#include "minuit2/FunctionMinimum.h"
#include<optional>

class MinimizationRunner {
    public:
        enum RunnerResult {
            Cancelled,
            Error,
            Success,
        };
        RunnerResult Run(unsigned int maximumFunctionCalls = 0, double tolerance = 0.1);
        ROOT::Minuit2::FunctionMinimum GetFunctionMinimum() const { return minimum.value(); }
        std::string GetErrorMessage() const { return errorMessage; }

        ~MinimizationRunner() {}

    protected:
        virtual ROOT::Minuit2::MnApplication& GetApplication() = 0;

    private:
        std::optional<ROOT::Minuit2::FunctionMinimum> minimum;
        std::string errorMessage;


};

#endif
