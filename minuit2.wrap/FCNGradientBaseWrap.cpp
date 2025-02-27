#include "FCNGradientBaseWrap.h"

double ROOT::Minuit2::FCNGradientBaseWrap::operator()(std::vector<double> const& v) const
{
    return Run(v);
}

double ROOT::Minuit2::FCNGradientBaseWrap::Run(std::vector<double> const& v) const
{
    return 0;
}

double ROOT::Minuit2::FCNGradientBaseWrap::Up() const
{
    return 1337;
}

std::vector<double> ROOT::Minuit2::FCNGradientBaseWrap::Gradient(const std::vector<double>& v) const
{
    return v;
}
