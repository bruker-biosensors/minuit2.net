#ifndef MIGRADWRAP_H_
#define MIGRADWRAP_H_

#include "minuit2/MnMigrad.h"
#include "minuit2/FunctionMinimum.h"
#include "FCNWrap.h"

namespace ROOT
{
    namespace Minuit2
    {
        class MnMigradWrap : public MnMigrad
        {
            public:
                MnMigradWrap(const FCNWrap& function, const MnUserParameterState& parameterState, const MnStrategy& strategy = MnStrategy(1)) : MnMigrad(function, parameterState, strategy)
                {
                }

                FunctionMinimum Run(unsigned int maximumFunctionCalls = 0, double tolerance = 0.1);
        };
    }
}

#endif
