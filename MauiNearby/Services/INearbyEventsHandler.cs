#if __ANDROID__

using Android.Gms.Nearby.Connection;

namespace MauiNearby.Services
{
    public interface INearbyEventsHandler
    {
        void OnEndpointFound(string endpointId, DiscoveredEndpointInfo info);
        void OnEndpointLost(string endpointId);

        void OnConnectionInitiated(string endpointId, ConnectionInfo info);
        void OnConnectionResult(string endpointId, ConnectionResolution connectionResolution);
        void OnDisconnected(string endpointId);

        void OnPayloadReceived(string endpointId, Payload payload);
        void OnPayloadTransferUpdate(string endpointId, PayloadTransferUpdate update);
    }
}

#endif