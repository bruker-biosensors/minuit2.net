#ifndef _MIGRADWRAP_H_
#define _MIGRADWRAP_H_

#include "minuit2/MnMigrad.h"
#include "minuit2/FcnGradientBase.h"
#include "FCNGradientBaseWrap.h"
#include <iostream>

namespace ROOT
{
    namespace Minuit2
    {
        class MnMigradWrap : public MnMigrad
        {
            public:
                MnMigradWrap(const FCNGradientBaseWrap& fcn, const MnUserParameterState& par, const MnStrategy& str = MnStrategy(1)) : MnMigrad(fcn, par, str)
                {
                    fcn.Up();
                    std::cout << "Hello MnMigradWrap" << std::endl;
                }

                //MnMigradWrap(const FCNGradientBase& fcn, const MnUserParameterState& par, const MnStrategy& str = MnStrategy(1))
                //    : MnMigrad(fcn, par, str)
                //{
                //    fcn.Up();
                //    std::cout << "Hello MnMigradWrap" << std::endl;
                //}

                FunctionMinimum Run(unsigned int maxfcn = 0, double tolerance = 0.1);
        };
    }
}

#endif
