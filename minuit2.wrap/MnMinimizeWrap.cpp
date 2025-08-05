#include "MnMinimizeWrap.h"

ROOT::Minuit2::FunctionMinimum ROOT::Minuit2::MnMinimizeWrap::Run(unsigned int maximumFunctionCalls, double tolerance)
{
    return this->operator()(maximumFunctionCalls, tolerance);
}
