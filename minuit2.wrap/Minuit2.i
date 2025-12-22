// Enable SWIG directors for polymorphic behavior across C++ and C#
%module(directors="1") Minuit2

// Make all generated C# classes internal to limit API surface
%typemap(csclassmodifiers) SWIGTYPE, SWIGTYPE *, SWIGTYPE &, SWIGTYPE &&, SWIGTYPE [], SWIGTYPE (CLASS::*) "internal class"
%pragma(csharp) moduleclassmodifiers="internal class"



// Include SWIG helpers and enable support for common C++ STL types
%include <swiginterface.i>
%include "stl.i"
%include "std_vector.i"
%include "std_string.i"
%include "typemaps.i"

// Enable array support for double[] in C#
%include "carrays.i"
%array_class(double, DoubleArray)

// Instantiate std::vector<double> as VectorDouble for C# use
namespace std {
    %template(VectorDouble) vector<double>;
};



// Inject required C++ headers into the wrapper (SWIG preprocessor block)
%{
    #include "ROOT/RSpan.hxx"
    #include "Minuit2/FCNBase.h"
    #include "FCNWrap.h"
    #include "Minuit2/FunctionMinimum.h"
    #include "Minuit2/MnUserCovariance.h"
    #include "Minuit2/MnStrategy.h"
    #include "Minuit2/MnUserParameterState.h"
    #include "MnHesseWrap.h"
    #include "FunctionMinimumExtensions.h"

    #include "Minuit2/MnMigrad.h"
    #include "Minuit2/MnSimplex.h"
    #include "Minuit2/MnMinimize.h"
    #include "MnApplicationWrap.h"

    using namespace ROOT::Minuit2;
%}



// Declare C++ types to be wrapped and made accessible in C# (SWIG binding generation)
%feature("director") FCNWrap;
%include "FCNWrap.h"
%include "Minuit2/MnStrategy.h"
%include "Minuit2/MnUserCovariance.h"
%include "Minuit2/MnUserParameterState.h"
%include "Minuit2/FCNBase.h"
%include "Minuit2/FunctionMinimum.h"
%include "MnHesseWrap.h"
%include "FunctionMinimumExtensions.h"

%include "MnApplicationWrap.h"
namespace ROOT {
  namespace Minuit2 {
    %template(MnMigradWrap) MnApplicationWrap<MnMigrad>;
    %template(MnSimplexWrap) MnApplicationWrap<MnSimplex>;
    %template(MnMinimizeWrap) MnApplicationWrap<MnMinimize>;
  }
}
