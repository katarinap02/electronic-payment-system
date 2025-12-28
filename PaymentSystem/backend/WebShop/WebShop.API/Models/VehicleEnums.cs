namespace WebShop.API.Models
{
    public enum VehicleCategory
    {
        Economy = 1,
        Comfort = 2,
        Luxury = 3,
        SUV = 4,
        Van = 5,
        Sport = 6
    }

    public enum TransmissionType
    {
        Manual = 1,
        Automatic = 2
    }

    public enum FuelType
    {
        Petrol = 1,
        Diesel = 2,
        Electric = 3,
        Hybrid = 4
    }

    public enum VehicleStatus
    {
        Available = 1,
        Rented = 2,
        Maintenance = 3,
        Unavailable = 4
    }
}
