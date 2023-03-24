using System;
using MauiNearby.Models;
using MauiNearby.Services;

namespace MauiNearby
{
    public static class TicketBuilder
    {
        public static List<Ticket> Build(int amount)
        {
            List<Ticket> tickets = new List<Ticket>();

            for (var i = 0; i < amount; ++i)
            {
                var ticket = new Ticket()
                {
                    Id = Guid.NewGuid().ToString(),
                    CreatedByDeviceID = NearbySyncService.Instance.DeviceId,
                    FirstName = Faker.Name.First(),
                    LastName = Faker.Name.Last(),
                    IssueDate = DateTime.UtcNow,
                    Phone = Faker.Phone.Number()
                };

                tickets.Add(ticket);
            }

            return tickets;
        }
    }
}

