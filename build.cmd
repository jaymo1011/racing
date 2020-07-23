@echo off
pushd Client
dotnet publish -c Release
popd

pushd Server
dotnet publish -c Release
popd

rmdir /s /q dist
mkdir dist

xcopy /y /e resource-root dist\
copy /y fxmanifest.lua dist
xcopy /y /e Client\bin\Release\net452\publish dist\Client\bin\Release\net452\publish\
xcopy /y /e Server\bin\Release\netstandard2.0\publish dist\Server\bin\Release\netstandard2.0\publish\