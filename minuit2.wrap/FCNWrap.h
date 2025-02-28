#ifndef _FCNWRAP_H_
#define _FCNWRAP_H_

#include "minuit2/FCNBase.h"
#include <iostream>
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

            virtual double Cost(std::vector<double> const& v) const;


            virtual double Up() const;


            double operator()(std::vector<double> const& v) const;


        };
    }
}

#endif
