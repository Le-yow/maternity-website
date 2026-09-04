using MaterniTrack.Models;

namespace MaterniTrack.Models.ViewModels;

public class ReportsViewModel
{
    public int DailyAppointmentsCount { get; set; }
    public int DailyPatientsCount { get; set; }
    public string TodayDateFormatted { get; set; } = string.Empty;

    public int MonthlyAppointmentsCount { get; set; }
    public int MonthlyPatientsCount { get; set; }
    public int ActivePatientsCount { get; set; }

    public List<Appointment> TodayAppointments { get; set; } = new();
    public List<Patient> RecentPatients { get; set; } = new();
}
