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
// Modifications made by Legimi in 2023.
// Based on https://github.com/LibUsbDotNet/LibUsbDotNet

using System;

namespace LibMtpSharpStandardMacOS.Usb
{
    /// <summary>
    /// The <see cref="IUsbDevice"/> interface contains members needed to configure a USB device for use.
    /// </summary>
    /// <example>
    /// This example uses the <see cref="IUsbDevice"/> interface to select the desired configuration and interface
    /// for usb devices that require it.
    /// <code source="../../Examples/Read.Write/ReadWrite.cs" lang="cs"/>
    /// </example>
    public interface IUsbDevice : IDisposable
    {
        /// <summary>
        /// Gets the underlying device handle. The handle is populated when you open the device
        /// using <see cref="Open"/>, and cleared when you close the device using <see cref="Close"/>.
        /// </summary>
        DeviceHandle DeviceHandle { get; }

        /// <summary>
        /// Gets a value indicating whether the device handle is valid.
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// Gets the USB devices active configuration value.
        /// </summary>
        /// <returns>
        /// The active configuration value. A zero value means the device is not configured and a non-zero value indicates the device is configured.
        /// </returns>
        int Configuration { get; }

        /// <summary>
        /// Closes and frees device resources.
        /// </summary>
        void Close();

        /// <summary>
        /// Opens/re-opens this USB device instance for communication.
        /// </summary>
        void Open();

        /// <summary>
        /// Attempts to open this device.
        /// </summary>
        /// <returns>
        /// <see langword="true" /> if the device could be opened successfully;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        bool TryOpen();

        /// <summary>
        /// Sets the USB devices active configuration value.
        /// </summary>
        /// <param name="config">The active configuration value. A zero value means the device is not configured and a non-zero value indicates the device is configured.</param>
        /// <remarks>
        /// A USB device can have several different configurations, but only one active configuration.
        /// </remarks>
        void SetConfiguration(int config);

        /// <summary>
        /// Creates a clone of this device.
        /// </summary>
        /// <returns>
        /// A new <see cref="UsbDevice"/> which represents a clone of this device.
        /// </returns>
        IUsbDevice Clone();
    }
}