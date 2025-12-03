#ifndef MN_APPLICATION_WRAP_H_
#define MN_APPLICATION_WRAP_H_

#include "Minuit2/FunctionMinimum.h"
#include "FCNWrap.h"
#include <optional>

namespace ROOT
{
    namespace Minuit2
    {
        struct RunResult
        {
            bool Success;
            std::optional<FunctionMinimum> Minimum;

            RunResult(bool success, std::optional<FunctionMinimum> minimum)
                : Success(success), Minimum(std::move(minimum)) {}

            const FunctionMinimum &FunctionMinimum() const { return Minimum.value(); }
        };

        template <typename MinimizerType>
        class MnApplicationWrap : public MinimizerType
        {
        public:
            MnApplicationWrap(const FCNWrap& function,
                              const MnUserParameterState& parameterState,
                              const MnStrategy& strategy = MnStrategy(1))
                : MinimizerType(function, parameterState, strategy) {}

            RunResult Run(unsigned int maximumFunctionCalls = 0,
                          double tolerance = 0.1)
            {
                try
                {
                    FunctionMinimum minimum = this->operator()(maximumFunctionCalls, tolerance);
                    return RunResult(true, minimum);
                }
                catch (...)
                {
                    return RunResult(false, std::nullopt);
                }
            }
        };
    }
}

#endif
