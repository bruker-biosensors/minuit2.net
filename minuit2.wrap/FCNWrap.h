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
            FCNWrap(){}

            virtual double Cost(std::vector<double> const &parameterValues) const;

            virtual std::vector<double> Gradient(std::vector<double> const &) const final;

            virtual std::vector<double> CalculateGradient(std::vector<double> const &) const;

            virtual bool HasGradient() const override;

            virtual double Up() const;

            double operator()(std::vector<double> const &parameterValues) const;

            void Abort(bool expected, std::string const& reason);

            virtual ~FCNWrap() {}
        private:

            struct AbortCommand
                {
                    bool ShouldAbort = false;
                    bool IsExpected = false;
                    std::string Reason;
                };

            AbortCommand abort = AbortCommand();

            void ThrowAbortExeceptionIfRequired() const;

        };
    }
}

#endif
