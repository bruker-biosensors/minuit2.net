#ifndef FCN_WRAP_H_
#define FCN_WRAP_H_

#include "minuit2/FCNBase.h"

namespace ROOT
{
    namespace Minuit2
    {
        class FCNWrap : public FCNBase
        {
        public:
            FCNWrap()
            {
            }

            double operator()(std::vector<double> const &parameterValues) const override;

            virtual double Cost(std::vector<double> const &parameterValues) const;

            virtual std::vector<double> Gradient(std::vector<double> const &) const override;

            virtual bool HasGradient() const override;

            virtual double Up() const;


            virtual ~FCNWrap() {}
        };
    }
}

#endif
