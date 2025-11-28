#ifndef MN_HESSE_WRAP_H_
#define MN_HESSE_WRAP_H_

#include "Minuit2/MnHesse.h"
#include "FCNWrap.h"

namespace ROOT
{
    namespace Minuit2
    {
        class MnHesseWrap : public MnHesse
        {
        public:
            MnHesseWrap(const MnStrategy &strategy = MnStrategy(1))
                : MnHesse(strategy) {}

            void Update(FunctionMinimum &minimum,
                        const FCNWrap &function,
                        unsigned int maximumFunctionCalls = 0) const
            {
                this->operator()(function, minimum, maximumFunctionCalls);
            }
        };
    }
}

#endif
