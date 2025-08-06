cmake_minimum_required(VERSION 3.18)
if (POLICY CMP0122)
  cmake_policy(SET CMP0122 NEW)
endif()
set(CMAKE_CXX_STANDARD 17)

project(minuit2.net)

include(FetchContent)

SET(minuit2_standalone ON CACHE BOOL "minuit2_standalone Option" FORCE)
cmake_path(ABSOLUTE_PATH ROOT_PATH NORMALIZE)
SET(root_minuit2 ${ROOT_PATH}/math/minuit2)

add_subdirectory(${root_minuit2})
add_subdirectory("minuit2.wrap")
