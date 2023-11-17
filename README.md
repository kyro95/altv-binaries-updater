![Screenshot](https://i.imgur.com/0JQioOM.png)
# altv-binaries-updater

This is a simple updater for alt:V binaries. It will download the latest binaries from the alt:V repository and extract them to the specified directory.

## Usage

```bash
altv-binaries-updater.exe
```

and follow the prompts. (cli version coming soon)

## Configuration

The updater can be configured using the `config.json` file. The following options are available:

**BaseUri** - The base URI to download the binaries from.

**DefaultPath** - The default path to extract the binaries to.

## Installation

You have two options for installing the updater:

### 1. Auto install
Run the `install.ps1` script download. This will install the updater to the default path (`C:\Program Files\altv-binaries-updater`) and it's gonna add a alias to your PowerShell profile so you can run the updater from anywhere by executing `**altv-update**'.

### 2. Portable
Download the latest release from the [releases](https://github.com/kyro95/altv-binaries-updater/releases) page and extract it to a folder of your choice. You can then run the updater by executing `altv-binaries-updater.exe`.
