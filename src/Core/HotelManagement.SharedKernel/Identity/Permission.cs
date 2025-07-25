namespace HotelManagement.Identity.Domain;

public static class Permissions
{
    // Booking permissions
    public const string ViewAllBookings = "bookings.view.all";
    public const string ViewOwnBookings = "bookings.view.own";
    public const string CreateBooking = "bookings.create";
    public const string CancelAnyBooking = "bookings.cancel.any";
    public const string CancelOwnBooking = "bookings.cancel.own";
    public const string ForceRefund = "bookings.refund.force";
    
    // Room permissions
    public const string ViewRoomDetails = "rooms.view.details";
    public const string ViewRoomCondition = "rooms.view.condition";
    public const string UpdateRoomCondition = "rooms.update.condition";
    public const string ManageRooms = "rooms.manage";
    
    // Housekeeping permissions
    public const string ViewCleaningTasks = "housekeeping.view.tasks";
    public const string UpdateCleaningTasks = "housekeeping.update.tasks";
    public const string ReportDamage = "housekeeping.report.damage";
    
    // Payment permissions
    public const string ProcessPayments = "payments.process";
    public const string ViewAllPayments = "payments.view.all";
    public const string ProcessRefunds = "payments.refund.process";
}