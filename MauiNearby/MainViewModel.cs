using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MauiNearby.Models;
using MauiNearby.Services;
using Newtonsoft.Json;

namespace MauiNearby
{
	public class MainViewModel : INotifyPropertyChanged
	{
        private readonly NearbySyncService nearbySyncService;

        public event PropertyChangedEventHandler PropertyChanged;

		public string DeviceSummary => $"This Device: {nearbySyncService.DeviceId}";
        public string ServiceSummary => $"Service ID: {nearbySyncService.ServiceId}";

        public List<string> NearbyDevices => nearbySyncService.Devices.Select(d => d.Name + ":" + d.EndpointId).ToList();
        public string NearbyDevicesSummary => "Nearby Devices: " + string.Join(", ", NearbyDevices);

        public List<string> ConnectedDevices => nearbySyncService.ConnectedDevices.Select(d => d.Name + ":" + d.EndpointId).ToList();
        public string ConnectedDevicesSummary => "Connected Devices: " + string.Join(", ", ConnectedDevices);

        public List<Ticket> Tickets { get; set; } = new List<Ticket>();

        public List<Ticket> SortedTickets => Tickets.OrderByDescending(t => t.IssueDate).ToList();

        public void RaisePropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public MainViewModel()
		{
			nearbySyncService = NearbySyncService.Instance;

            nearbySyncService.OnPayloadReceived += NearbySyncService_OnPayloadReceived;
            nearbySyncService.OnDeviceConnected += NearbySyncService_OnDeviceConnected;
            nearbySyncService.OnDeviceDisconnected += NearbySyncService_OnDeviceDisconnected;
            nearbySyncService.OnDevicesDiscovered += NearbySyncService_OnDevicesDiscovered;
            nearbySyncService.OnDevicesLost += NearbySyncService_OnDevicesLost;

			CreateTicketCommand = new Command(async () => await CreateAndSendTicket());
        }

        private void NearbySyncService_OnDevicesLost(object sender, EventArgs e)
        {
            RaiseEvents();
        }

        public void RaiseEvents()
        {
            RaisePropertyChanged(nameof(DeviceSummary));
            RaisePropertyChanged(nameof(ServiceSummary));
            RaisePropertyChanged(nameof(NearbyDevices));
            RaisePropertyChanged(nameof(NearbyDevicesSummary));
            RaisePropertyChanged(nameof(ConnectedDevices));
            RaisePropertyChanged(nameof(ConnectedDevicesSummary));
        }

        private void NearbySyncService_OnDevicesDiscovered(object sender, EventArgs e)
        {
            RaiseEvents();
        }

        private void NearbySyncService_OnDeviceDisconnected(object sender, DeviceConnectionStateEventArgs e)
        {
            RaiseEvents();
        }

        private void NearbySyncService_OnDeviceConnected(object sender, DeviceConnectionStateEventArgs e)
        {
            RaiseEvents();
        }

        private void NearbySyncService_OnPayloadReceived(object sender, PayloadEventArgs e)
        {
            var incomingTickets = JsonConvert.DeserializeObject<List<Ticket>>(e.PayloadContent);
            if (incomingTickets == null || !incomingTickets.Any())
            {
                return;
            }

            var filteredTickets = incomingTickets.Where(t => t.CreatedByDeviceID != this.nearbySyncService.DeviceId).ToList();

            if (filteredTickets.Any())
            {
                this.Tickets.AddRange(filteredTickets);
                RaisePropertyChanged(nameof(SortedTickets));

                BroadcastTickets(filteredTickets).ConfigureAwait(false);
            }
        }

        private async Task CreateAndSendTicket()
        {
			var tickets = TicketBuilder.Build(1);

            Tickets.AddRange(tickets);

            RaisePropertyChanged(nameof(SortedTickets));

            BroadcastTickets(tickets).ConfigureAwait(false);
        }

        private async Task BroadcastTickets(List<Ticket> tickets)
        {
            if (tickets is null)
            {
                return;
            }

            var connectedDevices = nearbySyncService.ConnectedDevices;
            foreach (var device in connectedDevices)
            {
                var filteredTickets = tickets.Where(t => t.CreatedByDeviceID != device.EndpointId).ToList();

                if (filteredTickets != null && filteredTickets.Any())
                {
                    var json = JsonConvert.SerializeObject(filteredTickets);
                    await nearbySyncService.SendContent(device.EndpointId, json);
                }
            }
        }


		public ICommand CreateTicketCommand { get; }
    }
}

