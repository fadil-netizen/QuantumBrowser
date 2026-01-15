# Quantum Browser - Windows Native C++

This project is a native Windows port of the Quantum Browser, built using C++ (Win32 API) and Microsoft Edge WebView2.

## Prerequisites

1.  **Visual Studio 2019 or 2022**
    *   Workload: "Desktop development with C++"
2.  **Microsoft.Web.WebView2** NuGet package.

## How to Build

### Option 1: Visual Studio (Recommended)

1.  Open **Visual Studio**.
2.  Click **"Open a local folder"** and select the `windows_native` folder.
3.  Visual Studio should detect the `CMakeLists.txt` and configure the project.
4.  If it fails to find `WebView2.h`:
    *   Right-click `CMakeLists.txt` -> **Manage Configurations** (or Project Settings).
    *   You may need to manually install the `Microsoft.Web.WebView2` NuGet package or ensure the SDK is linked.
    *   **Alternative**: Create a standard `.sln` solution:
        1.  File -> New -> Project -> "Windows Desktop Application (C++)".
        2.  Name it `QuantumBrowser`.
        3.  Copy the code from `src/` into the new project.
        4.  Project -> Manage NuGet Packages -> Search "WebView2" -> Install `Microsoft.Web.WebView2`.
        5.  Build & Run.

### Option 2: CMake Command Line

```powershell
mkdir build
cd build
cmake ..
cmake --build . --config Release
```

*Note: You must have the WebView2 loader library available in your path or configured in your environment variables.*

## Project Structure

*   `src/main.cpp`: Entry point.
*   `src/BrowserWindow.cpp`: Main window logic and UI layout.
*   `src/BrowserWindow.h`: Header file.
*   `CMakeLists.txt`: CMake build configuration.

## Debugging

### 1. Native C++ Debugging
*   **Visual Studio**: Run the project in **Debug** mode (`F5`). You can set breakpoints in `main.cpp` or `BrowserWindow.cpp` to inspect window creation and message handling.
*   **Logs**: The application logs debug messages to the Visual Studio **Output** window. Look for messages starting with `[QuantumBrowser]`.

### 2. WebView Debugging (DevTools)
*   **F12 / Right-Click -> Inspect**: The WebView2 control is configured to allow debugging. You can right-click any element in the browser window and select **Inspect** to open the standard Edge DevTools. This allows you to debug HTML, CSS, and JavaScript exactly like in the Chrome/Edge browser.

