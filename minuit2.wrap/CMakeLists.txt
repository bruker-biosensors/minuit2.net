cmake_minimum_required(VERSION 3.13)
set(CMAKE_CXX_STANDARD 17)
project(minuit2.wrap CXX C)

set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/minuit2.net)

set(CMAKE_SWIG_FLAGS)
FIND_PACKAGE(SWIG REQUIRED)
include(UseSWIG)

SET_SOURCE_FILES_PROPERTIES(Minuit2.i PROPERTIES CPLUSPLUS ON)

set(SOURCE_FILES
  FCNWrap.cpp
  MnMigradWrap.cpp
  MnHesseWrap.cpp
  Minuit2.i
)

set(HEADER_FILES
  FCNWrap.h
  MnMigradWrap.h
  MnHesseWrap.h
)

SWIG_ADD_LIBRARY(minuit2.wrap
  LANGUAGE csharp
  OUTPUT_DIR ${MINUIT2NET_PATH}/gen
  SOURCES ${SOURCE_FILES} ${HEADER_FILES})

target_include_directories(minuit2.wrap PUBLIC ${CMAKE_CURRENT_SOURCE_DIR})
set_target_properties(minuit2.wrap PROPERTIES
  SWIG_USE_TARGET_INCLUDE_DIRECTORIES ON
  POSITION_INDEPENDENT_CODE ON)

target_include_directories(minuit2.wrap PRIVATE ${root_minuit2_SOURCE_DIR}/Core/Foundation/inc)

TARGET_LINK_LIBRARIES(minuit2.wrap PRIVATE Minuit2::Minuit2)
