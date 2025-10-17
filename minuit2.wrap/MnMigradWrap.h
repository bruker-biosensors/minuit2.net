#ifndef MN_MIGRAD_WRAP_H_
#define MN_MIGRAD_WRAP_H_

#include "minuit2/MnMigrad.h"
#include "minuit2/FunctionMinimum.h"
#include "FCNWrap.h"
#include "MinimizationRunner.h"
#include "NativeMinimizationFcn.h"
#include <FcnFacadeWrapper.h>

namespace ROOT
{
    namespace Minuit2
    {
        class MnMigradWrap : public MnMigrad, public MinimizationRunner
        {
        public:
            MnMigradWrap(const FcnFacade&function, const MnUserParameterState &parameterState, const MnStrategy &strategy = MnStrategy(1))
                :_wrapper(FcnFacadeWrapper(function)), MnMigrad(_wrapper, parameterState, strategy), MinimizationRunner()
            {
            }

        protected:
            ROOT::Minuit2::MnApplication& GetApplication() { return *this; }

        private:
            FcnFacadeWrapper _wrapper;
        };
    }
}

#endif
