#ifndef _MIGRADWRAP_H_
#define _MIGRADWRAP_H_

#include "minuit2/MnMigrad.h"
#include "FCNWrap.h"
#include <iostream>

namespace ROOT
{
    namespace Minuit2
    {
        class MnMigradWrap : public MnMigrad
        {
            public:
                MnMigradWrap(const FCNWrap& fcn, const MnUserParameterState& par, const MnStrategy& str = MnStrategy(1)) : MnMigrad(fcn, par, str)
                {
                }

                FunctionMinimum Run(unsigned int maxfcn = 0, double tolerance = 0.1);
        };
    }
}

#endif
