#ifndef _FNCGRADIENTBASEWRAP_H_
#define _FNCGRADIENTBASEWRAP_H_

#include "minuit2/FCNGradientBase.h"
#include <iostream>
namespace ROOT
{
    namespace Minuit2
    {
        class FCNGradientBaseWrap : public FCNGradientBase
        {

        public:
            FCNGradientBaseWrap()
            {
                std::cout << "Hello FCNGradientBaseWrap" << std::endl;
            }

            virtual double Run(std::vector<double> const& v) const;

            virtual std::vector<double> Gradient(const std::vector<double>&) const;


            virtual double Up() const;


            double operator()(std::vector<double> const& v) const override;


        };
    }
}

#endif
