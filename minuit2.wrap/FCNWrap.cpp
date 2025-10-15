#include "FCNWrap.h"
#include "OperationCancelledException.h"
#include <iostream>

double ROOT::Minuit2::FCNWrap::operator()(std::vector<double> const &parameterValues) const
{
    ThrowAbortExeceptionIfRequired();
    double cost =  Cost(parameterValues);
    ThrowAbortExeceptionIfRequired();
    return cost;
}

double ROOT::Minuit2::FCNWrap::Cost(std::vector<double> const &parameterValues) const
{
    return 0;
}

std::vector<double> ROOT::Minuit2::FCNWrap::Gradient(std::vector<double> const &v) const
{
    ThrowAbortExeceptionIfRequired();
    std::vector<double> result =  CalculateGradient(v);
    ThrowAbortExeceptionIfRequired();
    return result;
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

void ROOT::Minuit2::FCNWrap::Abort(bool expected, std::string const &reason)
{
        abort = { true, expected, reason };
}

void ROOT::Minuit2::FCNWrap::ThrowAbortExeceptionIfRequired() const {
    if (!abort.ShouldAbort) return;

    if (abort.IsExpected)
         throw OperationCancelledException(abort.Reason.c_str());
    else
         throw std::exception(abort.Reason.c_str());
}
