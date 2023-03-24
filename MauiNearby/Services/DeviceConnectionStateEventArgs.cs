using System;
using MauiNearby.Models;

namespace MauiNearby.Services
{
	public class DeviceConnectionStateEventArgs : EventArgs
	{
		public DeviceConnectionStateEventArgs(NearbyDevice device, bool isConnected)
		{
            if (device is null)
            {
                throw new ArgumentNullException(nameof(device));
            }
            Device = device;
            IsConnected = isConnected;
        }

        public NearbyDevice Device { get; }

        public bool IsConnected { get; }
    }
}

