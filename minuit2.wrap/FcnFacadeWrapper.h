#ifndef _FCN_FACADE_WRAPPER_
#define  _FCN_FACADE_WRAPPER_

#include "Minuit2/FCNBase.h"
#include "FcnFacade.h"
namespace ROOT
{
    namespace Minuit2
    {
        class FcnFacadeWrapper : public FCNBase{
        public:

            FcnFacadeWrapper(FcnFacade const& facade) : _facade(facade){}

            double operator()(std::vector<double> const& parameterValues) const {
                _facade.ThrowAbortExeceptionIfRequired();
                auto cost = _facade.Cost(parameterValues);
                _facade.ThrowAbortExeceptionIfRequired();
                return cost;
            }

            std::vector<double> Gradient(std::vector<double> const&v) const {
                _facade.ThrowAbortExeceptionIfRequired();
                auto gradient = _facade.CalculateGradient(v);
                _facade.ThrowAbortExeceptionIfRequired();
                return gradient;
            }

            bool HasGradient() const {
                return _facade.HasGradient();
            }

            double Up() const {
                return _facade.Up();
            }

        private:
                FcnFacade const& _facade;
        };

    }
}

#endif
