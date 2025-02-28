#include "FCNWrap.h"

double ROOT::Minuit2::FCNWrap::operator()(std::vector<double> const& v) const
{
    return Cost(v);
}

double ROOT::Minuit2::FCNWrap::Cost(std::vector<double> const& v) const
{
    return 0;
}

double ROOT::Minuit2::FCNWrap::Up() const
{
    return 1.0;
}
