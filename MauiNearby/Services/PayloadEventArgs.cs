using System;
using MauiNearby.Models;

namespace MauiNearby.Services
{
	public class PayloadEventArgs : EventArgs
	{
		public PayloadEventArgs(NearbyDevice device, string payloadContent)
		{
            if (string.IsNullOrWhiteSpace(payloadContent))
            {
                throw new ArgumentException($"'{nameof(payloadContent)}' cannot be null or whitespace.", nameof(payloadContent));
            }

            Device = device ?? throw new ArgumentNullException(nameof(device));
            PayloadContent = payloadContent;
        }

        public NearbyDevice Device { get; }
        public string PayloadContent { get; }
    }
}

