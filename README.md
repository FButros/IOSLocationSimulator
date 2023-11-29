# IOSLocationSimulator

IOSLocationSimulator is a command-line tool designed to simulate GPS locations on iOS devices. It's particularly useful for developers and testers who require setting specific geographic locations for app development and testing purposes. This tool is developed using C# and interfaces with iOS devices through `libimobiledevice`.

## Features

- **Set GPS Location**: Simulate GPS locations on an iOS device using latitude and longitude coordinates.
- **Disable Simulated Location**: Revert the device to its actual GPS location.
- **Support for iOS 16 and Above**: Optimized for the latest iOS versions.
- **User-Friendly CLI**: Easy-to-navigate command-line interface.

## Prerequisites

Before using IOSLocationSimulator, make sure to install:

- .NET Runtime (version 8.0 or later recommended).
- iTunes (for Windows users) or `libimobiledevice` (for macOS/Linux users).

## Installation

1. Download the latest release from the GitHub repository.
2. Execute the `IOSLocationSimulator_Setup.exe` file and follow the installation wizard.

Note: the exe can be found in \bin\Release\net8.0\win-x(64 or 86 based on your system)\

## Usage

To use IOSLocationSimulator, follow these steps:

### Set Location

1. Run `IOSLocationSimulator` in the terminal or command prompt.
2. Select the option to connect a device.
3. Choose the device from the displayed list.
4. Enter the latitude and longitude for the desired location.

### Disable Simulated Location

1. Choose the option to disable the simulated location from the main menu.
2. Select the connected device from the list.

## Contributing

Contributions to IOSLocationSimulator are welcome. Please adhere to the following steps:

- Fork the repository.
- Create a new branch for your feature or fix.
- Commit your changes with a clear commit message.
- Submit a pull request detailing the changes made.

## License

This project is licensed under the [MIT License](LICENSE.md).

## Disclaimer

IOSLocationSimulator is provided "as is" without warranty of any kind. Developers should use it responsibly and at their own risk.

## Support and Contact

If you encounter any issues or have questions, feel free to open an issue in the GitHub repository.
