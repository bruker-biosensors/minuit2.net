#include "NativeMinimizationFcn.h"

double _model(double x, std::vector<double> c) {
    return c[0] + c[1] * x + c[2] * x * x;
}

std::vector<double> _modelGradient(double x, std::vector<double> c) {
    return {1, x, x*x};
}

double ROOT::Minuit2::NativeMinimizationFcn::ResidualFor(int i, std::vector <double> const& parameterValues) const
{
   return (_y[i] - _model(_x[i], parameterValues)) / _error[i];
}

double ROOT::Minuit2::NativeMinimizationFcn::Cost(std::vector<double> const& parameterValues) const
{
    double sum = 0;
    for (auto i = 0; i < _x.size(); i++)
    {
        double residual = ResidualFor(i, parameterValues);
        sum += residual * residual;
    }

    return sum;
}

std::vector<double> ROOT::Minuit2::NativeMinimizationFcn::CalculateGradient(std::vector<double> const& v) const
{
    std::vector<double> gradientSums = std::vector<double>(3);
    for (int i = 0; i < _x.size(); i++)
    {
        double factor = 2 * ResidualFor(i, v);
        std::vector<double> gradients = _modelGradient(_x[i], v);
        for (int j = 0; j < v.size(); j++)
            gradientSums[j] -= factor * gradients[j] / _error[i];
    }

    return gradientSums;
}

bool ROOT::Minuit2::NativeMinimizationFcn::HasGradient() const
{
    return _useGradient;
}

double ROOT::Minuit2::NativeMinimizationFcn::Up() const
{
    return 1.0;
}
