cmake_minimum_required(VERSION 3.21)
set(CMAKE_CXX_STANDARD 17)

project(minuit2.net)

include(FetchContent)

SET(minuit2_standalone ON CACHE BOOL "minuit2_standalone Option" FORCE)
SET(minuit2_mpi OFF CACHE BOOL "minuit2_mpi Option" FORCE)
SET(minuit2_omp OFF CACHE BOOL "minuit2_omp Option")
if(use_openmp)
  set(minuit2_omp ${use_openmp})
endif()

cmake_path(ABSOLUTE_PATH native_code_dir NORMALIZE)
cmake_path(ABSOLUTE_PATH root_clone_dir NORMALIZE)

SET(root_minuit2_dir ${root_clone_dir}/math/minuit2)

add_subdirectory(${native_code_dir})
add_subdirectory(${root_minuit2_dir})
