#ifndef MN_APPLICATION_WRAP_H_
#define MN_APPLICATION_WRAP_H_

#include "Minuit2/FunctionMinimum.h"
#include "FCNWrap.h"

namespace ROOT
{
    namespace Minuit2
    {
        template <typename MinimizerType>
        class MnApplicationWrap : public MinimizerType
        {
        public:
            MnApplicationWrap(const FCNWrap& function,
                              const MnUserParameterState& parameterState,
                              const MnStrategy& strategy = MnStrategy(1))
                : MinimizerType(function, parameterState, strategy) {}

            FunctionMinimum Run(unsigned int maximumFunctionCalls = 0,
                                double tolerance = 0.1)
            {
                return this->operator()(maximumFunctionCalls, tolerance);
            }
        };
    }
}

#endif
