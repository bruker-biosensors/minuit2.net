#ifndef FCN_WRAP_H_
#define FCN_WRAP_H_

#include "Minuit2/FCNBase.h"
#include <atomic>
#include <stdexcept>

namespace ROOT
{
	namespace Minuit2
	{
		class FCNWrap : public FCNBase
		{
		public:
			FCNWrap() {}

			double operator()(const std::vector<double>& parameterValues) const override final
			{
			    return CalculateValue(parameterValues);
			}

			std::vector<double> Gradient(const std::vector<double>& parameterValues) const override final
			{
				auto result = CalculateGradient(parameterValues);
				ThrowIfAbortRequested();
				return result;
			}

			std::vector<double> Hessian(const std::vector<double>& parameterValues) const override final
			{
				auto result = CalculateHessian(parameterValues);
				ThrowIfAbortRequested();
				return result;
			}

			std::vector<double> G2(const std::vector<double>& parameterValues) const override final
			{
				auto result = CalculateG2(parameterValues);
				ThrowIfAbortRequested();
				return result;
			}

			virtual double CalculateValue(const std::vector<double>& parameterValues) const = 0;

			virtual std::vector<double> CalculateGradient(const std::vector<double>& parameterValues) const { return {}; }

			virtual std::vector<double> CalculateHessian(const std::vector<double>& parameterValues) const { return {}; }

			virtual std::vector<double> CalculateG2(const std::vector<double>& parameterValues) const { return {}; }

			virtual bool HasGradient() const override { return false; }

			virtual bool HasHessian() const override { return false; }

			virtual bool HasG2() const override { return false; }

			virtual double Up() const override { return 1.0; }

			void Abort() const noexcept { _abortRequested.store(true, std::memory_order_relaxed); }

		    virtual ~FCNWrap() {}

		private:
		    void ThrowIfAbortRequested() const
		    {
		        if (_abortRequested.exchange(false, std::memory_order_relaxed))
		        {
		            throw std::runtime_error("Abort");
		        }
		    }

			mutable std::atomic<bool> _abortRequested{ false };
		};
	}
}

#endif
