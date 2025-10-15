#ifndef OPERATIONCANCELLEDEXCEPTION_H
#define OPERATIONCANCELLEDEXCEPTION_H

#include <exception>
/**
The OperationCancelledException is thrown when the User wants to cancel the minimization via the C# API.
*/
class OperationCancelledException : public std::exception
{
public:
    OperationCancelledException(const char* msg) : std::exception(msg) {}
};

#endif
