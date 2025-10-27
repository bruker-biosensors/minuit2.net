#include "MnMigradWrap.h"

ROOT::Minuit2::FunctionMinimum ROOT::Minuit2::MnMigradWrap::Run(unsigned int maximumFunctionCalls, double tolerance)
{
    return this->operator()(maximumFunctionCalls, tolerance);
}
