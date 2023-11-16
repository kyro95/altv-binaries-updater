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

# Adding AltV.Binaries.Updater.exe to Windows Environment Variables (chatgpt generated)

## Introduction
Environment variables provide a way to store configuration settings and system paths for easy access by applications. Adding AltV.Binaries.Updater.exe to the Windows environment variables allows you to execute it from any command prompt window without specifying its full path.

## Steps

### 1. Locate AltV.Binaries.Updater.exe
   - Make sure you know the full path to `AltV.Binaries.Updater.exe`. If you don't have it yet, download and install AltV and find the executable in the installation directory.

### 2. Open System Properties
   - Right-click on the Windows Start menu and select "System."
   - Alternatively, press `Win + Pause/Break` to open the System window.

### 3. Access Advanced System Settings
   - In the System window, click on "Advanced system settings" on the left sidebar.

### 4. Open Environment Variables
   - In the System Properties window, click on the "Environment Variables..." button.

### 5. Edit System Environment Variables
   - In the Environment Variables window, under the "System variables" section, find and select the `Path` variable.
   - Click on the "Edit..." button.

### 6. Add AltV.Binaries.Updater.exe
   - Click on the "New" button.
   - Paste the full path to `AltV.Binaries.Updater.exe` in the provided text field.
   - Click "OK" to close each window.

### 7. Verify the Changes
   - Open a new Command Prompt window or restart the existing one.
   - Type `AltV.Binaries.Updater.exe` and press Enter.
   - If everything is set up correctly, the AltV updater should execute without specifying the full path.

## Conclusion
You've successfully added AltV.Binaries.Updater.exe to the Windows environment variables. This makes it easier to run the updater from any command prompt window on your system.
