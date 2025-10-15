#ifndef FCN_WRAP_H_
#define FCN_WRAP_H_

#include "minuit2/FCNBase.h"

namespace ROOT
{
    namespace Minuit2
    {
/***
Wrapper around the FCNBase class to allow for the use of the C# API.
This class is exposed via a Director (see SWIG documentation) which allows overriding of the virtual methods.
**/
        class FCNWrap : public FCNBase
        {
        public:
            FCNWrap(){}

            /**
               The Cost function should be overridden by the C# API to provide the Cost result.
                Internally, the Minimizer uses the () operator overload to call the Cost function.
            */
            virtual double Cost(std::vector<double> const &parameterValues) const;

            /**
                Method used by the minimizer to calculate the gradient.
                This method is intentionally not exposed via the C# API as it also contains the logic for aborting the minimization.
            */
            virtual std::vector<double> Gradient(std::vector<double> const &) const final;

            /**
                CalculateGradient should be overridden by the C# API to provide the Gradient result.
                Internally, the minimizer will use the Gradient function to calculate the gradient.
             */
            virtual std::vector<double> CalculateGradient(std::vector<double> const &) const;

            virtual bool HasGradient() const override;

            virtual double Up() const;

            double operator()(std::vector<double> const &parameterValues) const;

            /***
                The Abort Command is used to signal that the Minimizer should abort. When called, the object will throw an Exception dependent on the abort type.
                This Exception is than cought by the MinimizationRunner. Abort should only be used in conjunction with the Minimization Runner.
            **/
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
