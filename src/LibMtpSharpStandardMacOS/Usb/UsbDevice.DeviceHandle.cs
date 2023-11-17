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
using System.Threading;
using LibMtpSharpStandardMacOS.Exceptions;
using LibMtpSharpStandardMacOS.Extensions;
using LibMtpSharpStandardMacOS.NativeAPI;

namespace LibMtpSharpStandardMacOS.Usb
{
    // Implementation of functionality which wraps around a DeviceHandle.
    public partial class UsbDevice
    {
        /// <summary>
        /// The underlying device handle. The handle is populated when you open the device
        /// using <see cref="Open"/>, and cleared when you close the device using <see cref="Close"/>.
        /// </summary>
        private DeviceHandle deviceHandle;

        /// <inheritdoc/>
        public DeviceHandle DeviceHandle
        {
            get { return deviceHandle; }
        }

        /// <summary>
        /// Gets a value indicating whether the device has been opened. You can perform I/O on a
        /// device when it is open.
        /// </summary>
        public bool IsOpen
        {
            get { return deviceHandle != null; }
        }

        /// <summary>
        /// Gets the <c>bConfigurationValue</c> of the currently active configuration.
        /// </summary>
        /// <remarks>
        /// <para>
        /// You could formulate your own control request to obtain this information, but this function
        /// has the advantage that it may be able to retrieve the information from operating system caches
        /// (no I/O involved).
        /// </para>
        /// <para>
        /// If the OS does not cache this information, then this function will block while a control
        /// transfer is submitted to retrieve the information.
        /// </para>
        /// <para>
        /// This property will return a value of 0 in the config output parameter if the device is
        /// in unconfigured state.
        /// </para>
        /// </remarks>
        public int Configuration
        {
            get
            {
                EnsureNotDisposed();
                EnsureOpen();

                int config = 0;
                UsbLibrary.GetConfiguration(deviceHandle, ref config).ThrowOnError();
                return config;
            }
        }

        /// <inheritdoc/>
        public void SetConfiguration(int config)
        {
            EnsureNotDisposed();
            EnsureOpen();

            UsbLibrary.SetConfiguration(deviceHandle, config).ThrowOnError();
        }

        /// <summary>
        /// Opens a device, allowing you to perform I/O on this device.
        /// </summary>
        public void Open()
        {
            OpenNative().ThrowOnError();
        }

        /// <inheritdoc/>
        public bool TryOpen()
        {
            return OpenNative() == UsbError.Success;
        }

        /// <summary>
        /// Closes the device.
        /// </summary>
        public void Close()
        {
            EnsureNotDisposed();

            if (!IsOpen)
                return;

            bool shouldStopHandlingEvents = originatingContext.OpenDevices.Count == 1 && !originatingContext.IsUsingHotplug;

            if (shouldStopHandlingEvents)
                Interlocked.Exchange(ref originatingContext.stopHandlingEvents, 1);

            deviceHandle.Dispose();
            deviceHandle = null;

            if (shouldStopHandlingEvents)
                originatingContext.StopHandlingEvents();

            if (!originatingContext.IsDisposing)
                originatingContext.OpenDevices.Remove(this);
        }

        /// <summary>
        /// Throws a <see cref="UsbException"/> if the device is not open.
        /// </summary>
        protected void EnsureOpen()
        {
            if (!IsOpen)
            {
                throw new UsbException("The device has not been opened. You need to call Open() first.");
            }
        }

        private UsbError OpenNative()
        {
            EnsureNotDisposed();

            if (IsOpen)
            {
                return UsbError.Success;
            }

            IntPtr deviceHandle = IntPtr.Zero;
            var ret = UsbLibrary.Open(device, ref deviceHandle);

            if (ret == UsbError.Success)
            {
                this.deviceHandle = DeviceHandle.DangerousCreate(deviceHandle);
                if (originatingContext.OpenDevices.Count == 0 && !originatingContext.IsUsingHotplug)
                    originatingContext.StartHandlingEvents();
                if (!originatingContext.IsDisposing)
                    originatingContext.OpenDevices.Add(this);
            }

            return ret;
        }
    }
}