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

%exception {
    try {
        $action
    }
    catch(OperationCancelledException &e){
        SWIG_CSharpSetPendingException(SWIG_CSharpApplicationException, e.what());
        return $null;
    }
    catch (std::exception &e) {
        SWIG_CSharpSetPendingException(SWIG_CSharpSystemException, e.what());
        return $null;
    }
}

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
    #include "OperationCancelledException.h"
    #include <exception>

    using namespace ROOT::Minuit2;
%}


%feature("director") FCNWrap;
%ignore ROOT::Minuit2::FCNWrap::Gradient;
%include "FCNWrap.h"
%include "Minuit2/MnUserCovariance.h"
%include "Minuit2/MnUserParameterState.h"
%include "Minuit2/FCNBase.h"
%include "Minuit2/FunctionMinimum.h"
%include "MnMigradWrap.h"
%include "MnSimplexWrap.h"
%include "MnMinimizeWrap.h"
%include "MnHesseWrap.h"
