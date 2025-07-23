cmake_minimum_required(VERSION 3.18)
if (POLICY CMP0122)
  cmake_policy(SET CMP0122 NEW)
endif()
set(CMAKE_CXX_STANDARD 17)

project(minuit2.net)

include(FetchContent)

SET(minuit2_standalone ON CACHE BOOL "minuit2_standalone Option" FORCE)

FetchContent_Declare(
    root_minuit2
    GIT_REPOSITORY https://github.com/root-project/root.git
    GIT_TAG v6-34-04
    SOURCE_SUBDIR math/minuit2
)
FetchContent_MakeAvailable(root_minuit2)

add_subdirectory("minuit2.wrap")
