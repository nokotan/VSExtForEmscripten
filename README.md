# Visual Studio Project Support for emscripten

## Introduction

This extension will provide...

- Project template for emscripten
- Property pages for compilation with emcc
- Debugger Support (experimental)

This extension is based on following repositories.

- <https://github.com/crosire/vs-toolsets>
- <https://github.com/gavinpugh/vs-android>

This extension is developed with [Visual Studio Project System](https://github.com/microsoft/VSProjectSystem), which is in preview.

## Requirements

- Visual Studio 2017, Visual Studio 2019, Visual Studio 2022
- emscripten Installation

## Installation

### From Visual Studio Extenstion Manager

- Open Visual Studio Extenstion Manager ( [Tools] > [Extensions and Updates...] )
- Search for Emscripten Build Support

### From Visual Studio MarketPlace

|Visual Studio Version|.vsix Download Page|
|:--:|:--:|
|Visual Studio 2013, 2015|(Not .vsix, but <https://github.com/crosire/vs-toolsets> will help)|
|Visual Studio 2017, 2019, 2022|<https://marketplace.visualstudio.com/items?itemName=KamenokoSoft.emscripten-extensions>|

### From GitHub Releases (Cutting Edge)

- <https://github.com/nokotan/VSExtForEmscripten/releases/>

## First Step with this tool

* Open project preference (in \[Project\] > \[$(ProjectName)\ Properties ...]) and fill out **General/Emscripten** ("emscripten インストールディレクトリ" in Japanese) with your emscripten installation path.

![General/Emscripten](https://camo.githubusercontent.com/7f04788bbdf57ddb56c3c609bcb7947a704bb045457d3337e8211533a21bc35d/68747470733a2f2f71696974612d757365722d636f6e74656e74732e696d6769782e6e65742f687474707325334125324625324671696974612d696d6167652d73746f72652e73332e61702d6e6f727468656173742d312e616d617a6f6e6177732e636f6d2532463025324631353835313425324637343939336639632d386666342d653530302d333532312d3866306537373438613430332e706e673f69786c69623d72622d312e322e32266175746f3d666f726d6174266769662d713d363026713d373526773d31343030266669743d6d617826733d3031336263333530636136376530376362383537383165333465636435333133)
