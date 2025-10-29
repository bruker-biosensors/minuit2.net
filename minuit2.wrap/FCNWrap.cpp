#include "FCNWrap.h"
#include <limits>

double ROOT::Minuit2::FCNWrap::operator()(std::vector<double> const &parameterValues) const
{
    if (shouldAbort.load()) {
        return std::numeric_limits<double>::quiet_NaN();
    }

    return CalculateValue(parameterValues);
}

double ROOT::Minuit2::FCNWrap::CalculateValue(std::vector<double> const &parameterValues) const
{
    return 0.0;
}

std::vector<double> ROOT::Minuit2::FCNWrap::Gradient(std::vector<double> const &parameterValues) const
{
    if (shouldAbort.load()) {
        return std::vector<double>(parameterValues.size(), std::numeric_limits<double>::quiet_NaN());
    }

    return CalculateGradient(parameterValues);
}

std::vector<double> ROOT::Minuit2::FCNWrap::CalculateGradient(std::vector<double> const &parameterValues) const
{
    return std::vector<double>(parameterValues.size(), 0.0);
}

bool ROOT::Minuit2::FCNWrap::HasGradient() const
{
    return false;
}

double ROOT::Minuit2::FCNWrap::Up() const
{
    return 1.0;
}

void ROOT::Minuit2::FCNWrap::Abort()
{
    shouldAbort.store(true);
}
