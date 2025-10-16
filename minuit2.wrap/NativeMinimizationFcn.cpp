#include "NativeMinimizationFcn.h"


double ROOT::Minuit2::NativeMinimizationFcn::Cost(std::vector<double> const& parameterValues) const
{
    return 0;
}

std::vector<double> ROOT::Minuit2::NativeMinimizationFcn::CalculateGradient(std::vector<double> const& v) const
{
    return std::vector<double>(v.size(), 0);
}

bool ROOT::Minuit2::NativeMinimizationFcn::HasGradient() const
{
    return false;
}

double ROOT::Minuit2::NativeMinimizationFcn::Up() const
{
    return 1.0;
}
