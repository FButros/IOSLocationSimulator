using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using iMobileDevice;
using iMobileDevice.iDevice;
using iMobileDevice.Lockdown;
using iMobileDevice.Plist;

class Program
{
        private static readonly string DeveloperDiskImagesPath = "DeveloperDiskImages";

    static void Main(string[] args)
    {
        NativeLibraries.Load();
        bool exitProgram = false;

        while (!exitProgram)
        {
            DisplayMainMenu();
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    DetectAndHandleDevice();
                    break;
                case "2":
                    DisableSimulatedLocation();
                    break;
                case "3":
                    exitProgram = true;
                    Console.WriteLine("Exiting program. Goodbye!");
                    break;   
                case "help":
                    DisplayHelp();
                    break;
                default:
                    Console.WriteLine("Invalid choice. Please enter 1, 2 or 'help'.");
                    break;
            }
        }
    }


static void DisableSimulatedLocation()
{
    var udids = ListConnectedDevices();
    if (udids.Count == 0)
    {
        Console.WriteLine("No devices connected. Please connect a device.");
        PromptRetry();
    }
    else
    {
        string selectedUdid = udids.Count == 1 ? udids[0] : GetUserSelectedUdid(udids);
        ResetLocation(selectedUdid);
    }
}

static void ResetLocation(string udid)
{
    ProcessStartInfo startInfo = new ProcessStartInfo()
    {
        FileName = "idevicesetlocation",
        Arguments = $"--udid {udid} reset",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
    };

    using (Process process = Process.Start(startInfo))
    {
        using (StreamReader reader = process.StandardOutput)
        {
            string result = reader.ReadToEnd();
            Console.WriteLine(result);
        }

        using (StreamReader reader = process.StandardError)
        {
            string error = reader.ReadToEnd();
            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine("Error: " + error);
            }
        }
    }
    Console.WriteLine("Simulated location has been disabled.");
    Console.WriteLine("Press any key to return to the main menu.");
        Console.ReadKey();
}

    static void DisplayMainMenu()
    {
        Console.Clear();
        Console.WriteLine("Welcome to IOSLocationSimulator");
        Console.WriteLine("1. Connect a device");
        Console.WriteLine("2. Reset Location");
        Console.WriteLine("3. Exit");
        Console.WriteLine("Type 'help' for instructions.");
        Console.Write("Enter your choice: ");
    }

    static void DisplayHelp()
    {
        Console.Clear();
        Console.WriteLine("Help Guide:");
        Console.WriteLine("1 - Connect your iOS device to set a new location.");
        Console.WriteLine("2 - Exit the program.");
        Console.WriteLine("Follow the on-screen instructions for each option.");
        Console.WriteLine("Press any key to return to the main menu.");
        Console.ReadKey();
    }

    static void DetectAndHandleDevice()
    {
        var udids = ListConnectedDevices();
        if (udids.Count == 0)
        {
            Console.WriteLine("No devices connected. Please connect a device.");
            PromptRetry();
        }
        else
        {
            try
            {
                string selectedUdid = udids.Count == 1 ? udids[0] : GetUserSelectedUdid(udids);
                string iosVersion = GetiOSVersion(selectedUdid);
                if (!string.IsNullOrEmpty(iosVersion) && iosVersion.StartsWith("16"))
                {
                    MountDeveloperDiskImage(selectedUdid, iosVersion);
                }

                SetLocationWithInput(selectedUdid);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                PromptRetry();
            }
        }
    }

    static void PromptRetry()
    {
        Console.WriteLine("Press any key to try again.");
        Console.ReadKey();
        Console.Clear();
    }

    static void SetLocationWithInput(string udid)
    {
        Console.WriteLine("Enter the Latitude (e.g., 37.7749):");
        if (!double.TryParse(Console.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out double latitude))
        {
            Console.WriteLine("Invalid latitude format. Please enter a valid number.");
            return;
        }

        Console.WriteLine("Enter the Longitude (e.g., -122.4194):");
        if (!double.TryParse(Console.ReadLine(), NumberStyles.Any, CultureInfo.InvariantCulture, out double longitude))
        {
            Console.WriteLine("Invalid longitude format. Please enter a valid number.");
            return;
        }

        SetLocation(udid, latitude, longitude);
    }
    static List<string> ListConnectedDevices()
    {
        List<string> deviceUdids = new List<string>();
        ReadOnlyCollection<string> udids;
        int count = 0;

        var idevice = LibiMobileDevice.Instance.iDevice;
        var lockdown = LibiMobileDevice.Instance.Lockdown;

        var ret = idevice.idevice_get_device_list(out udids, ref count);

        if (ret == iDeviceError.NoDevice)
        {
            return deviceUdids;
        }

        ret.ThrowOnError();

        foreach (var udid in udids)
        {
            deviceUdids.Add(udid);
        }

        return deviceUdids;
    }

    static string GetUserSelectedUdid(List<string> udids)
    {
        Console.WriteLine("Multiple devices detected. Please select a device:");
        for (int i = 0; i < udids.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {udids[i]}");
        }

        Console.Write("Enter the number of the desired device: ");
        int choice = Convert.ToInt32(Console.ReadLine());

        return udids[choice - 1];
    }

    static void SetLocation(string udid, double latitude, double longitude)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = "idevicesetlocation",
            Arguments = $"--udid {udid} -- {latitude.ToString(CultureInfo.InvariantCulture)} {longitude.ToString(CultureInfo.InvariantCulture)}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using (Process process = Process.Start(startInfo))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                string result = reader.ReadToEnd();
                Console.WriteLine(result);
            }

            using (StreamReader reader = process.StandardError)
            {
                string error = reader.ReadToEnd();
                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine("Error: " + error);
                }
            }
        }
    }

static void MountDeveloperDiskImage(string udid, string iosVersion)
{
    // Normalize the iOS version (e.g., "16.0.1" to "16.0")
    string normalizedVersion = NormalizeVersion(iosVersion);

    // Construct the path to the disk image based on the normalized iOS version
    string diskImageDirectory = Path.Combine(DeveloperDiskImagesPath, normalizedVersion);
    string diskImagePath = Path.Combine(diskImageDirectory, "DeveloperDiskImage.dmg");
    string diskImageSignaturePath = diskImagePath + ".signature";

    if (!File.Exists(diskImagePath) || !File.Exists(diskImageSignaturePath))
    {
        Console.WriteLine($"Developer disk image for iOS {normalizedVersion} not found.");
        return;
    }

    ProcessStartInfo startInfo = new ProcessStartInfo()
    {
        FileName = "ideviceimagemounter",
        Arguments = $"{diskImagePath} {diskImageSignaturePath}",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
    };

    using (Process process = Process.Start(startInfo))
    {
        using (StreamReader reader = process.StandardOutput)
        {
            string result = reader.ReadToEnd();
            Console.WriteLine(result);
        }

        using (StreamReader reader = process.StandardError)
        {
            string error = reader.ReadToEnd();
            if (!string.IsNullOrEmpty(error))
            {
                Console.WriteLine("Error: " + error);
            }
        }
    }
}

static string NormalizeVersion(string version)
{
    // Example: Convert "16.0.1" to "16.0"
    var versionParts = version.Split('.');
    if (versionParts.Length >= 2)
    {
        return versionParts[0] + "." + versionParts[1];
    }

    return version;
}


    static string GetiOSVersion(string udid)
    {
        iDeviceError ideviceError = iDeviceError.NoDevice;
        LockdownError lockdownError = LockdownError.InvalidArg;

        iDeviceHandle deviceHandle;
        LockdownClientHandle lockdownHandle;
        string productVersion = string.Empty;

        ideviceError = LibiMobileDevice.Instance.iDevice.idevice_new(out deviceHandle, udid);
        if (ideviceError != iDeviceError.Success)
        {
            Console.WriteLine($"Failed to connect to device with UDID: {udid}");
            return productVersion;
        }

        lockdownError = LibiMobileDevice.Instance.Lockdown.lockdownd_client_new_with_handshake(deviceHandle, out lockdownHandle, "GetiOSVersion");
        if (lockdownError != LockdownError.Success)
        {
            Console.WriteLine($"Failed to start lockdown session with device: {udid}");
            return productVersion;
        }

        lockdownError = LibiMobileDevice.Instance.Lockdown.lockdownd_get_value(lockdownHandle, null, "ProductVersion", out PlistHandle versionPlist);
        if (lockdownError == LockdownError.Success)
        {
            versionPlist.Api.Plist.plist_get_string_val(versionPlist, out productVersion);
        }
        else
        {
            Console.WriteLine($"Failed to retrieve iOS version from device: {udid}");
        }

        versionPlist.Dispose();
        lockdownHandle.Dispose();
        deviceHandle.Dispose();

        return productVersion;
    }
}
