%module(directors="1") Minuit2

// make all SWIG generated C# wrappers internal so that they arent exposed to the outside.
%typemap(csclassmodifiers) SWIGTYPE, SWIGTYPE *, SWIGTYPE &, SWIGTYPE &&, SWIGTYPE [], SWIGTYPE (CLASS::*) "internal class"
%pragma(csharp) moduleclassmodifiers="internal class"

//Adds translation of standard C++ types to C#
%include <swiginterface.i>
%include "stl.i"
%include "std_vector.i"
%include "std_string.i"
%include "typemaps.i"
%include "carrays.i"
// Specify the C# class name for double arrays translated to C#
%array_class(double, DoubleArray);

%include "Minuit2/MnStrategy.h"

// Specify the C# class name for double vectors translated to C#
namespace std {
    %template(VectorDouble) vector<double>;
};

// Includes required by the C-Wrapper.
// These statements are verbatim copied into Minuit2CSHARP_wrap.c
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

// Definition of classes which should be translated
// FCNWrap will be managed via SWIG director feature to allow overriding of methods
%feature("director") FCNWrap;

// Do not translate the FcnWrap Gradient method - this method should not be able to be changed from the C# side.
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
