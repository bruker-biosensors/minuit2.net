#include "MnMigradWrap.h"
#include "minuit2/FunctionMinimum.h"

#include "FCNGradientBaseWrap.h"
#include <iostream>

ROOT::Minuit2::FunctionMinimum ROOT::Minuit2::MnMigradWrap::Run(unsigned int maxfcn, double tolerance)
{
    std::cout << "Hello World" << std::endl;
    return this->operator()(maxfcn, tolerance);
}

