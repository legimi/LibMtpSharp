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
using System.Collections.Generic;
using LibMtpSharpStandardMacOS.NativeAPI;

namespace LibMtpSharpStandardMacOS.Usb
{
    /// <summary>
    /// Represents a device which is managed by libusb. Use <see cref="List{T}"/>
    /// to get a list of devices which are available for use.
    /// </summary>
    public partial class UsbDevice : IUsbDevice, IDisposable, ICloneable
    {
        private bool disposed;
        private readonly Device device;

        /// <summary>
        /// The <see cref="UsbContext"/> the device originated from.
        /// </summary>
        private readonly UsbContext originatingContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="UsbDevice"/> class.
        /// </summary>
        /// <param name="device">
        /// A device handle for this device. In most cases, you will want to use the
        /// <see cref="List{T}"/> methods to list all devices.
        /// </param>
        /// <param name="originatingContext">The <see cref="List{T}"/> the device originated from.</param>
        public UsbDevice(Device device, UsbContext originatingContext)
        {
            if (device == null)
            {
                throw new ArgumentNullException(nameof(device));
            }

            if (device == Device.Zero || device.IsClosed || device.IsInvalid)
            {
                throw new ArgumentOutOfRangeException(nameof(device));
            }

            this.device = device;
            this.originatingContext = originatingContext;
        }

        /// <summary>
        /// Creates a clone of this device.
        /// </summary>
        /// <returns>
        /// A new <see cref="UsbDevice"/> which represents a clone of this device.
        /// </returns>
        public IUsbDevice Clone()
        {
            return new UsbDevice(UsbLibrary.RefDevice(device), originatingContext);
        }

        /// <inheritdoc/>
        object ICloneable.Clone()
        {
            return Clone();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!disposed)
            {
                // Close the libusb_device_handle if required.
                Close();

                // Close the libusb_device handle.
                device.Dispose();

                disposed = true;
            }
        }

        public override int GetHashCode() => device.GetHashCode();

        public override bool Equals(object obj)
        {
            if (obj is not UsbDevice usbDevice)
                return false;

            return GetHashCode() == usbDevice.GetHashCode();
        }

        /// <summary>
        /// Throws a <see cref="ObjectDisposedException"/> if this device or the <see cref="originatingContext"/> of the device has been disposed of.
        /// </summary>
        protected void EnsureNotDisposed()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(UsbDevice));
            if (originatingContext.IsDisposed)
                throw new ObjectDisposedException(nameof(UsbContext),
                    $"Cannot operate on {nameof(UsbDevice)} whose {nameof(originatingContext)} has been disposed.");
        }
    }
}