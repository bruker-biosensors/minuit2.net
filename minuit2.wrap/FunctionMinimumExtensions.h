#ifndef FUNCTION_MINIMUM_EXTENSIONS_H_
#define FUNCTION_MINIMUM_EXTENSIONS_H_

#include "Minuit2/FunctionMinimum.h"

namespace ROOT 
{
    namespace Minuit2 
    {
        class FunctionMinimumExtensions
        {
        public:
            static FunctionMinimum Copy(const FunctionMinimum& minimum)
            {
                // Creates a shallow copy
                const MinimumSeed& seed = minimum.Seed();
                const std::vector<MinimumState>& states = minimum.States();

                FunctionMinimum::Status status = FunctionMinimum::MnValid;
                if (minimum.IsAboveMaxEdm())
                    status = FunctionMinimum::MnAboveMaxEdm;
                else if (minimum.HasReachedCallLimit())
                    status = FunctionMinimum::MnReachedCallLimit;

                return FunctionMinimum(seed, std::span<const MinimumState>(states.data(), states.size()), minimum.Up(), status);
            }
        };
    }
}

#endif
