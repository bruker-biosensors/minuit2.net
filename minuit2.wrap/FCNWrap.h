#ifndef FCN_WRAP_H_
#define FCN_WRAP_H_

#include "minuit2/FCNBase.h"
#include <atomic>

namespace ROOT
{
    namespace Minuit2
    {
        class FCNWrap : public FCNBase
        {
        private:
            std::atomic<bool> shouldAbort = false;

        public:
            FCNWrap()
            {
            }

            virtual double operator()(std::vector<double> const &parameterValues) const override final;

            virtual double CalculateValue(std::vector<double> const &parameterValues) const;

            virtual std::vector<double> Gradient(std::vector<double> const &parameterValues) const override final;

            virtual std::vector<double> CalculateGradient(std::vector<double> const &parameterValues) const;

            virtual bool HasGradient() const override;

            virtual double Up() const override;

            void Abort();

            virtual ~FCNWrap() {}
        };
    }
}

#endif
