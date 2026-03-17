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


echo *** Publishing MacOS Intel silicon
dotnet publish -c Debug -p:Platform="Any CPU" -r osx-x64 --framework net10.0 --self-contained true /p:PublishTrimmed=false -p:PublishSingleFile=true -p:PublishReadyToRunShowWarnings=true
echo *** Creating tar file
bsdtar -cJf publish/sentinel.macos-x64.tar.xz -C Sentinel\bin\Debug\net10.0\osx-x64\publish\ *
echo *** Completed MacOS Intel silicon

echo *** Publishing MacOS Apple silicon
dotnet publish -c Debug -p:Platform="Any CPU" -r osx-arm64 --framework net10.0 --self-contained true /p:PublishTrimmed=false -p:PublishSingleFile=true 
echo *** Creating tar file
bsdtar -cJf publish/sentinel.macos-arm64.tar.xz -C Sentinel\bin\Debug\net10.0\osx-arm64\publish\ *
echo *** Completed MacOS Apple silicon

echo *** Publishing Linux ARM32
dotnet publish -c Debug /p:Platform="Any CPU" -r linux-arm --framework net10.0 --self-contained true /p:PublishTrimmed=false /p:PublishSingleFile=true 
echo *** Creating tar file
bsdtar -cJf publish/sentinel.linux-arm32.needsexec.tar.xz -C Sentinel\bin\Debug\net10.0\linux-arm\publish\ *
echo *** Completed Linux ARM32

echo *** Publishing Linux ARM64
dotnet publish -c Debug /p:Platform="Any CPU" -r linux-arm64 --framework net10.0 --self-contained true /p:PublishTrimmed=false /p:PublishSingleFile=true
echo *** Creating tar file
bsdtar -cJf publish/sentinel.linux-arm64.needsexec.tar.xz -C Sentinel\bin\Debug\net10.0\linux-arm64\publish\ *
echo *** Completed Linux ARM64

echo *** Publishing Linux X64
dotnet publish -c Debug /p:Platform="Any CPU" -r linux-x64 --framework net10.0 --self-contained true /p:PublishTrimmed=false /p:PublishSingleFile=true
echo *** Creating tar file
bsdtar -cJf publish/sentinel.linux-x64.needsexec.tar.xz -C Sentinel\bin\Debug\net10.0\linux-x64\publish\ *
echo *** Completed Linux X64


echo *** Publishing Windows ARM 64bit
dotnet publish Sentinel/Sentinel.csproj -c Debug /p:Platform="Any CPU" -r win-arm64 --framework net10.0 --self-contained true /p:DefineConstants=WINDOWS /p:PublishTrimmed=false /p:PublishSingleFile=true -o ./publish/SentinelArm64/
echo *** Completed Windows ARM 64bit publish

echo *** Publishing Windows Intel 64bit
dotnet publish Sentinel/Sentinel.csproj -c Debug /p:Platform="Any CPU" -r win-x64   --framework net10.0 --self-contained true /p:DefineConstants=WINDOWS /p:PublishTrimmed=false /p:PublishSingleFile=true -o ./publish/Sentinelx64/
echo *** Completed Windows Intel 64bit publish

rem The Intel 32bit version serves on ARM64 as well because .NET doesn't support publishing 32bit Windows-Arm executables
echo *** Publishing Windows Intel 32bit
dotnet publish Sentinel/Sentinel.csproj -c Debug /p:Platform="Any CPU" -r win-x86   --framework net10.0 --self-contained true /p:DefineConstants=WINDOWS /p:PublishTrimmed=false /p:PublishSingleFile=true -o ./publish/Sentinelx86/
echo *** Completed Windows Intel 32bit publish

echo *** Creating Windows installer
cd J:\ASCOMSentinel\Setup
"C:\Program Files (x86)\Inno Script Studio\isstudio.exe" -compile "Sentinel.iss"
cd ..

echo *** Builds complete
pause