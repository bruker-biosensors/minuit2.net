#include "MnHesseWrap.h"

void ROOT::Minuit2::MnHesseWrap::Update(FunctionMinimum &minimum, const FCNWrap &function, unsigned int maximumFunctionCalls) const
{
	this->operator()(function, minimum, maximumFunctionCalls);
}
