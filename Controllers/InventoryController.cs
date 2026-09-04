using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MaterniTrack.Data;
using MaterniTrack.Models;

namespace MaterniTrack.Controllers;

[Authorize]
public class InventoryController : Controller
{
    private readonly ApplicationDbContext _context;

    public InventoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Inventory
    public async Task<IActionResult> Index(string? search, string? category, string? status, string? sortBy)
    {
        var query = _context.InventoryItems.AsQueryable();

        // Search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(i =>
                i.Name.ToLower().Contains(s) ||
                (i.Supplier != null && i.Supplier.ToLower().Contains(s)));
        }

        // Category filter
        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(i => i.Category == category);

        // Load all for status calculation (status is computed, not stored)
        var allItems = await query.ToListAsync();

        // Status filter (computed property)
        if (!string.IsNullOrWhiteSpace(status))
            allItems = allItems.Where(i => i.CalculateStatus() == status).ToList();

        // Sort
        allItems = sortBy switch
        {
            "name-desc" => allItems.OrderByDescending(i => i.Name).ToList(),
            "qty-asc"   => allItems.OrderBy(i => i.Quantity).ToList(),
            "qty-desc"  => allItems.OrderByDescending(i => i.Quantity).ToList(),
            "expiry-asc" => allItems.OrderBy(i => i.ExpirationDate).ToList(),
            _           => allItems.OrderBy(i => i.Name).ToList()  // name-asc default
        };

        // Stats (computed from full unfiltered set for accuracy)
        var allForStats = await _context.InventoryItems.ToListAsync();
        var today = DateOnly.FromDateTime(DateTime.Today);

        ViewBag.TotalItems = allForStats.Count;
        ViewBag.LowStockCount = allForStats.Count(i => i.CalculateStatus() == "Low Stock");
        ViewBag.ExpiringCount = allForStats.Count(i =>
        {
            var days = i.ExpirationDate.DayNumber - today.DayNumber;
            return days >= 0 && days <= 30;
        });
        ViewBag.OutOfStockCount = allForStats.Count(i => i.Quantity == 0);

        // Pass back filters for UI state
        ViewBag.CurrentSearch   = search;
        ViewBag.CurrentCategory = category ?? "";
        ViewBag.CurrentStatus   = status ?? "";
        ViewBag.CurrentSort     = sortBy ?? "name-asc";

        // Low stock alert banner items
        ViewBag.LowStockItems = allForStats
            .Where(i => i.CalculateStatus() is "Low Stock" or "Out of Stock")
            .Select(i => i.Name)
            .ToList();

        if (TempData.ContainsKey("SuccessMessage"))
            ViewBag.SuccessMessage = TempData["SuccessMessage"];

        return View(allItems);
    }

    // GET: Inventory/Create
    public IActionResult Create()
    {
        var item = new InventoryItem
        {
            DateAdded = DateOnly.FromDateTime(DateTime.Today),
            LastUpdated = DateOnly.FromDateTime(DateTime.Today)
        };
        return View(item);
    }

    // POST: Inventory/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(InventoryItem item)
    {
        if (ModelState.IsValid)
        {
            item.DateAdded = DateOnly.FromDateTime(DateTime.Today);
            item.LastUpdated = DateOnly.FromDateTime(DateTime.Today);
            _context.InventoryItems.Add(item);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"\"{item.Name}\" added to inventory!";
            return RedirectToAction(nameof(Index));
        }
        return View(item);
    }

    // GET: Inventory/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();
        var item = await _context.InventoryItems.FindAsync(id);
        if (item == null) return NotFound();
        return View(item);
    }

    // POST: Inventory/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, InventoryItem item)
    {
        if (id != item.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                item.LastUpdated = DateOnly.FromDateTime(DateTime.Today);
                _context.Update(item);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"\"{item.Name}\" updated successfully!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.InventoryItems.AnyAsync(e => e.Id == item.Id))
                    return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }
        return View(item);
    }

    // POST: Inventory/QuickUpdate — updates just the quantity from the index page
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> QuickUpdate(int id, int newQuantity)
    {
        var item = await _context.InventoryItems.FindAsync(id);
        if (item != null)
        {
            item.Quantity = Math.Max(0, newQuantity);
            item.LastUpdated = DateOnly.FromDateTime(DateTime.Today);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"\"{item.Name}\" stock updated to {item.Quantity} {item.Unit}.";
        }
        return RedirectToAction(nameof(Index));
    }

    // POST: Inventory/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.InventoryItems.FindAsync(id);
        if (item != null)
        {
            _context.InventoryItems.Remove(item);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"\"{item.Name}\" removed from inventory.";
        }
        return RedirectToAction(nameof(Index));
    }
}
