# Visual Studio Project Support for emscripten

## Introduction

This extension will provide...

- Project template for emscripten
- Property pages for compilation with emcc

This extension is based on following repositories.

- <https://github.com/crosire/vs-toolsets>
- <https://github.com/gavinpugh/vs-android>

This extension is developed with [Visual Studio Project System](https://github.com/microsoft/VSProjectSystem), which is in preview.

## Requirements

- Visual Studio 2017 or Visual Studio 2019
- emscripten Installation

## Installation

### From Visual Studio Extenstion Manager

- Open Visual Studio Extenstion Manager ( [Tools] > [Extensions and Updates...] )
- Search for Emscripten.ProjectType

### From Visual Studio MarketPlace

|Visual Studio Version|.vsix Download Page|
|:--:|:--:|
|Visual Studio 2013, 2015|(Not .vsix, but <https://github.com/crosire/vs-toolsets> will help)|
|Visual Studio 2017, Visual Studio 2019|<https://marketplace.visualstudio.com/items?itemName=KamenokoSoft.emscriptenproj1>|

### From GitHub Releases (Cutting Edge)

- <https://github.com/nokotan/VSExtForEmscripten/releases/>

## First Step with this tool

* Open project preference (in \[Project\] > \[$(ProjectName)\ Properties ...]) and fill out **General/Emscripten** ("emscripten インストールディレクトリ" in Japanese) with your emscripten installation path.

![General/Emscripten](https://qiita-user-contents.imgix.net/https%3A%2F%2Fqiita-image-store.s3.ap-northeast-1.amazonaws.com%2F0%2F158514%2F74993f9c-8ff4-e500-3521-8f0e7748a403.png?ixlib=rb-1.2.2&auto=format&gif-q=60&q=75&w=1400&fit=max&s=013bc350ca67e07cb85781e34ecd5313)
