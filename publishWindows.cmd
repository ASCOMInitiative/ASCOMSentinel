@echo off
echo *** Setup environment

rmdir /s /q "publish"
mkdir publish

if defined __VCVARSALL_HOST_ARCH (
    echo __VCVARSALL_HOST_ARCH is set to "%__VCVARSALL_HOST_ARCH%"
) else (
    echo __VCVARSALL_HOST_ARCH is NOT set, setting environment
    call "C:\Program Files\Microsoft Visual Studio\18\Community\VC\Auxiliary\Build\vcvarsall.bat" x64
)

cd
cd J:\ASCOMSentinel

rem echo *** Build application
rem MSBuild "Sentinel.slnx" /p:Configuration=Debug /p:Platform="Any CPU" /t:Restore 
rem cd
rem echo *** Completed Build

echo *** Publishing Windows ARM 64bit
dotnet publish Sentinel/Sentinel.csproj -c Debug /p:Platform="Any CPU" -r win-arm64 --framework net10.0 --self-contained true /p:PublishTrimmed=false /p:PublishSingleFile=true -o ./publish/SentinelArm64/
echo *** Completed Windows ARM 64bit publish

echo *** Publishing Windows Intel 64bit
dotnet publish Sentinel/Sentinel.csproj -c Debug /p:Platform="Any CPU" -r win-x64   --framework net10.0 --self-contained true /p:PublishTrimmed=false /p:PublishSingleFile=true -o ./publish/Sentinelx64/
echo *** Completed Windows Intel 64bit publish

rem The Intel 32bit version serves on ARM64 as well because .NET doesn't support publishing 32bit Windows-Arm executables
echo *** Publishing Windows Intel 32bit
dotnet publish Sentinel/Sentinel.csproj -c Debug /p:Platform="Any CPU" -r win-x86   --framework net10.0 --self-contained true /p:PublishTrimmed=false /p:PublishSingleFile=true -o ./publish/Sentinelx86/
echo *** Completed Windows Intel 32bit publish

echo *** Creating Windows installer
cd J:\ASCOMSentinel\Setup
"C:\Program Files (x86)\Inno Script Studio\isstudio.exe" -compile "Sentinel.iss"
cd ..

echo *** Builds complete

pause