%module(directors="1") Minuit2 //enable directors
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
%interface_impl(ROOT::Minuit2::FCNBase);
%include "Minuit2/MnStrategy.h"
%include "Minuit2/MnUserParameterState.h"
namespace std {
    %template(VectorDouble) vector<double>;
};

%{
    //#include "Minuit2/GenericFunction.h"
    #include "ROOT/RSpan.hxx"
    #include "Minuit2/FCNBase.h"
    #include "Minuit2/MnMatrixfwd.h"
    //#include "Minuit2/FCNGradientBase.h"
    #include "FCNWrap.h"
    #include "Minuit2/MnUserParameters.h"
    //#include "Minuit2/MnMatrix.h"
    #include "Minuit2/MinimumError.h"
    #include "Minuit2/MinimumState.h"
    #include "Minuit2/MinimumParameters.h"
    #include "Minuit2/FunctionMinimum.h"
    #include "Minuit2/MnUserCovariance.h"
    #include "Minuit2/MnStrategy.h"
    #include "Minuit2/MnUserParameterState.h"
    //#include "Minuit2/MnUserTransformation.h"

    #include "MnMigradWrap.h"

    using namespace ROOT::Minuit2;
%}

%feature("director") FCNWrap;
//%feature("director") ROOT::Minuit2::FCNGradientBaseWrap::Up;
//%feature("director") ROOT::Minuit2::FCNGradientBaseWrap::Gradient;
%include "Minuit2/MnMatrixfwd.h"
%include "Minuit2/LAVector.h"
%include "Minuit2/MinimumParameters.h"
%include "FCNWrap.h"
%include "Minuit2/MnUserParameters.h"
%include "Minuit2/MnUserCovariance.h"
//%include "Minuit2/GenericFunction.h"
%include "Minuit2/FCNBase.h"
%include "Minuit2/FCNBase.h"

%include "Minuit2/FunctionMinimum.h"
%include "MnMigradWrap.h"

//%include "Minuit2/LASymMatrix.h"
%include "Minuit2/MinimumState.h"
%include "Minuit2/MinimumParameters.h"

//%include "Minuit2/FunctionGradient.h"
//%include "Minuit2/MnUserTransformation.h"

