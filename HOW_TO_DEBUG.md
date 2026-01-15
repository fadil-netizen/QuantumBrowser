# How to Debug & Run Quantum Browser

Since we switched to **C# (WinForms)** to avoid large downloads, you have two options:

## Option 1: The "Simple Way" (No Extension Needed)
1.  Open the `bin` folder in the project.
2.  Double-click `QuantumBrowser.exe`.
    *   This will run the browser immediately.

## Option 2: Debugging in VS Code (F5)
To use the "Run" button or F5 in VS Code, you must:
1.  Install the **"C# Dev Kit"** extension by Microsoft.
2.  Once installed, press **F5**.
    *   VS Code will compile the code.
    *   It will attach the debugger.

## Troubleshooting
*   **"Build Failed"**: Ensure you have run the build script at least once or that there are no errors in `QuantumBrowser.cs`.
*   **"WebView2 not found"**: Ensure the `libs` folder exists and contains the WebView2 files (the build script handles this).

Your browser is already built and located at:
`bin/QuantumBrowser.exe`
