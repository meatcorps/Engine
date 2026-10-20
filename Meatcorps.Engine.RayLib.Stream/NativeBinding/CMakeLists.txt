cmake_minimum_required(VERSION 3.21)
project(MeatcorpsVideoStreaming LANGUAGES C)
set(CMAKE_C_STANDARD 11)
set(CMAKE_C_STANDARD_REQUIRED ON)

find_package(PkgConfig REQUIRED)
pkg_check_modules(GST REQUIRED IMPORTED_TARGET
    gstreamer-1.0
    gstreamer-app-1.0
    gstreamer-rtsp-server-1.0)

if(APPLE)
    set(MC_BACKEND src/platform/macos/backend.c)
elseif(WIN32)
    set(MC_BACKEND src/platform/windows/backend.c)
elseif(UNIX)
    # Dedicated AMD/Intel GPU-only VA-API implementation plus the previous
    # NVENC and compatibility backend, whose public symbols are renamed.
    set(MC_BACKEND
        src/platform/linux/backend.c
        src/platform/linux/backend_compat.c)
else()
    message(FATAL_ERROR "Unsupported operating system")
endif()

add_library(meatcorps_stream SHARED
    src/stream.c
    src/common.c
    ${MC_BACKEND})
target_compile_definitions(meatcorps_stream PRIVATE MC_BUILD_SHARED)
target_include_directories(meatcorps_stream PRIVATE include src)
target_link_libraries(meatcorps_stream PRIVATE PkgConfig::GST)

if(APPLE)
    foreach(framework CoreFoundation CoreMedia CoreVideo VideoToolbox IOSurface OpenGL)
        find_library(MC_${framework} ${framework} REQUIRED)
        target_link_libraries(meatcorps_stream PRIVATE ${MC_${framework}})
    endforeach()
else()
    pkg_check_modules(GST_GPU REQUIRED IMPORTED_TARGET
        gstreamer-gl-1.0
        gstreamer-video-1.0)
    target_link_libraries(meatcorps_stream PRIVATE PkgConfig::GST_GPU)
endif()

if(UNIX AND NOT APPLE)
    pkg_check_modules(VA_GPU REQUIRED IMPORTED_TARGET
        libavcodec
        libavutil
        libva
        egl
        gl
        libdrm)
    target_link_libraries(meatcorps_stream PRIVATE
        PkgConfig::VA_GPU
        ${CMAKE_DL_LIBS})
endif()

if(WIN32)
    set_target_properties(meatcorps_stream PROPERTIES
            PREFIX ""
            OUTPUT_NAME "meatcorps_stream"
    )
endif()