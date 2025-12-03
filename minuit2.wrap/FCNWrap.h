#ifndef FCN_WRAP_H_
#define FCN_WRAP_H_

#include "Minuit2/FCNBase.h"
#include <atomic>
#include <exception>
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

            double operator()(const std::vector<double>& parameterValues) const override final
            {
                // With OpenMP enabled, this method is invoked concurrently from multiple threads. Throwing exceptions
                // to abort the calling process immediately is unsafe here: across OpenMP threads and the SWIG C#/C++
                // boundary it leads to crashes/memory corruption. Instead, we return a non‑finite value to gracefully
                // trigger process termination, which may occur only after a few additional calls.

                if (shouldAbort.load())
                {
                    return std::numeric_limits<double>::quiet_NaN();
                }

                return Value(parameterValues);
            }

            virtual double Value(const std::vector<double>& parameterValues) const = 0;

            virtual std::vector<double> Gradient(const std::vector<double>& parameterValues) const override { return {}; }

            virtual bool HasGradient() const override { return false; }

            virtual double Up() const override { return 1.0; }

            void RequestAbort() { shouldAbort.store(true); }

            void AbortImmediately() const { throw std::exception("Abort"); }

            virtual ~FCNWrap() {}
        };
    }
}

#endif
