#ifndef MN_HESSE_WRAP_H_
#define MN_HESSE_WRAP_H_

#include "minuit2/MnHesse.h"
#include "FCNWrap.h"
#include <MinimizationRunner.h>

namespace ROOT
{
    namespace Minuit2
    {
        class MnHesseWrap : public MnHesse
        {
        public:
            MnHesseWrap(const MnStrategy &strategy = MnStrategy(1))
                : MnHesse(strategy)
            {
            }

            MinimizationRunner::RunnerResult Update(FunctionMinimum &minimum, const FcnFacade& function, unsigned int maximumFunctionCalls = 0);
            std::string GetErrorMessage() const { return errorMessage; }

        private:
            std::string errorMessage;
        };
    }
}

#endif
