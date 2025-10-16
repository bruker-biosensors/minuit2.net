#include "FCNWrap.h"
#include "OperationCancelledException.h"
#include <iostream>

double ROOT::Minuit2::FCNWrap::Cost(std::vector<double> const &parameterValues) const
{
    return 0;
}

std::vector<double> ROOT::Minuit2::FCNWrap::CalculateGradient(std::vector<double> const &v) const
{
    return std::vector<double>(v.size(), 0);
}

bool ROOT::Minuit2::FCNWrap::HasGradient() const
{
    return false;
}

double ROOT::Minuit2::FCNWrap::Up() const
{
    return 1.0;
}
