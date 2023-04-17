# escape=`

FROM nokotan/emscripten-windows:latest

# Restore the default Windows shell for correct batch processing.
SHELL ["cmd", "/S", "/C"]

COPY Emscripten.Build.Definition.vsix .

RUN `
    ""%ProgramFiles%\Microsoft Visual Studio\2022\BuildTools\Common7\Tools\VsDevCmd.bat" `
    && VSIXInstaller /quiet /admin Emscripten.Build.Definition.vsix `
    && PowerShell -NoLogo -ExecutionPolicy ByPass Wait-Process -Name "VSIXInstaller" `
    && del /q Emscripten.Build.Definition.vsix"
