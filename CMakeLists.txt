cmake_minimum_required(VERSION 3.18)
if (POLICY CMP0122)
  cmake_policy(SET CMP0122 NEW)
endif()
set(CMAKE_CXX_STANDARD 17)

project(minuite2.net)

include(FetchContent)

FetchContent_Declare(
    minuit2
    GIT_REPOSITORY https://github.com/root-project/root.git
    GIT_TAG v6-32-04
    SOURCE_SUBDIR math/minuit2
)
FetchContent_MakeAvailable(minuit2)
add_subdirectory("minuit2.net")
add_subdirectory("minuit2.wrap")

