#include "MinimizationRunner.h"
#include "OperationCancelledException.h"
#include <exception>
#include <iostream>

MinimizationRunner::RunnerResult MinimizationRunner::Run(unsigned int maximumFunctionCalls, double tolerance)
{
    try {
        ROOT::Minuit2::FunctionMinimum min = GetApplication()(maximumFunctionCalls, tolerance);
        minimum = min;
        return RunnerResult::Success;
    }
    catch(OperationCancelledException &e){
        std::cout << e.what() << std::endl;
        errorMessage = e.what();
        return RunnerResult::Cancelled;
    }
    catch (std::exception &e) {
        errorMessage = e.what();
        return RunnerResult::Error;
    }
}
