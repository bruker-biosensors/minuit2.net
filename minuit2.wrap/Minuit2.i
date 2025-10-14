%module(directors="1") Minuit2 //enable directors
%typemap(csclassmodifiers) SWIGTYPE, SWIGTYPE *, SWIGTYPE &, SWIGTYPE &&, SWIGTYPE [], SWIGTYPE (CLASS::*) "internal class"
%pragma(csharp) moduleclassmodifiers="internal class"
%include <swiginterface.i>
%include "stl.i"
%include "std_vector.i"
%include "std_string.i"
%include "typemaps.i"
%include "carrays.i"
%array_class(double, DoubleArray);

%include "Minuit2/MnStrategy.h"
namespace std {
    %template(VectorDouble) vector<double>;
};

%{
    #include "ROOT/RSpan.hxx"
    #include "Minuit2/FCNBase.h"
    #include "FCNWrap.h"
    #include "Minuit2/FunctionMinimum.h"
    #include "Minuit2/MnUserCovariance.h"
    #include "Minuit2/MnStrategy.h"
    #include "Minuit2/MnApplication.h"
    #include "Minuit2/MnUserParameterState.h"
    #include "MnMigradWrap.h"
    #include "MnSimplexWrap.h"
    #include "MnMinimizeWrap.h"
    #include "MnHesseWrap.h"
    #include "OperationCancelledException.h"
    #include "MinimizationRunner.h"
    #include <exception>
    #include <optional>

    using namespace ROOT::Minuit2;
%}


%feature("director") FCNWrap;
%ignore ROOT::Minuit2::FCNWrap::Gradient;
%include "FCNWrap.h"
%include "MinimizationRunner.h"
%include "Minuit2/MnUserCovariance.h"
%include "Minuit2/MnUserParameterState.h"
%include "Minuit2/FCNBase.h"
%include "Minuit2/FunctionMinimum.h"
%include "Minuit2/MnApplication.h"
%include "MnMigradWrap.h"
%include "MnSimplexWrap.h"
%include "MnMinimizeWrap.h"
%include "MnHesseWrap.h"
