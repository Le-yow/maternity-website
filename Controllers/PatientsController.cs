using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaterniTrack.Data;
using MaterniTrack.Models;

namespace MaterniTrack.Controllers;

[Authorize]
public class PatientsController : Controller
{
    private readonly ApplicationDbContext _context;

    public PatientsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Patients
    public async Task<IActionResult> Index(string? search, string? status, string? sortBy)
    {
        var query = _context.Patients.AsQueryable();

        // 1. Search Filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(p => 
                p.FullName.ToLower().Contains(s) || 
                p.Contact.ToLower().Contains(s) || 
                (p.Email != null && p.Email.ToLower().Contains(s)));
        }

        // 2. Status Filter
        if (!string.IsNullOrWhiteSpace(status) && status != "all")
        {
            query = query.Where(p => p.Status.ToLower() == status.ToLower());
        }

        // 3. Sorting
        query = sortBy switch
        {
            "name-desc" => query.OrderByDescending(p => p.FullName),
            "date-new" => query.OrderByDescending(p => p.DateAdded).ThenByDescending(p => p.Id),
            "date-old" => query.OrderBy(p => p.DateAdded).ThenBy(p => p.Id),
            "age-asc" => query.OrderBy(p => p.Age),
            "age-desc" => query.OrderByDescending(p => p.Age),
            _ => query.OrderBy(p => p.FullName) // default: name-asc
        };

        var patients = await query.ToListAsync();

        // Calculate Stats
        var allPatients = await _context.Patients.ToListAsync();
        var currentMonth = DateTime.Today.Month;
        var currentYear = DateTime.Today.Year;

        ViewBag.TotalPatients = allPatients.Count;
        ViewBag.AddedThisMonth = allPatients.Count(p => p.DateAdded.Month == currentMonth && p.DateAdded.Year == currentYear);
        ViewBag.ActivePatients = allPatients.Count(p => p.Status.Equals("active", StringComparison.OrdinalIgnoreCase));

        ViewBag.CurrentSearch = search;
        ViewBag.CurrentStatus = status ?? "all";
        ViewBag.CurrentSort = sortBy ?? "name-asc";

        return View(patients);
    }

    // GET: Patients/Create
    public IActionResult Create()
    {
        var patient = new Patient
        {
            Status = "active",
            DateAdded = DateOnly.FromDateTime(DateTime.Today)
        };
        return View(patient);
    }

    // POST: Patients/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Patient patient)
    {
        if (ModelState.IsValid)
        {
            patient.DateAdded = DateOnly.FromDateTime(DateTime.Today);
            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Patient record created successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(patient);
    }

    // GET: Patients/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var patient = await _context.Patients.FindAsync(id);
        if (patient == null) return NotFound();

        return View(patient);
    }

    // POST: Patients/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Patient patient)
    {
        if (id != patient.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(patient);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Patient record updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Patients.AnyAsync(e => e.Id == patient.Id))
                {
                    return NotFound();
                }
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(patient);
    }

    // POST: Patients/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var patient = await _context.Patients.FindAsync(id);
        if (patient != null)
        {
            _context.Patients.Remove(patient);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Patient record deleted successfully!";
        }
        return RedirectToAction(nameof(Index));
    }
}
