@echo off
echo ========================================
echo   Quantum Browser - APK Builder
echo ========================================
echo.

REM Check for Android Studio JBR
echo [1/4] Searching for Java JDK...

set "JAVA_PATHS[0]=C:\Program Files\Android\Android Studio\jbr"
set "JAVA_PATHS[1]=C:\Program Files\Android\Android Studio\jre"
set "JAVA_PATHS[2]=C:\Android\Android Studio\jbr"
set "JAVA_PATHS[3]=C:\Android\Android Studio\jre"

set FOUND_JAVA=0

for /L %%i in (0,1,3) do (
    if exist "!JAVA_PATHS[%%i]!\bin\java.exe" (
        set "JAVA_HOME=!JAVA_PATHS[%%i]!"
        set FOUND_JAVA=1
        goto :found
    )
)

:found
if %FOUND_JAVA%==0 (
    echo ERROR: Java JDK not found!
    echo.
    echo Please install Android Studio or set JAVA_HOME manually.
    echo Expected locations:
    echo   - C:\Program Files\Android\Android Studio\jbr
    echo   - C:\Program Files\Android\Android Studio\jre
    echo.
    pause
    exit /b 1
)

echo Found Java at: %JAVA_HOME%
echo.

echo [2/4] Setting environment...
set PATH=%JAVA_HOME%\bin;%PATH%

echo [3/4] Building APK...
echo This may take 2-5 minutes...
echo.

call gradlew.bat clean assembleDebug

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ========================================
    echo   BUILD FAILED!
    echo ========================================
    echo.
    echo Please check the error messages above.
    echo.
    pause
    exit /b 1
)

echo.
echo ========================================
echo   BUILD SUCCESSFUL!
echo ========================================
echo.
echo APK Location:
echo   app\build\outputs\apk\debug\app-debug.apk
echo.
echo File size:
dir /b app\build\outputs\apk\debug\app-debug.apk 2>nul
echo.
echo You can now install this APK on your phone!
echo.
pause
