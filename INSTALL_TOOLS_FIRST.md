# ⚠️ Critical Missing Tools ⚠️

The build failed because your computer is **missing the C++ Compiler**. This is required to turn the code into an application (`.exe`).

## How to Fix (Step-by-Step)

### 1. Install Visual Studio 2022 (Required)
You need the compiler from Microsoft.
1.  Download **Visual Studio 2022 Community** (Free) from: [visualstudio.microsoft.com/downloads](https://visualstudio.microsoft.com/downloads/)
2.  Run the installer.
3.  **CRITICAL**: In the "Workloads" tab, check the box:
    *   **Desktop development with C++**
4.  Click **Install**.

### 2. Install VS Code Extension
The error `debug type 'cppvsdbg' is not supported` means your VS Code doesn't know how to debug C++.
1.  Open VS Code.
2.  Click the **Extensions** icon (blocks on the left).
3.  Search for `C++`.
4.  Install the one by **Microsoft** (`ms-vscode.cpptools`).

---

## Once Installed:
1.  Restart VS Code.
2.  Press **F5** again.
    *   It will find the new compiler.
    *   It will build the app.
    *   It will launch the browser.
