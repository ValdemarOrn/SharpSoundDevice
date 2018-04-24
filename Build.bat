rem This has issues about some bogus? inconsistent method declarations in tuplicated types
rd /S /Q bin
msbuild SharpSoundDevice.sln /p:Configuration=Debug,Platform=x86
msbuild SharpSoundDevice.sln /p:Configuration=Release,Platform=x86
msbuild SharpSoundDevice.sln /p:Configuration=Debug,Platform=x64
msbuild SharpSoundDevice.sln /p:Configuration=Release,Platform=x64

set ts=%DATE:~6,4%-%DATE:~3,2%-%DATE:~0,2%

rd Builds\Current /s /q
mkdir Builds\Current
xcopy bin Builds\Current\ /s /e /h

cd Builds\Current
rm ../SharpSoundDevice-%ts%.zip
7z a ../SharpSoundDevice-%ts%.zip -r *.*
cd..
cd..
rd /S /Q Builds\Current