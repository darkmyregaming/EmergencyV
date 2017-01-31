# EmergencyV

A Grand Theft Auto V modification that adds firefighting and emergency medical service simulations into the game.

**NOTE: This plugin is a work in progress (WIP) and not guaranteed to be stable or fully-featured.**

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See installation notes on how to install and play the plugin.

### Prerequisites

* [RAGE Plugin Hook](https://ragepluginhook.net/Downloads.aspx) 0.47 or later
* [AdvancedUI](https://github.com/alexguirre/AdvancedUI) 0.0.1.0 or later
* [Visual Studio](https://www.visualstudio.com/downloads/) 2015 (14.0) or later
* [XSerializer](https://github.com/QuickenLoans/XSerializer) 0.3.8 or later

### Development or Building

Instructions for setting a development or building environment.

1. Clone the project repository:
`git clone https://github.com/alexguirre/EmergencyV.git`
2. Create a `Dependencies` folder in the main project folder.
3. Copy `RagePluginHookSDK.dll` and `AdvancedUI.dll` to the newly created folder.
4. Follow the appropriate steps for Visual Studio or Command Prompt below.

##### Visual Studio
1. Open `EmergencyV.csproj` in Visual Studio.
2. Select the build configuration target `Debug` or `Release`
3. Click `Build > Build Solution` or press `F6`

##### Command Prompt
1. Open a developer command prompt.
2. Navigate to same folder as `EmergencyV.csproj`
3. Type `msbuild` (Debug) or `msbuild /property:Configuration=Release` (Release)

### Installation

Follow the steps listed under [Development or Building](#development-or-building).

The following assumes you have both RAGE Plugin Hook and AdvancedUI installed successfully and have sufficient knowledge of how to use them. If not, please consult their respective tutorials.

1. Copy the files found under `bin\Debug` or `bin\Release` to `[GTA Folder]\Plugins`
2. Run RAGE Plugin Hook normally and load the `EmergencyV.dll` plugin.

## Authors

* [alexguirre](https://github.com/alexguirre) - *Project Leader*

See also the list of [contributors](https://github.com/alexguirre/EmergencyV/graphs/contributors) who participated in this project.

## License

This project is licensed under the MIT License - see the [LICENSE.txt](LICENSE.txt) file for details
