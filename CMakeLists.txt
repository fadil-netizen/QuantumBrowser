cmake_minimum_required(VERSION 3.15)
project(QuantumBrowser LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Define source files
set(SOURCES
    src/main.cpp
    src/BrowserWindow.cpp
)

set(HEADERS
    src/BrowserWindow.h
)

# Create Windows executable (GUI application, not console)
add_executable(QuantumBrowser WIN32 ${SOURCES} ${HEADERS})

# Link libraries
# WebView2Loader.dll.lib is required. 
# We assume usages of Nuget to install Microsoft.Web.WebView2 into the project root or system path.
# For simplicity in this generated file, we expect the user to ensure WebView2 headers and libs are in the include/lib paths.
target_link_libraries(QuantumBrowser PRIVATE 
    shlwapi 
    user32 
    gdi32
    ole32
    oleaut32
)

# Define UNICODE for modern Windows APIs
target_compile_definitions(QuantumBrowser PRIVATE UNICODE _UNICODE)

# Add include directory for local headers
target_include_directories(QuantumBrowser PRIVATE src)

# NOTE: If building with Visual Studio and NuGet, ensure the Microsoft.Web.WebView2 package is installed.
# You may need to add:  <package id="Microsoft.Web.WebView2" version="1.0.1264.42" targetFramework="native" /> to a packages.config
