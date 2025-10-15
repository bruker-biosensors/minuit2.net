#include "MnHesseWrap.h"
#include "OperationCancelledException.h"
#include <exception>

MinimizationRunner::RunnerResult ROOT::Minuit2::MnHesseWrap::Update(FunctionMinimum &minimum, const FCNWrap &function, unsigned int maximumFunctionCalls)
{
    try {
        this->operator()(function, minimum, maximumFunctionCalls);
        return MinimizationRunner::Success;
    }
    catch (OperationCancelledException& e) {
        errorMessage = e.what();
        return MinimizationRunner::Cancelled;
    }
    catch (std::exception& e) {
        errorMessage = e.what();
        return MinimizationRunner::Error;
    }

}
