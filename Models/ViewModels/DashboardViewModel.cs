namespace MaterniTrack.Models.ViewModels;

public class DashboardViewModel
{
    public string UserName { get; set; } = string.Empty;
    public string UserRole { get; set; } = "Staff";
    public string UserInitials { get; set; } = "LM";

    public int TodayAppointmentsCount { get; set; }
    public int TotalPatientsCount { get; set; }
    public int TotalSuppliesCount { get; set; }

    public List<Appointment> TodayAppointments { get; set; } = new();
    public List<InventoryItem> DisplayInventory { get; set; } = new();
    public List<InventoryItem> LowStockAlertItems { get; set; } = new();
}
