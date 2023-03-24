#if __ANDROID__

using Android.Gms.Nearby.Connection;
using static Android.Icu.Text.IDNA;

namespace MauiNearby.Services
{
    public class EndpointDiscovery : EndpointDiscoveryCallback
    {
        private readonly INearbyEventsHandler handler;

        public EndpointDiscovery(INearbyEventsHandler handler)
        {
            if (handler is null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            this.handler = handler;
        }

        public override void OnEndpointFound(string endpointId, DiscoveredEndpointInfo info)
        {
            Console.WriteLine("OnEndpointFound:" + endpointId + ", " + info.EndpointName + ", " + info.ServiceId);
            handler.OnEndpointFound(endpointId, info);
        }

        public override void OnEndpointLost(string endpointId)
        {
            Console.WriteLine("OnEndpointLost:" + endpointId);
            handler.OnEndpointLost(endpointId);
        }
    }
}

#endif