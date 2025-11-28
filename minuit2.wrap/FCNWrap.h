#ifndef FCN_WRAP_H_
#define FCN_WRAP_H_

#include "Minuit2/FCNBase.h"
#include <atomic>
#include <limits>

namespace ROOT
{
    namespace Minuit2
    {
        class FCNWrap : public FCNBase
        {
        private:
            std::atomic<bool> shouldAbort = false;

        public:
            FCNWrap() {}

            double operator()(std::vector<double> const &parameterValues) const override final
            {
                if (shouldAbort.load())
                {
                    return std::numeric_limits<double>::quiet_NaN();
                }
                
                return CalculateValue(parameterValues);
            }

            virtual double CalculateValue(std::vector<double> const &parameterValues) const = 0;

            std::vector<double> Gradient(std::vector<double> const &parameterValues) const override final
            {
                if (shouldAbort.load())
                {
                    return std::vector<double>(parameterValues.size(), std::numeric_limits<double>::quiet_NaN());
                }
                
                return CalculateGradient(parameterValues);
            }

            virtual std::vector<double> CalculateGradient(std::vector<double> const &parameterValues) const = 0;

			virtual bool HasGradient() const override { return false; }

			virtual double Up() const override { return 1.0; }

            void Abort() { shouldAbort.store(true); }

            virtual ~FCNWrap() {}
        };
    }
}

#endif
