using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaterniTrack.Data;
using MaterniTrack.Models;

namespace MaterniTrack.Controllers;

[Authorize]
public class AppointmentsController : Controller
{
    private readonly ApplicationDbContext _context;

    public AppointmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Appointments?year=2026&month=8&selectedDate=2026-08-28&filter=all
    public async Task<IActionResult> Index(int? year, int? month, string? selectedDate, string? filter)
    {
        var today = DateTime.Today;
        var calYear = year ?? today.Year;
        var calMonth = month ?? today.Month;

        // Clamp month
        if (calMonth < 1) { calMonth = 12; calYear--; }
        if (calMonth > 12) { calMonth = 1; calYear++; }

        // Selected date defaults to today
        DateOnly pickedDate = selectedDate != null && DateOnly.TryParse(selectedDate, out var parsed)
            ? parsed
            : DateOnly.FromDateTime(today);

        // Load all appointments for this calendar month (for dot indicators)
        var monthStart = new DateOnly(calYear, calMonth, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);
        var monthAppointments = await _context.Appointments
            .Where(a => a.AppointmentDate >= monthStart && a.AppointmentDate <= monthEnd)
            .ToListAsync();

        // Load appointments for selected date
        var dayQuery = _context.Appointments
            .Where(a => a.AppointmentDate == pickedDate);

        if (!string.IsNullOrWhiteSpace(filter) && filter != "all")
        {
            dayQuery = dayQuery.Where(a => a.Status == filter);
        }

        var dayAppointments = await dayQuery.OrderBy(a => a.AppointmentTime).ToListAsync();

        // Stats for the selected date
        ViewBag.SelectedDate = pickedDate;
        ViewBag.CalYear = calYear;
        ViewBag.CalMonth = calMonth;
        ViewBag.CurrentFilter = filter ?? "all";
        ViewBag.DayAppointments = dayAppointments;
        ViewBag.DaysWithAppointments = monthAppointments
            .Select(a => a.AppointmentDate.Day)
            .Distinct()
            .ToHashSet();

        if (TempData.ContainsKey("SuccessMessage"))
            ViewBag.SuccessMessage = TempData["SuccessMessage"];

        return View();
    }

    // GET: Appointments/Create
    public IActionResult Create()
    {
        var apt = new Appointment
        {
            AppointmentDate = DateOnly.FromDateTime(DateTime.Today),
            AppointmentTime = "09:00",
            Status = "pending"
        };
        return View(apt);
    }

    // POST: Appointments/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Appointment appointment)
    {
        if (ModelState.IsValid)
        {
            // Conflict check: same staff, same date & time, not cancelled
            bool conflict = await _context.Appointments.AnyAsync(a =>
                a.AssignedStaff == appointment.AssignedStaff &&
                a.AppointmentDate == appointment.AppointmentDate &&
                a.AppointmentTime == appointment.AppointmentTime &&
                a.Status != "cancelled");

            if (conflict)
            {
                ModelState.AddModelError(string.Empty,
                    $"{appointment.AssignedStaff.Replace("-", " ")} already has an appointment at this time. Please choose a different time slot.");
                return View(appointment);
            }

            appointment.DateCreated = DateOnly.FromDateTime(DateTime.Today);
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Appointment scheduled successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(appointment);
    }

    // GET: Appointments/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment == null) return NotFound();
        return View(appointment);
    }

    // POST: Appointments/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Appointment appointment)
    {
        if (id != appointment.Id) return NotFound();

        if (ModelState.IsValid)
        {
            // Conflict check: exclude self (id != appointment.Id)
            bool conflict = await _context.Appointments.AnyAsync(a =>
                a.AssignedStaff == appointment.AssignedStaff &&
                a.AppointmentDate == appointment.AppointmentDate &&
                a.AppointmentTime == appointment.AppointmentTime &&
                a.Status != "cancelled" &&
                a.Id != appointment.Id);

            if (conflict)
            {
                ModelState.AddModelError(string.Empty,
                    $"{appointment.AssignedStaff.Replace("-", " ")} already has an appointment at this time. Please choose a different time slot.");
                return View(appointment);
            }

            try
            {
                _context.Update(appointment);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Appointment updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Appointments.AnyAsync(e => e.Id == appointment.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(appointment);
    }

    // POST: Appointments/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment != null)
        {
            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Appointment deleted.";
        }
        return RedirectToAction(nameof(Index));
    }

    // POST: Appointments/UpdateStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, string status, string returnDate)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment != null)
        {
            appointment.Status = status;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index), new { selectedDate = returnDate });
    }
}
