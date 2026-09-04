namespace MaterniTrack.Models.ViewModels;

public class AboutSystemViewModel
{
    public string AppName { get; set; } = "MaterniTrack Clinical Management System";
    public string Version { get; set; } = "2.4.0 (Enterprise Clinic Edition)";
    public string ReleaseDate { get; set; } = "March 2026";
    public string Framework { get; set; } = "ASP.NET Core 9.0 (C# 13)";
    public string DatabaseProvider { get; set; } = "Entity Framework Core 9 (SQLite Local DB)";
    public string EnvironmentName { get; set; } = "Production / Local Clinical Host";
    public string Compliance { get; set; } = "DOH Clinical Facility Standards & PhilHealth e-Claims Ready";
    public int TotalPatientsCount { get; set; }
    public int TotalAppointmentsCount { get; set; }
    public int TotalInventoryItemsCount { get; set; }
    public int TotalUsersCount { get; set; }
    public bool DatabaseConnected { get; set; } = true;
    public DateTime ServerTime { get; set; } = DateTime.Now;
}
