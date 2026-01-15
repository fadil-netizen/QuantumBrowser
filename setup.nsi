; Script generated for Quantum Browser
; Clean & Organized Installer
; Using MUI2

!include "MUI2.nsh"
!include "FileFunc.nsh"
!include "LogicLib.nsh"

;--------------------------------
;General

  Name "Quantum Browser"
  OutFile "bin\build\QuantumBrowser_OfflineInstaller.exe"
  InstallDir "$PROGRAMFILES64\Quantum Browser"
  
  ; Get installation folder from registry if available
  InstallDirRegKey HKCU "Software\QuantumBrowser" ""

  RequestExecutionLevel admin
  SetCompressor /SOLID lzma

;--------------------------------
;Interface Settings

  !define MUI_ABORTWARNING
  !define MUI_ICON "logo.ico" 
  !define MUI_UNICON "logo.ico"
  
  ; Finish Page - Run Application
  !define MUI_FINISHPAGE_RUN "$INSTDIR\QuantumBrowser.exe"
  !define MUI_FINISHPAGE_RUN_TEXT "Launch Quantum Browser"

;--------------------------------
;Pages

  !insertmacro MUI_PAGE_WELCOME
  !insertmacro MUI_PAGE_LICENSE "README.md"
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  !insertmacro MUI_PAGE_FINISH

  !insertmacro MUI_UNPAGE_WELCOME
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  !insertmacro MUI_UNPAGE_FINISH

;--------------------------------
;Languages
 
  !insertmacro MUI_LANGUAGE "English"

;--------------------------------
;Installer Sections

Section "Quantum Browser Core" SecCore

  SetOutPath "$INSTDIR"
  
  ; 1. Main Executable & Config
  File "bin\QuantumBrowser.exe"
  File "bin\QuantumBrowser.exe.config"
  File "bin\*.ini"
  File "bin\*.txt"
  
  ; 2. Root DLLs (WebView2Loader, etc)
  File "bin\*.dll"
  
  ; 3. Resources (Assets & Runtimes)
  SetOutPath "$INSTDIR\assets"
  File /r "bin\assets\*"
  
  SetOutPath "$INSTDIR\runtimes"
  File /r "bin\runtimes\*"
  
  ; Reset OutPath
  SetOutPath "$INSTDIR"

  ; Store installation folder
  WriteRegStr HKCU "Software\QuantumBrowser" "" $INSTDIR
  
  ; Create Uninstaller
  WriteUninstaller "$INSTDIR\Uninstall.exe"
  
  ; Add/Remove Programs Registry
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\QuantumBrowser" "DisplayName" "Quantum Browser"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\QuantumBrowser" "UninstallString" "$\"$INSTDIR\Uninstall.exe$\""
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\QuantumBrowser" "DisplayIcon" "$INSTDIR\QuantumBrowser.exe"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\QuantumBrowser" "Publisher" "FSL"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\QuantumBrowser" "DisplayVersion" "1.0.0"

  ; Shortcuts
  CreateDirectory "$SMPROGRAMS\Quantum Browser"
  CreateShortcut "$SMPROGRAMS\Quantum Browser\Quantum Browser.lnk" "$INSTDIR\QuantumBrowser.exe"
  CreateShortcut "$SMPROGRAMS\Quantum Browser\Uninstall.lnk" "$INSTDIR\Uninstall.exe"
  CreateShortcut "$DESKTOP\Quantum Browser.lnk" "$INSTDIR\QuantumBrowser.exe"

SectionEnd

Section "WebView2 Runtime" SecWebView2
  ; Embed and Install WebView2 Runtime Bootstrapper
  SetOutPath "$TEMP"
  
  ; Extract the file from our compile-time source to the user's temp folder
  File "MicrosoftEdgeWebview2Setup.exe"
  
  DetailPrint "Checking/Installing WebView2 Runtime..."
  ; Run it silently
  ExecWait '"$TEMP\MicrosoftEdgeWebview2Setup.exe" /silent /install'
  
  ; Cleanup
  Delete "$TEMP\MicrosoftEdgeWebview2Setup.exe"
  
  SetOutPath "$INSTDIR"
SectionEnd

;--------------------------------
;Uninstaller Section

Section "Uninstall"

  ; Remove Files
  Delete "$INSTDIR\QuantumBrowser.exe"
  Delete "$INSTDIR\QuantumBrowser.exe.config"
  Delete "$INSTDIR\*.dll"
  Delete "$INSTDIR\Uninstall.exe"
  
  ; Remove Directories
  RMDir /r "$INSTDIR\assets"
  RMDir /r "$INSTDIR\runtimes"
  RMDir /r "$INSTDIR\extensions"
  RMDir /r "$INSTDIR\locales"
  
  ; Remove Install Directory (only if empty)
  RMDir "$INSTDIR"

  ; Remove Shortcuts
  Delete "$SMPROGRAMS\Quantum Browser\Quantum Browser.lnk"
  Delete "$SMPROGRAMS\Quantum Browser\Uninstall.lnk"
  RMDir "$SMPROGRAMS\Quantum Browser"
  Delete "$DESKTOP\Quantum Browser.lnk"

  ; Remove Registry Keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\QuantumBrowser"
  DeleteRegKey HKCU "Software\QuantumBrowser"

SectionEnd
