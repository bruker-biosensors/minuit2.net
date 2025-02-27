cmake_minimum_required(VERSION 3.18)
if (POLICY CMP0122)
  cmake_policy(SET CMP0122 NEW)
endif()
set(CMAKE_CXX_STANDARD 17)

project(minuite2.net)

include(FetchContent)

FetchContent_Declare(
    minuit2
    GIT_REPOSITORY https://github.com/GooFit/Minuit2.git
    GIT_TAG v6-24-00
)
FetchContent_MakeAvailable(minuit2)
add_subdirectory("minuit2.net")
add_subdirectory("minuit2.wrap")

