#ifndef OPERATIONCANCELLEDEXCEPTION_H
#define OPERATIONCANCELLEDEXCEPTION_H

#include <exception>
class OperationCancelledException : public std::exception
{
public:
    OperationCancelledException(const char* msg) : std::exception(msg) {}
};

#endif
