#ifndef MN_MINIMIZE_WRAP_H_
#define MN_MINIMIZE_WRAP_H_

#include "minuit2/MnMinimize.h"
#include "minuit2/FunctionMinimum.h"
#include "FCNWrap.h"

namespace ROOT
{
    namespace Minuit2
    {
        class MnMinimizeWrap : public MnMinimize, public MinimizationRunner
        {
        public:
            MnMinimizeWrap(const FCNWrap &function, const MnUserParameterState &parameterState, const MnStrategy &strategy = MnStrategy(1))
                : MnMinimize(function, parameterState, strategy)
            {
            }

        protected:
            ROOT::Minuit2::MnApplication& GetApplication() { return *this; }
        };
    }
}

#endif
