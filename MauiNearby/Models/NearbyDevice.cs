using System;
namespace MauiNearby.Models
{
	public class NearbyDevice
	{
        public NearbyDevice(string name, string endpointId)
        {
            Name = name;
            EndpointId = endpointId;
        }

		public string Name { get;  }

		public string EndpointId { get; }
	}
}

