# LibMtpSharp

LibMtpSharp is a wrapper around [libmtp library](https://github.com/libmtp/libmtp) written in c with a bit of custom changes
at [this fork](https://github.com/shaosss/libmtp) which made for being able to wrap c code properly
and some ease of use functionality.

## What packages does I need to use?

There are multiple packages related to the wrapper: the wrapper itself and packages with native libraries.
To use the wrapper you need LibMtpSharp package and appropriate package for the OS which your SW targets.
You can see all the packages and differences in the table below:

| Package name                                | Latest Version | Content                                                                                                                | Usage Instructions                                                          |
|---------------------------------------------|----------------|------------------------------------------------------------------------------------------------------------------------|-----------------------------------------------------------------------------|
| LibMtpSharp                                 | 0.1.3          | Contains the wrapper managed code. API coverage is limited and will improve in future versions.                        | Main package with logic. Required to be able to use libmpt                  |
| LibMtpSharpStandardMacOS                    | 0.1.12         | Contains the wrapper managed code with MacOS dependencies for .Net standard                                            | Main package with logic for .Net standard when you target MacOS             |
| LibMtpSharp.Native.Linux                    | 1.1.20-beta    | Linux native Libmtp library with custom improvements built for x64                                                     | Use if you target linux and instruct the user how to install dependencies   |
| LibMtpSharp.Native.Linux.WithDependencies   | 1.1.20-beta    | Linux native dependencies built for x64 and references LibMtpSharp.Native.Linux package.                               | Use if you target linux and don't want user to manage dependencies          |
| LibMtpSharp.Native.MacOS                    | 1.1.20-beta    | MacOS native Libmtp library with custom improvements built for x64 (not sure if will work on M1)                       | Use if you target MacOS and instruct the user how to install dependencies   |
| LibMtpSharp.Native.MacOS.WithDependencies   | 1.1.20-beta    | MacOS native dependencies built for x64 (not sure if will work on M1) and references LibMtpSharp.Native.MacOS package. | Use if you target MacOS and don't want user to manage dependencies          |
| LibMtpSharp.Native.Windows                  | 1.1.20-beta    | Windows native Libmtp library with custom improvements built for x64                                                   | Use if you target Windows and instruct the user how to install dependencies |
| LibMtpSharp.Native.Windows.WithDependencies | 1.1.20-beta    | Windows native dependencies built for x64 and references LibMtpSharp.Native.Windows package.                           | Use if you target Windows and don't want user to manage dependencies        |

The dependencies package include following libraries: libgcrypt, libgpg-error, libiconv, libcharset and libusb.

## How to compile this project?

Requirements: homebrew, correctly configured Xcode command line tools, dotnet 6.0.x, wget

Compilation of libmtp with dependencies (assuming that `LibMtpSharp` is located in `~/LibMtpSharp` path):

```
cd ~
git clone https://github.com/legimi/LibMtpSharp
cd LibMtpSharp/lib/MacOS
chmod +x buildNativeLibs.sh
mkdir input
cd input
./../buildNativeLibs.sh ~/LibMtpSharp/lib ~/LibMtpSharp/lib/MacOS/output
```

Building LibMtpSharp.Native.MacOS.WithDependencies:

```
cd ../../..
dotnet build ./lib/MacOS/LibMtpSharp.Native.MacOS.WithDependencies/LibMtpSharp.Native.MacOS.WithDependencies.csproj
```
For our purpose, we are using only `LibMtpSharpStandardMacOS`.

# Using install_name_tool and otool in macOS

## Overview
This note explains the use of `install_name_tool` for setting the install name of a dynamic library (`.dylib`) and updating its dependencies' paths in macOS. It also covers how to verify these changes with `otool`. Adjusting these paths is crucial for ensuring that the dynamic libraries remain valid and accessible when releasing an application to users.

## Prerequisites
### Xcode Command Line Tools
- `install_name_tool` is part of the Command Line Tools for Xcode.
- You can install the Command Line Tools by running `xcode-select --install` in the Terminal.
- This is necessary for using `install_name_tool` and other development utilities.

## Importance of Changing Library Paths
Dynamic libraries (`dylib` files) in macOS use install names and paths to locate each other. When developing an application, these paths often point to locations on the developer's machine. However, these paths will not be valid on a user's machine. To ensure that the application can correctly locate and load its libraries on any user's system, the paths need to be set relative to the application's executable. This is achieved using the `@executable_path` variable, which makes the application self-contained and portable.

## Steps

### 1. Setting the Install Name of a Library

To set the install name of a library, use the following command:

```bash
install_name_tool -id @executable_path/../MonoBundle/library_name.dylib /path/to/library.dylib
````

- `library_name.dylib` should be replaced with the actual name of your dylib.
- `/path/to/library.dylib` is the path to your dylib file.

### 2. Updating Dependency Paths
For each dependency that needs its path updated, use the following command:

```bash
install_name_tool -change /path/to/original/dependency.dylib @executable_path/../MonoBundle/dependency.dylib /path/to/library.dylib
````

- Replace `/path/to/original/dependency.dylib` with the current path of the dependency.
- `dependency.dylib` is the name of the dependency library.
- `/path/to/library.dylib` is the path to your dylib file that has this dependency.

Repeat this step for each dependency that needs to be updated.

### 3. Verifying Changes with otool
After applying the changes, verify them using otool:
```bash
otool -L /path/to/library.dylib
````

This command lists the dependencies of the dylib and shows their paths. Ensure that all the required paths now correctly point to `@executable_path/../MonoBundle/`.

### 4.Including Dylibs in a macOS Project:
To include a dynamic library in your macOS project, add it in your project configuration file like this:
```xml
  <Content Include="library_name.dylib">
    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
  </Content>
```
This ensures that the dylib file is always copied to the output directory of your project during the build process.

## What has been changed in native libmtp?

The libmtp native library in the packages contains followinf changes:
- Add `LIBMTP_Free(void *)` function to free native resources (.net can't access c `free()` function directly, since it's behaviour can be compiler specific)
- Add bcd device info to `DeviceEntry` struct.
- Make MTPZ data be able to come from shared resources, not only from file in $HOME directory as used in vanila libmtp library.

## How to use wrapper?

The documentation is limited for now, but you can use LibMtp library documentation for reference. The documentation will be improved with time.

To get the list of available devices you should create `RawDeviceList`, which implements the `IEnumerable<RawDevice>`. **!Dont forget to dispose it!**

```c#
using (var list = new RawDeviceList())
{
    foreach(var device in list)
        Console.WriteLine(device);
}
````

To connect to device in interest you should create the `OpenedMtpDevice` with corresponding `RawDevice` struct from the list

```c#
using (var list = new RawDeviceList())
{
    var rawDevice = list.First(); //assuming we have at least one device
    var connectedDevice = new OpenedMtpDevice(ref rawDevice, false);
}
```

`OpenedMtpDevice` contains the methods to communicate with device. **! `OpenedMtpDevice` is a disposable object. When you finished communicating with device - you should Dispose the instance. This is equivalent to closing the connection. !**

You can donate through Github or [Paypal](https://www.paypal.com/donate/?hosted_button_id=FFM78JRJCKNS8)
