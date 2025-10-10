#include "MnSimplexWrap.h"

ROOT::Minuit2::FunctionMinimum ROOT::Minuit2::MnSimplexWrap::Run(unsigned int maximumFunctionCalls, double tolerance)
{
    return this->operator()(maximumFunctionCalls, tolerance);
}
