# @vscode/dwarf-debugging

This repository publishes an npm module which is used by [js-debug](https://github.com/microsoft/vscode-js-debug) to better debug WebAssembly code when [DWARF](https://dwarfstd.org/) debugging information is present. You can use this in two ways:

- For VS Code users, you will be prompted to install the appropriate extension that contains this module when you first step into a WebAssembly file.
- For users of the js-debug standalone DAP server, you can install the `@vscode/dwarf-debugging` alongside js-debug or somewhere in your [NODE_PATH](https://nodejs.org/api/modules.html#loading-from-the-global-folders).

This project works by compiling the Chrome [C/C++ Debugging Extension](https://github.com/ChromeDevTools/devtools-frontend/tree/main/extensions/cxx_debugging) in a way that is usable by Node.js programs. Fortunately, the extension is architected such that most work is done inside a WebWorker, and making this instead run in a Node worker_thread is not terribly difficult. Appropriate TypeScript types are exposed, and this module is then shimmed into js-debug which 'pretends' to be Devtools.

## Contributing

### Building

1. Clone this repo, and have Docker or Podman installed.
2. Run `npm run build`. This will take a while (~1hr depending on how fast your computer is.) Note that for developing you can run individual build steps in their appropriate scripts.
3. You then have a built module. Run `npm run test` to run some basic tests.

### General

This project welcomes contributions and suggestions. Most contributions require you to agree to a
Contributor License Agreement (CLA) declaring that you have the right to, and actually do, grant us
the rights to use your contribution. For details, visit https://cla.opensource.microsoft.com.

When you submit a pull request, a CLA bot will automatically determine whether you need to provide
a CLA and decorate the PR appropriately (e.g., status check, comment). Simply follow the instructions
provided by the bot. You will only need to do this once across all repos using our CLA.

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).
For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Trademarks

This project may contain trademarks or logos for projects, products, or services. Authorized use of Microsoft
trademarks or logos is subject to and must follow
[Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general).
Use of Microsoft trademarks or logos in modified versions of this project must not cause confusion or imply Microsoft sponsorship.
Any use of third-party trademarks or logos are subject to those third-party's policies.
