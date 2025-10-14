#ifndef MN_SIMPLEX_WRAP_H_
#define MN_SIMPLEX_WRAP_H_

#include "minuit2/MnSimplex.h"
#include "minuit2/FunctionMinimum.h"
#include "FCNWrap.h"

namespace ROOT
{
    namespace Minuit2
    {
        class MnSimplexWrap : public MnSimplex, public MinimizationRunner
        {
        public:
            MnSimplexWrap(const FCNWrap &function, const MnUserParameterState &parameterState, const MnStrategy &strategy = MnStrategy(1))
                : MnSimplex(function, parameterState, strategy)
            {
            }
        protected:
            ROOT::Minuit2::MnApplication& GetApplication() { return *this; }
        };
    }
}

#endif
