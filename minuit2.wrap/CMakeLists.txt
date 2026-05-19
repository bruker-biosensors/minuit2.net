cmake_minimum_required(VERSION 3.21)
set(CMAKE_CXX_STANDARD 17)
project(minuit2.wrap CXX C)

FIND_PACKAGE(SWIG REQUIRED)
include(UseSWIG)

SET_SOURCE_FILES_PROPERTIES(Minuit2.i PROPERTIES CPLUSPLUS ON)

cmake_path(ABSOLUTE_PATH swig_output_dir NORMALIZE)
cmake_path(ABSOLUTE_PATH CMAKE_CURRENT_SOURCE_DIR NORMALIZE)

set(sources Minuit2.i)

if(WIN32)
    list(APPEND sources version.rc)
    set_source_files_properties(version.rc PROPERTIES OBJECT_DEPENDS "${CMAKE_CURRENT_SOURCE_DIR}/version.h")
endif()

SWIG_ADD_LIBRARY(${native_assembly_name}
  LANGUAGE csharp
  OUTPUT_DIR ${swig_output_dir}
  SOURCES ${sources})

target_include_directories(${native_assembly_name} PUBLIC ${CMAKE_CURRENT_SOURCE_DIR})
set_target_properties(${native_assembly_name} PROPERTIES
  SWIG_USE_TARGET_INCLUDE_DIRECTORIES ON
  POSITION_INDEPENDENT_CODE ON
  PREFIX ""
  IMPORT_PREFIX "")

TARGET_LINK_LIBRARIES(${native_assembly_name} PRIVATE Minuit2::Minuit2)
