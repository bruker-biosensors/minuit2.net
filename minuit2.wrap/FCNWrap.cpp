#include "FCNWrap.h"

double ROOT::Minuit2::FCNWrap::operator()(std::vector<double> const& v) const
{
    return Run(v);
}

double ROOT::Minuit2::FCNWrap::Run(std::vector<double> const& v) const
{
    return 0;
}

double ROOT::Minuit2::FCNWrap::Up() const
{
    return 1.0;
}
