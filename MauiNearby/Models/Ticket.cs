using System;
using Newtonsoft.Json;

namespace MauiNearby.Models
{
	public class Ticket
	{
		public string Id { get; set; }

		public string CreatedByDeviceID { get; set; }

		public DateTime IssueDate { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string Phone { get; set; }

		[JsonIgnore]
		public string Summary
		{
			get => $"Device {CreatedByDeviceID} at {IssueDate.ToString("O")} for {FirstName} {LastName} (ID: {Id})";
		}
	}
}

