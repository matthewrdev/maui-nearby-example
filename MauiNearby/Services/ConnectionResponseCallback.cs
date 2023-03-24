#if __ANDROID__

using Android.Gms.Nearby.Connection;

namespace MauiNearby.Services
{
    class ConnectionLifecycle : ConnectionLifecycleCallback
    {
        private readonly INearbyEventsHandler handler;

        public ConnectionLifecycle(INearbyEventsHandler handler)
        {
            if (handler is null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            this.handler = handler;
        }
        public override void OnConnectionInitiated(string endpointId, ConnectionInfo p1)
        {
            Console.WriteLine("OnConnectionInitiated:" + endpointId);
            handler.OnConnectionInitiated(endpointId, p1);
        }

        public override void OnConnectionResult(string endpointId, ConnectionResolution p1)
        {
            Console.WriteLine("OnConnectionResult:" + endpointId);
            handler.OnConnectionResult(endpointId, p1);
        }

        public override void OnDisconnected(string endpointId)
        {
            Console.WriteLine("OnDisconnected:" + endpointId);
            handler.OnDisconnected(endpointId);
        }
    }
}

#endif