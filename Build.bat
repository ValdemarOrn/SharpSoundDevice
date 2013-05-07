msbuild SharpSoundDevice.sln /p:Configuration=Debug
msbuild SharpSoundDevice.sln /p:Configuration=Release

set ts=%DATE:~11%-%DATE:~8,2%-%DATE:~5,2%

rd Builds\Current /s /q
mkdir Builds\Current
mkdir Builds\Current\Debug
mkdir Builds\Current\Release

cp Debug\SharpSoundDevice.dll Builds\Current\Debug\SharpSoundDevice.dll
cp Debug\SharpSoundDevice.pdb Builds\Current\Debug\SharpSoundDevice.pdb
cp Debug\SharpSoundDevice.xml Builds\Current\Debug\SharpSoundDevice.xml
cp Debug\SharpSoundDevice.VST.dll Builds\Current\Debug\SharpSoundDevice.VST.dll
cp Debug\SharpSoundDevice.VST.pdb Builds\Current\Debug\SharpSoundDevice.VST.pdb
cp BridgeGenerator\bin\Debug\BridgeGenerator.exe Builds\Current\Debug\BridgeGenerator.exe

cp Release\SharpSoundDevice.dll Builds\Current\Release\SharpSoundDevice.dll
cp Release\SharpSoundDevice.pdb Builds\Current\Release\SharpSoundDevice.pdb
cp Release\SharpSoundDevice.xml Builds\Current\Release\SharpSoundDevice.xml
cp Release\SharpSoundDevice.VST.dll Builds\Current\Release\SharpSoundDevice.VST.dll
cp Release\SharpSoundDevice.VST.pdb Builds\Current\Release\SharpSoundDevice.VST.pdb
cp BridgeGenerator\bin\Release\BridgeGenerator.exe Builds\Current\Release\BridgeGenerator.exe

cd Builds\Current
rm ../SharpSoundDevice-%ts%.zip
7z a ../SharpSoundDevice-%ts%.zip -r *.*
cd..
cd..
rd Builds\Current /s /q