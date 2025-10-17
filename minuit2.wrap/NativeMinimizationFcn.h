#ifndef _NATIVE_MINIMIZER_H_
#define _NATIVE_MINIMIZER_H_

#include "FcnFacade.h"
namespace ROOT
{
    namespace Minuit2
    {
        class NativeMinimizationFcn : public FcnFacade
        {
        public:
            NativeMinimizationFcn(std::vector<double> x, std::vector<double> y, std::vector<double> z, bool useGradient) : _x(x), _y(y), _error(z), _useGradient(useGradient){}

            virtual double Cost(std::vector<double> const& parameterValues) const override;
            /**
            CalculateGradient should be overridden by the C# API to provide the Gradient result.
            Internally, the minimizer will use the Gradient function to calculate the gradient.
         */
            virtual std::vector<double> CalculateGradient(std::vector<double> const&) const override;

            virtual double Up() const override;

            virtual bool HasGradient() const override;


            virtual ~NativeMinimizationFcn() {}

        private:
            double ROOT::Minuit2::NativeMinimizationFcn::ResidualFor(int i, std::vector <double> const& parameterValues) const;
            std::vector<double> _x;
            std::vector<double> _y;
            std::vector<double> _error;
            bool _useGradient;
        };
    }
}

#endif
