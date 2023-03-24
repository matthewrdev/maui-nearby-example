#if __ANDROID__

using System;
using System.Text;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Common.Apis;
using Android.Gms.Nearby;
using Android.Gms.Nearby.Connection;
using Android.Gms.Nearby.Messages;
using Android.Gms.Tasks;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using MauiNearby.Models;
using static Google.Android.Material.CircularReveal.CircularRevealHelper;

namespace MauiNearby.Services
{
    public class NearbySyncService : INearbyEventsHandler
    {
        class SendContentListener : Java.Lang.Object, IOnCanceledListener, IOnCompleteListener, IOnFailureListener, IOnSuccessListener
        {
            private readonly Action<bool> callback;

            public SendContentListener(Action<bool> callback)
            {
                this.callback = callback;
            }

            public void OnCanceled()
            {
                callback?.Invoke(false);
            }

            public void OnComplete(Android.Gms.Tasks.Task task)
            {
                callback?.Invoke(true);
            }

            public void OnFailure(Java.Lang.Exception e)
            {
                // TODO: Log error
                callback?.Invoke(false);
            }

            public void OnSuccess(Java.Lang.Object result)
            {
                callback?.Invoke(true);
            }
        }

        public static bool hasPermissions(Context context, IReadOnlyList<string> permissions)
        {
            foreach (var permission in permissions)
            {
                if (ContextCompat.CheckSelfPermission(context, permission) != Permission.Granted)
                {
                    return false;
                }
            }
            return true;
        }

        private static IReadOnlyList<string> GetRequiredPermissions()
        {
            var requiredPermissions = Array.Empty<string>();

            if (Build.VERSION.SdkInt >= BuildVersionCodes.S)
            {
                requiredPermissions =
                      new String[] {
                    Manifest.Permission.BluetoothScan,
                    Manifest.Permission.BluetoothAdvertise,
                    Manifest.Permission.BluetoothConnect,
                    Manifest.Permission.AccessWifiState,
                    Manifest.Permission.ChangeWifiState,
                    Manifest.Permission.AccessCoarseLocation,
                    Manifest.Permission.AccessFineLocation,
                    Manifest.Permission.NearbyWifiDevices
                      };
            }
            else if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
            {
                requiredPermissions =
                    new String[] {
                    Manifest.Permission.BluetoothScan,
                    Manifest.Permission.BluetoothAdmin,
                    Manifest.Permission.AccessWifiState,
                    Manifest.Permission.ChangeWifiState,
                    Manifest.Permission.AccessCoarseLocation,
                    Manifest.Permission.AccessFineLocation,
                    Manifest.Permission.NearbyWifiDevices
                    };
            }
            else
            {
                requiredPermissions =
                      new String[] {
                    Manifest.Permission.BluetoothScan,
                    Manifest.Permission.BluetoothAdmin,
                    Manifest.Permission.AccessWifiState,
                    Manifest.Permission.ChangeWifiState,
                    Manifest.Permission.AccessCoarseLocation,
                    Manifest.Permission.NearbyWifiDevices
                      };
            }

            return requiredPermissions;
        }

        public static readonly NearbySyncService Instance = new NearbySyncService();
        private Activity activity;
        private EndpointDiscovery endpointDiscovery;
        private ConnectionLifecycle connectionsCallback;
        private PayloadReceiver payloadReceiver;
        IConnectionsClient client;
        public NearbySyncService()
        {
            // TODO: Init the peer api
        }

        public bool VerifyPermissions()
        {
            return hasPermissions(this.activity, GetRequiredPermissions());
        }


        public void TryRequestPermissions()
        {
            if (!hasPermissions(activity, GetRequiredPermissions()))
            {
                if ((int)Build.VERSION.SdkInt < 23)
                {
                    ActivityCompat.RequestPermissions(
                        activity, GetRequiredPermissions().ToArray(), 1);
                }
                else
                {
                    activity.RequestPermissions(GetRequiredPermissions().ToArray(), 1);
                }
            }
        }

        public string ServiceId { get; private set; }

        public string DeviceId { get; private set; }

        private readonly HashSet<string> connectedDevicesIds = new HashSet<string>();
        public IReadOnlyList<NearbyDevice> ConnectedDevices => Devices.Where(d => connectedDevicesIds.Contains(d.EndpointId)).ToList();

        private readonly Dictionary<string, NearbyDevice> devices = new Dictionary<string, NearbyDevice>();
        public IReadOnlyList<NearbyDevice> Devices => devices.Select(kp => kp.Value).ToList();

        public event EventHandler OnDevicesDiscovered;

        public event EventHandler OnDevicesLost;

        public event EventHandler<DeviceConnectionStateEventArgs> OnDeviceConnected;

        public event EventHandler<DeviceConnectionStateEventArgs> OnDeviceDisconnected;

        public event EventHandler<PayloadEventArgs> OnPayloadReceived;

        public bool IsAdvertising { get; }

        public bool IsDiscovering { get; }

        public void Initialise(Activity activity, string serviceId, string deviceId)
        {
            if (activity is null)
            {
                throw new ArgumentNullException(nameof(activity));
            }

            if (client != null)
            {
                return;
            }

            ServiceId = serviceId;
            DeviceId = deviceId;
            this.activity = activity;
            endpointDiscovery = new EndpointDiscovery(this);
            connectionsCallback = new ConnectionLifecycle(this);
            payloadReceiver = new PayloadReceiver(this);

            client = NearbyClass.GetConnectionsClient(activity);
        }

        public void Start()
        {
            StartAdvertising();
            StartDiscovery();
        }

        private void StartAdvertising()
        {
            AdvertisingOptions advertisingOptions = new AdvertisingOptions.Builder().SetStrategy(Android.Gms.Nearby.Connection.Strategy.P2pCluster).Build();
            client.StartAdvertising(this.DeviceId, this.ServiceId, connectionsCallback, advertisingOptions);
        }

        private void StartDiscovery()
        {
            DiscoveryOptions discoveryOptions = new DiscoveryOptions.Builder().SetStrategy(Android.Gms.Nearby.Connection.Strategy.P2pCluster).Build();
            client.StartDiscovery(this.ServiceId, endpointDiscovery, discoveryOptions); 
        }

        private void Stop()
        {
            client.StopAllEndpoints();
            StopDiscovery();
            StopAdvertising();
        }

        private void StopDiscovery()
        {
            client.StopDiscovery();
        }

        private void StopAdvertising()
        {
            client.StopAdvertising();
        }

        public async Task<bool> SendContent(string endpoint, string content)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
            {
                throw new ArgumentException($"'{nameof(endpoint)}' cannot be null or whitespace.", nameof(endpoint));
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException($"'{nameof(content)}' cannot be null or whitespace.", nameof(content));
            }
            var bytes = Encoding.UTF8.GetBytes(content);

            if (bytes.Length > 32_000)
            {
                throw new ArgumentOutOfRangeException("The byte encoded content must be less than 32K bytes. See: ");
            }

            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            Payload bytesPayload = Payload.FromBytes(bytes);

            var listener = new SendContentListener((success) =>
            {
                taskCompletionSource.TrySetResult(success);
            });

            client.SendPayload(endpoint, bytesPayload)
                  .AddOnCanceledListener(listener)
                  .AddOnCompleteListener(listener)
                  .AddOnFailureListener(listener)
                  .AddOnSuccessListener(listener);

            return await taskCompletionSource.Task;
        }

        public void OnEndpointFound(string endpointId, DiscoveredEndpointInfo info)
        {
            if (info.ServiceId != this.ServiceId)
            {
                return;
            }

            var nearbyDevice = new NearbyDevice(info.EndpointName, endpointId);

            this.devices[endpointId] = nearbyDevice;

            this.OnDevicesDiscovered?.Invoke(this, EventArgs.Empty);

            client.RequestConnection(this.DeviceId, endpointId, connectionsCallback);
        }

        public void OnEndpointLost(string endpointId)
        {
            if (this.devices.ContainsKey(endpointId.ToLowerInvariant()))
            {
                this.devices.Remove(endpointId.ToLowerInvariant());
                this.OnDevicesLost?.Invoke(this, EventArgs.Empty);
            }
        }

        public void OnConnectionInitiated(string endpointId, ConnectionInfo info)
        {
            client.AcceptConnection(endpointId, payloadReceiver);
        }

        public void OnConnectionResult(string endpointId, ConnectionResolution connectionResolution)
        {
            if (connectionResolution.Status.StatusCode == Statuses.ResultSuccess.StatusCode)
            {
                var foundDevice = devices.TryGetValue(endpointId, out var nearbyDevice);
                if (!foundDevice)
                {
                    nearbyDevice = new NearbyDevice(endpointId, endpointId);
                }
                this.connectedDevicesIds.Add(endpointId);
                this.OnDeviceConnected?.Invoke(this, new DeviceConnectionStateEventArgs(nearbyDevice, true));
            }
        }

        public void OnDisconnected(string endpointId)
        {
            if (this.connectedDevicesIds.Contains(endpointId))
            {
                this.connectedDevicesIds.Remove(endpointId);
                if (devices.TryGetValue(endpointId, out var nearbyDevice))
                {
                    this.OnDeviceDisconnected?.Invoke(this, new DeviceConnectionStateEventArgs(nearbyDevice, false));
                }
            }
        }

        void INearbyEventsHandler.OnPayloadReceived(string endpointId, Payload payload)
        {
            if (string.IsNullOrEmpty(endpointId))
            {
                throw new ArgumentException($"'{nameof(endpointId)}' cannot be null or empty.", nameof(endpointId));
            }

            if (payload is null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            if (!devices.TryGetValue(endpointId, out var nearbyDevice))
            {
                // TODO: Error log.
                return;
            }

            var bytes = payload.AsBytes();
            var content = Encoding.UTF8.GetString(bytes);

            this.OnPayloadReceived?.Invoke(this, new PayloadEventArgs(nearbyDevice, content));
        }

        public void OnPayloadTransferUpdate(string endpointId, PayloadTransferUpdate update)
        {
            // Used for file and stream transfers.
        }
    }
}

#endif