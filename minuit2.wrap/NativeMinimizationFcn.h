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
            NativeMinimizationFcn() {}

            virtual double Cost(std::vector<double> const& parameterValues) const override;
            /**
            CalculateGradient should be overridden by the C# API to provide the Gradient result.
            Internally, the minimizer will use the Gradient function to calculate the gradient.
         */
            virtual std::vector<double> CalculateGradient(std::vector<double> const&) const override;

            virtual double Up() const override;

            virtual bool HasGradient() const override;


            virtual ~NativeMinimizationFcn() {}
        };
    }
}

#endif
