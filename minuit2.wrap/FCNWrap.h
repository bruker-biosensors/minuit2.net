#ifndef FCN_WRAP_H_
#define FCN_WRAP_H_

#include "Minuit2/FCNBase.h"
#include <exception>

namespace ROOT
{
	namespace Minuit2
	{
		class FCNWrap : public FCNBase
		{
		public:
			FCNWrap() {}

			double operator()(const std::vector<double>& parameterValues) const override final { return Value(parameterValues); }

			virtual double Value(const std::vector<double>& parameterValues) const = 0;

			virtual std::vector<double> Gradient(const std::vector<double>& parameterValues) const override { return {}; }

			virtual bool HasGradient() const override { return false; }

			virtual double Up() const override { return 1.0; }

			void Abort() const { throw std::exception("Abort"); }

			virtual ~FCNWrap() {}
		};
	}
}

#endif
