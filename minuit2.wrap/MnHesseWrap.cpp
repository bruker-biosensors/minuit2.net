#include "MnHesseWrap.h"
#include "OperationCancelledException.h"
#include <exception>
#include "FcnFacadeWrapper.h"
MinimizationRunner::RunnerResult ROOT::Minuit2::MnHesseWrap::Update(FunctionMinimum &minimum, const FcnFacade&function, unsigned int maximumFunctionCalls)
{
    try {
        auto wrap = FcnFacadeWrapper(function);
        this->operator()(wrap, minimum, maximumFunctionCalls);
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

    return MinimizationRunner::Success;

}
