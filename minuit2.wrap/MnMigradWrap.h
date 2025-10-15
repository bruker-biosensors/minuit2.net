#ifndef MN_MIGRAD_WRAP_H_
#define MN_MIGRAD_WRAP_H_

#include "minuit2/MnMigrad.h"
#include "minuit2/FunctionMinimum.h"
#include "FCNWrap.h"
#include "MinimizationRunner.h"

namespace ROOT
{
    namespace Minuit2
    {
        class MnMigradWrap : public MnMigrad, public MinimizationRunner
        {
        public:
            MnMigradWrap(const FCNWrap &function, const MnUserParameterState &parameterState, const MnStrategy &strategy = MnStrategy(1))
                : MnMigrad(function, parameterState, strategy), MinimizationRunner()
            {
            }

        protected:
            ROOT::Minuit2::MnApplication& GetApplication() { return *this; }
        };
    }
}

#endif
