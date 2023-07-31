namespace EventsAPIConsumer.Events.Models;

public class Room
{
    public string Id { get; set; }
    public string RoomClassDescription { get; set; }
    public bool RFlag { get; set; }

    public bool IsValidBedroom => !RFlag && RoomClassDescription == "Bedroom";
}
