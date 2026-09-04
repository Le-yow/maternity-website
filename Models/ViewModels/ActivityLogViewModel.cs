namespace MaterniTrack.Models.ViewModels;

public class ActivityLogViewModel
{
    public List<ActivityLog> Logs { get; set; } = new();
    public string CurrentCategory { get; set; } = "all";
    public string CurrentSearch { get; set; } = string.Empty;
    public int TotalLogsCount { get; set; }
    public int AuthCount { get; set; }
    public int PatientCount { get; set; }
    public int AppointmentCount { get; set; }
    public int InventoryCount { get; set; }
    public int SettingsCount { get; set; }
}
