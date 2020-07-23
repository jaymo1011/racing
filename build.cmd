@echo off
pushd Client
dotnet publish -c Release
popd

pushd Server
dotnet publish -c Release
popd

rmdir /s /q dist
mkdir dist
mkdir dist\racing

xcopy /y /e resource-root dist\
copy /y fxmanifest.lua dist\racing
xcopy /y /e Client\bin\Release\net452\publish dist\racing\Client\bin\Release\net452\publish\
xcopy /y /e Server\bin\Release\netstandard2.0\publish dist\racing\Server\bin\Release\netstandard2.0\publish\

TIMEOUT /T 5
