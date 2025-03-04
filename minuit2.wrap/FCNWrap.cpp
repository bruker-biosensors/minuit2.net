#include "FCNWrap.h"

double ROOT::Minuit2::FCNWrap::operator()(std::vector<double> const& v) const
{
    return Cost(v);
}

double ROOT::Minuit2::FCNWrap::Cost(std::vector<double> const& v) const
{
    return 0;
}

std::vector<double> ROOT::Minuit2::FCNWrap::Gradient(std::vector<double> const&) const
{
    return std::vector<double>();
}

bool ROOT::Minuit2::FCNWrap::HasGradient() const
{
    return false;
}

double ROOT::Minuit2::FCNWrap::Up() const
{
    return 1.0;
}
