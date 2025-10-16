#ifndef _FUNCTION_FACADE_H
#define _FUNCTION_FACADE_H
#include <string>
#include <vector>
#include <OperationCancelledException.h>

namespace ROOT
{
    namespace Minuit2
    {
        class FcnFacade {
        public:
            virtual double Cost(std::vector<double> const& parameterValues) const { return 0; }
            /**
            CalculateGradient should be overridden by the C# API to provide the Gradient result.
            Internally, the minimizer will use the Gradient function to calculate the gradient.
         */
            virtual std::vector<double> CalculateGradient(std::vector<double> const&v) const { return v; }

            virtual double Up() const { return 1.0;}

            virtual bool HasGradient() const { return false;}

            virtual void Abort(bool expected, std::string const& reason) {
                abort = { true, expected, reason };
            }

            void ThrowAbortExeceptionIfRequired() const {
                if (!abort.ShouldAbort) return;

                if (abort.IsExpected)
                    throw OperationCancelledException(abort.Reason.c_str());
                else
                    throw std::exception(abort.Reason.c_str());
            }


        private:
            struct AbortCommand
            {
                bool ShouldAbort = false;
                bool IsExpected = false;
                std::string Reason;
            };

            AbortCommand abort = AbortCommand();
        };
    }
}

#endif // !_FUNCTION_FACADE_H
