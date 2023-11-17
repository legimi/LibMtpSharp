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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using System.Threading;
using LibMtpSharpStandardMacOS.Extensions;
using LibMtpSharpStandardMacOS.Structs;
using LibMtpSharpStandardMacOS.Usb.Info;
using LibMtpSharpStandardMacOS.NativeAPI;

namespace LibMtpSharpStandardMacOS.Usb
{
    /// <summary>
    /// An instance of the libusb API. You can use multiple <see cref="UsbContext"/> which are independent
    /// from each other.
    /// </summary>
    public class UsbContext : IUsbContext
    {
        /// <summary>
        /// The native context.
        /// </summary>
        private readonly Context context;

        /// <summary>
        /// Thread for event handling.
        /// </summary>
        private Thread eventHandlingThread;

        /// <summary>
        /// Tracks whether this context has been disposed of, or not.
        /// </summary>
        public bool IsDisposed { get; private set; }

        public bool IsUsingHotplug { get; private set; }

        public HotplugOptions HotplugOptions { get; } = new();

        /// <summary>
        /// Tracking list of all devices that are open on this context.
        /// </summary>
        internal List<UsbDevice> OpenDevices { get; }

        /// <summary>
        /// Tracks when the context is currently being disposed.
        /// <remarks>
        /// Used for preventing the devices in <see cref="OpenDevices"/> from modifying the list when they are closed in <see cref="Dispose()"/>.
        /// </remarks>
        /// </summary>
        internal bool IsDisposing { get; private set; }

        /// <summary>
        /// Allows the event handling thread to return when set to 1.
        /// </summary>
        internal int stopHandlingEvents;

        private readonly IntPtr hotplugDelegatePtr;
        private readonly HotplugCallbackFn hotplugCallback;

        public delegate void DeviceEventHandler(object sender, DeviceEventArgs e);

        public event EventHandler<DeviceEventArgs> DeviceEvent;

        private readonly ISubject<DeviceState> _deviceStateChangedStream = new Subject<DeviceState>();

        public IObservable<DeviceState> DeviceStateChangedStream => _deviceStateChangedStream.AsObservable();

        internal ConcurrentDictionary<UsbDevice, CachedDeviceInfo> DeviceInfoDictionary { get; } = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="UsbContext"/> class.
        /// </summary>
        public UsbContext()
        {
            IntPtr contextHandle = IntPtr.Zero;
            UsbLibrary.Init(ref contextHandle).ThrowOnError();
            context = Context.DangerousCreate(contextHandle);
            OpenDevices = new List<UsbDevice>();
            hotplugCallback = HotplugCallback;
            hotplugDelegatePtr = Marshal.GetFunctionPointerForDelegate(hotplugCallback);
        }

        ~UsbContext()
        {
            // Put cleanup code in Dispose(bool disposing).
            Dispose(false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            // Put cleanup code in Dispose(bool disposing).
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        private int HotplugCallback(IntPtr ctx, IntPtr device, HotplugEvent hotplugEvent, IntPtr userData)
        {
            var usbDevice = new UsbDevice(Device.DangerousCreate(device, true), this);
            if (hotplugEvent == HotplugEvent.DeviceArrived)
            {
                _deviceStateChangedStream.OnNext(DeviceState.Added);
                DeviceInfoDictionary.TryAdd(usbDevice, new CachedDeviceInfo(usbDevice));
            }
            else
            {
                if (!DeviceInfoDictionary.TryRemove(usbDevice, out var info))
                    throw new InvalidOperationException("Device info not found in dictionary.");
                _deviceStateChangedStream.OnNext(DeviceState.Removed);
            }

            return 0;
        }

        public void RegisterHotPlug()
        {
            if (IsUsingHotplug)
                return;
            if (UsbLibrary.HasCapability((uint)Capability.HasHotplug) == 0)
                throw new PlatformNotSupportedException("This platform does not support hotplug.");

            UsbLibrary.HotplugRegisterCallback(context, HotplugOptions.HotplugEventFlags,
                HotplugFlag.Enumerate, HotplugOptions.VendorId, HotplugOptions.ProductId, HotplugOptions.DeviceClass,
                hotplugDelegatePtr, IntPtr.Zero, ref HotplugOptions.Handle);
            StartHandlingEvents();
            IsUsingHotplug = true;
        }

        public void UnregisterHotPlug()
        {
            if (!IsUsingHotplug)
                return;

            Interlocked.Exchange(ref stopHandlingEvents, 1);
            UsbLibrary.HotplugDeregisterCallback(context, HotplugOptions.Handle);
            StopHandlingEvents();
            IsUsingHotplug = false;
        }

        /// <summary>
        /// Starts the event handling thread.
        /// </summary>
        public void StartHandlingEvents()
        {
            if (eventHandlingThread != null)
                return;

            eventHandlingThread = new Thread(HandleEvents)
            {
                IsBackground = true
            };

            stopHandlingEvents = 0;
            eventHandlingThread.Start();
        }

        /// <summary>
        /// Attempts to stop the event handling thread.
        /// </summary>
        /// <remarks>
        /// Note that <see cref="UsbLibrary.HandleEventsCompleted"/> must be woken up an event in order for this method to return.
        /// Ideally this will happen when the last device in <see cref="OpenDevices"/> is closed.
        /// </remarks>
        public void StopHandlingEvents()
        {
            if (eventHandlingThread == null)
                return;

            eventHandlingThread.Join();
            eventHandlingThread = null;
        }

        protected virtual void Dispose(bool disposeManagedObjects)
        {
            if (IsDisposed)
                return;

            IsDisposing = true;

            // Not sure what should go here, what resources should be cleaned up by explicit Dispose but not in the finalizer?
            if (disposeManagedObjects)
            {
                // Dispose managed state (managed objects).
            }

            // Close any devices still open on this context.
            foreach (var openDevice in OpenDevices)
            {
                openDevice.Dispose();
            }

            OpenDevices.Clear();

            UnregisterHotPlug();

            // Dispose of underlying context handle.
            context.Dispose();

            IsDisposed = true;
        }

        private void HandleEvents()
        {
            while (stopHandlingEvents == 0)
            {
                UsbLibrary.HandleEventsCompleted(context, ref stopHandlingEvents).ThrowOnError();
            }
        }
    }
}