%module(directors="1") Minuit2 //enable directors
%typemap(csclassmodifiers) SWIGTYPE, SWIGTYPE *, SWIGTYPE &, SWIGTYPE &&, SWIGTYPE [], SWIGTYPE (CLASS::*) "internal class"
%pragma(csharp) moduleclassmodifiers="internal class"
%include <swiginterface.i>
%include "stl.i"
%include "std_vector.i"
%include "std_string.i"
%include "typemaps.i"
//%include "cpointer.i"
//%pointer_class(double ,doubleP);
%include "carrays.i"
%array_class(double, DoubleArray);


//%interface_impl(ROOT::Minuit2::GenericFunction);
//%interface_impl(ROOT::Minuit2::FCNBase);
//%typemap(csinterfacemodifiers) FCNBase "internal interface"
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
    #include "Minuit2/MnUserParameterState.h"
    #include "MnMigradWrap.h"
    #include "MnSimplexWrap.h"
    #include "MnMinimizeWrap.h"
    #include "MnHesseWrap.h"

    using namespace ROOT::Minuit2;
%}

%feature("director") FCNWrap;
%include "FCNWrap.h"
%include "Minuit2/MnUserCovariance.h"
%include "Minuit2/MnUserParameterState.h"
%include "Minuit2/FCNBase.h"
%include "Minuit2/FunctionMinimum.h"
%include "MnMigradWrap.h"
%include "MnSimplexWrap.h"
%include "MnMinimizeWrap.h"
%include "MnHesseWrap.h"
