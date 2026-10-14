using BookingSystem.Models;
using BookingSystem.Repositories;

namespace BookingSystem.Services;

public class BookingService(IBookingRepository repository)
{
    private static readonly IReadOnlyDictionary<string, Room> Rooms =
        new Dictionary<string, Room>(StringComparer.OrdinalIgnoreCase)
        {
            ["Aspen"] = new("Aspen", 4),
            ["Birken"] = new("Birken", 8),
            ["Eken"] = new("Eken", 12)
        };

    public IReadOnlyList<Booking> GetActiveBookings()
    {
        IReadOnlyList<Booking> bookings = repository.GetAll();

        IReadOnlyList<Booking> activeBookings = [.. from booking in bookings
                                                    where booking.IsCancelled == false
                                                    orderby booking.Date, booking.StartTime
                                                    select booking];
        return activeBookings;
    }

    public IReadOnlyList<Booking> Search(string searchTerm)
    {
        return repository.GetAll()
            .Where(booking =>
                !booking.IsCancelled &&
                booking.CustomerName.ToLower().Contains(searchTerm.ToLower()))
            .ToList();
    }

    public BookingResult Create(Booking booking)
    {
        var validationResult = Validate(booking);

        if (!validationResult.IsSuccessful)
        {
            return validationResult;
        }

        booking.Id = repository.GetAll().Select(item => item.Id).DefaultIfEmpty(1000).Max() + 1;
        repository.Add(booking);
        repository.Save();

        return BookingResult.Success($"Booking {booking.Id} was created.");
    }

    public BookingResult Cancel(int bookingId)
    {
        Booking? booking = repository.GetAll().FirstOrDefault(existing => existing.Id == bookingId);

        if (booking is null)
        {
            return BookingResult.Failure($"No booking with the id '{bookingId}' could be found.");
        }

        if (booking.IsCancelled)
        {
            return BookingResult.Failure($"The booking with id '{booking.Id}' has already been cancelled.");
        }

        booking.IsCancelled = true;
        repository.Save();

        return BookingResult.Success($"The booking with id '{bookingId}' has been cancelled.");
    }

    private BookingResult Validate(Booking booking)
    {
        if (string.IsNullOrWhiteSpace(booking.CustomerName))
        {
            return BookingResult.Failure("Customer name is required.");
        }

        if (!Rooms.TryGetValue(booking.RoomName, out var room))
        {
            return BookingResult.Failure("The selected room does not exist.");
        }

        if (booking.StartTime >= booking.EndTime)
        {
            return BookingResult.Failure("The start time must be earlier than the end time.");
        }

        if (booking.Participants < 1)
        {
            return BookingResult.Failure("The number of participants must be at least 1.");
        }

        if (booking.Participants > room.Capacity)
        {
            return BookingResult.Failure($"{room.Name} has a maximum capacity of {room.Capacity}.");
        }

        var hasOverlap = repository.GetAll().Any(existing =>
            !existing.IsCancelled &&
            existing.Date == booking.Date &&
            existing.RoomName.Equals(booking.RoomName, StringComparison.OrdinalIgnoreCase) &&
            booking.StartTime >= existing.StartTime &&
            booking.StartTime < existing.EndTime ||
            !existing.IsCancelled &&
            existing.Date == booking.Date &&
            existing.RoomName.Equals(booking.RoomName, StringComparison.OrdinalIgnoreCase) &&
            booking.StartTime <= existing.StartTime &&
            booking.EndTime > existing.StartTime);

        if (hasOverlap)
        {
            return BookingResult.Failure("The room is already booked during the selected time.");
        }

        return BookingResult.Success("The booking is valid.");
    }
}
