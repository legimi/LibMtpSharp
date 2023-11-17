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

using Microsoft.Win32.SafeHandles;
using System;
using System.Security.Permissions;
using LibMtpSharpStandardMacOS.NativeAPI;

namespace LibMtpSharpStandardMacOS.Usb
{
    /// <summary>
    /// Represents a wrapper class for <c>libusb_device</c> handles.
    /// </summary>
    public partial class Device : SafeHandleZeroOrMinusOneIsInvalid
    {
        private bool _fromHotplug;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="Device"/> class.
        /// </summary>
        protected Device() :
                base(true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Device"/> class, specifying whether the handle is to be reliably released.
        /// </summary>
        /// <param name="ownsHandle">
        /// <see langword="true"/> to reliably release the handle during the finalization phase; <see langword="false"/> to prevent reliable release (not recommended).
        /// </param>
        /// <param name="fromHotplug">Device was created from hotplug event.</param>
        protected Device(bool ownsHandle, bool fromHotplug) :
                base(ownsHandle)
        {
            _fromHotplug = fromHotplug;
        }

        /// <summary>
        /// Gets a value which represents a pointer or handle that has been initialized to zero.
        /// </summary>
        public static Device Zero
        {
            get
            {
                return Device.DangerousCreate(IntPtr.Zero, false);
            }
        }

        /// <summary>
        /// Creates a new <see cref="Device"/> from a <see cref="IntPtr"/>.
        /// </summary>
        /// <param name="unsafeHandle">
        /// The underlying <see cref="IntPtr"/>
        /// </param>
        /// <param name="ownsHandle">
        /// <see langword="true"/> to reliably release the handle during the finalization phase; <see langword="false"/> to prevent reliable release (not recommended).
        /// </param>
        /// <param name="fromHotplug">Device was created from hotplug event.</param>
        /// <returns>
        /// </returns>
        public static Device DangerousCreate(IntPtr unsafeHandle, bool ownsHandle, bool fromHotplug)
        {
            Device safeHandle = new Device(ownsHandle, fromHotplug);
            safeHandle.SetHandle(unsafeHandle);
            return safeHandle;
        }

        /// <summary>
        /// Creates a new <see cref="Device"/> from a <see cref="IntPtr"/>.
        /// </summary>
        /// <param name="unsafeHandle">
        /// The underlying <see cref="IntPtr"/>
        /// </param>
        /// <param name="fromHotplug">Device was created from hotplug event.</param>
        /// <returns>
        /// </returns>
        public static Device DangerousCreate(IntPtr unsafeHandle, bool fromHotplug)
        {
            return Device.DangerousCreate(unsafeHandle, true, fromHotplug);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("{0} ({1})", this.handle, "Device");
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj != null && obj.GetType() == typeof(Device))
            {
                return ((Device)obj).handle.Equals(this.handle);
            }
            else
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return this.handle.GetHashCode();
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="Device"/> are equal.
        /// </summary>
        /// <param name="value1">
        /// The first pointer or handle to compare.
        /// </param>
        /// <param name="value2">
        /// The second pointer or handle to compare.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="value1"/> equals <paramref name="value2"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator == (Device value1, Device value2) 
        {
            if (object.Equals(value1, null) && object.Equals(value2, null))
            {
                return true;
            }

            if (object.Equals(value1, null) || object.Equals(value2, null))
            {
                return false;
            }

            return value1.handle == value2.handle;
        }

        /// <summary>
        /// Determines whether two specified instances of <see cref="Device"/> are not equal.
        /// </summary>
        /// <param name="value1">
        /// The first pointer or handle to compare.
        /// </param>
        /// <param name="value2">
        /// The second pointer or handle to compare.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="value1"/> does not equal <paramref name="value2"/>; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool operator != (Device value1, Device value2) 
        {
            if (object.Equals(value1, null) && object.Equals(value2, null))
            {
                return false;
            }

            if (object.Equals(value1, null) || object.Equals(value2, null))
            {
                return true;
            }

            return value1.handle != value2.handle;
        }

        protected override bool ReleaseHandle()
        {
            if (!_fromHotplug)
                UsbLibrary.UnrefDevice(this.handle);
            return true;
        }
    }
}
