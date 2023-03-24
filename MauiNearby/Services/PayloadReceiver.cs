#if __ANDROID__
using System;

using System.Net;
using Android.Gms.Nearby.Connection;

namespace MauiNearby.Services
{
    public class PayloadReceiver : PayloadCallback
    {
        private readonly INearbyEventsHandler handler;

        public PayloadReceiver(INearbyEventsHandler handler)
        {
            if (handler is null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            this.handler = handler;
        }

        public override void OnPayloadReceived(string endpointId, Payload payload)
        {
            Console.WriteLine("OnPayloadReceived:" + endpointId);
            handler.OnPayloadReceived(endpointId, payload);
        }

        public override void OnPayloadTransferUpdate(string endpointId, PayloadTransferUpdate update)
        {
            Console.WriteLine("OnPayloadTransferUpdate:" + endpointId + ", " + update.PayloadId);
            handler.OnPayloadTransferUpdate(endpointId, update);
        }
    }
}
#endif