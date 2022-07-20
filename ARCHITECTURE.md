# Architecture

## Extension Structure

### Emscripten Build Target

- Core extension
- Provides following functionarity
  - Property page definition (for Visual Studio 2022)
  - Project template
  - WebAssembly debugger

### Emscripten Build Definition for vs2017, vs2019

- Additional package for Visual Studio 2017, 2019
- Provides following functionarity
  - Property page definition

### Emscripten Extension Pack for Visual Studio

- Installs required extension according to your development environment

## Project Files Structure

### Emscripten.Build.CppTasks

- Build Tasks Implementation (EmscriptenCompile, EmscriptenLink, EmscriptenLib) which are used in Toolset.targets

### Emscripten.ProjectTemplate

- Project template with single Main.cpp file

### Emscripten.Debugger

- Core files of WebAssembly Debugger (under heavy construction)
- Bundle result of [nokotan/cdb-gdb-bridge](https://github.com/nokotan/cdp-gdb-bridge)

### Emscripten.Debugger.Definition

- Debugger definition of WebAssembly Debugger

### Emscripten.Build.Definition

- .vsix package manifest
- Definitions of Toolset and project pages
