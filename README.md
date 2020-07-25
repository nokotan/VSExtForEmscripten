# Visual Studio Project Support for emscripten

## Introduction

This extension will provide...

- Project template for emscripten
- Property pages for compilation with emcc

This extension is based on following repositories.

- <https://github.com/crosire/vs-toolsets>
- <https://github.com/gavinpugh/vs-android>

This extension is developed with [Visual Studio Project System](https://github.com/microsoft/VSProjectSystem), which is in preview.

## Installation

### From Visual Studio Extenstion Manager

- Open Visual Studio Extenstion Manager ( [Tools] > [Extensions and Updates...] )
- Search for Emscripten.ProjectType

### From Visual Studio MarketPlace

|Visual Studio Version|.vsix Download Page|
|:--:|:--:|
|Visual Studio 2013, 2015|(Not .vsix, but <https://github.com/crosire/vs-toolsets> will help)|
|Visual Studio 2017, Visual Studio 2019|<https://marketplace.visualstudio.com/items?itemName=KamenokoSoft.emscriptenproj1>|

## From GitHub Releases (Cutting Edge)

- <https://github.com/nokotan/VSExtForEmscripten/releases/>

## Requirements

- Visual Studio 2017 or Visual Studio 2019
- emscripten Installation

## Projects in this repository

### Emscripten.Build.CppTasks

- Implementation for Build Tasks (EmscriptenCompile, EmscriptenLink, EmscriptenLib) which are used in Toolset.targets

### Emscripten.ProjectTemplate

- Implementation for project template

### Emscripten.ProjectType

- TestDebugger (under construction)

### Emscripten.ProjectPackage

- .vsix package manifest
