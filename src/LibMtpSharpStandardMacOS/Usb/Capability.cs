//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// Copyright © 2006-2010 Travis Robinson. All rights reserved.
// Copyright © 2011-2023 LibUsbDotNet contributors. All rights reserved.
// 
// website: http://github.com/libusbdotnet/libusbdotnet
// 
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the
// Free Software Foundation; either version 2 of the License, or 
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful, but 
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License
// for more details.
// 
// You should have received a copy of the GNU General Public License along
// with this program; if not, write to the Free Software Foundation, Inc.,
// 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA. or 
// visit www.gnu.org.
// 
//

using System;

namespace LibMtpSharpStandardMacOS.Usb
{
    /// <summary>
    /// Capabilities supported by an instance of libusb on the current running platform.
    /// </summary>
    /// <remarks>
    /// Test if the loaded library supports a given capability by calling <see cref="NativeMethods.HasCapability"/>.
    /// </remarks>
    [Flags]
    public enum Capability : uint
    {
        /// <summary>
        /// The libusb_has_capability() API is available. 
        /// </summary>
        HasCapability = 0,

        /// <summary>
        /// Hotplug support is available on this platform. 
        /// </summary>
        HasHotplug = 0x1,

        /// <summary>
        /// The library can access HID devices without requiring user intervention.
        /// </summary>
        /// <remarks>
        /// Note that before being able to actually access an HID device, you may still have to call additional libusb
        /// functions such as DetachKernelDriver.
        /// </remarks>
        HasHidAccess = 0x100,

        /// <summary>
        /// The library supports detaching of the default USB driver, using DetachKernelDriver,
        /// if one is set by the OS kernel.
        /// </summary>
        SupportsDetachKernelDriver = 0x101,
        
    }
}
