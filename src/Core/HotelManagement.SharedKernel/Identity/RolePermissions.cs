namespace HotelManagement.Identity.Domain;

public static class RolePermissions
{
    private static readonly Dictionary<UserRole, List<string>> _permissions = new()
    {
        [UserRole.Customer] = new List<string>
        {
            Permissions.ViewOwnBookings,
            Permissions.CreateBooking,
            Permissions.CancelOwnBooking,
        },

        [UserRole.Receptionist] = new List<string>
        {
            Permissions.ViewAllBookings,
            Permissions.ViewOwnBookings,
            Permissions.CreateBooking,
            Permissions.CancelAnyBooking,
            Permissions.CancelOwnBooking,
            Permissions.ForceRefund,
            Permissions.ViewRoomDetails,
            Permissions.ViewRoomCondition,
            Permissions.ProcessPayments,
            Permissions.ViewAllPayments,
            Permissions.ProcessRefunds,
        },

        [UserRole.HousekeepingStaff] = new List<string>
        {
            Permissions.ViewCleaningTasks,
            Permissions.UpdateCleaningTasks,
            Permissions.ReportDamage,
            Permissions.ViewRoomCondition,
        },

        [UserRole.Manager] = new List<string>
        {
            // Toutes les permissions
            Permissions.ViewAllBookings,
            Permissions.ViewOwnBookings,
            Permissions.CreateBooking,
            Permissions.CancelAnyBooking,
            Permissions.CancelOwnBooking,
            Permissions.ForceRefund,
            Permissions.ViewRoomDetails,
            Permissions.ViewRoomCondition,
            Permissions.UpdateRoomCondition,
            Permissions.ManageRooms,
            Permissions.ViewCleaningTasks,
            Permissions.UpdateCleaningTasks,
            Permissions.ReportDamage,
            Permissions.ProcessPayments,
            Permissions.ViewAllPayments,
            Permissions.ProcessRefunds,
        },

        [UserRole.Administrator] = new List<string>
        {
            // Administrateur = toutes les permissions
            "*"
        }
    };

    public static bool HasPermission(UserRole role, string permission)
    {
        if (!_permissions.ContainsKey(role))
            return false;

        var rolePermissions = _permissions[role];
        return rolePermissions.Contains("*") || rolePermissions.Contains(permission);
    }

    public static List<string> GetPermissions(UserRole role)
    {
        return _permissions.ContainsKey(role) ? _permissions[role] : new List<string>();
    }
}