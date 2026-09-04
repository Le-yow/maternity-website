using System.ComponentModel.DataAnnotations;

namespace MaterniTrack.Models;

public class InventoryItem
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Item name is required")]
    [Display(Name = "Item Name")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required")]
    [StringLength(50)]
    public string Category { get; set; } = "Medicine"; // "Medicine", "Supplies", "Equipment"

    [Required]
    [Range(0, 100000, ErrorMessage = "Quantity must be 0 or greater")]
    public int Quantity { get; set; }

    [Required(ErrorMessage = "Unit is required")]
    [StringLength(30)]
    public string Unit { get; set; } = "unit"; // "vial", "pack", "box", "bag", "set", "unit"

    [Required]
    [Range(0, 100000, ErrorMessage = "Reorder level must be 0 or greater")]
    [Display(Name = "Reorder Level")]
    public int ReorderLevel { get; set; }

    [Required(ErrorMessage = "Expiration date is required")]
    [Display(Name = "Expiration Date")]
    [DataType(DataType.Date)]
    public DateOnly ExpirationDate { get; set; }

    [StringLength(150)]
    public string? Supplier { get; set; }

    [Display(Name = "Date Added")]
    public DateOnly DateAdded { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    [Display(Name = "Last Updated")]
    public DateOnly LastUpdated { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    // Calculated status
    public string CalculateStatus()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var daysUntilExpiry = ExpirationDate.DayNumber - today.DayNumber;

        if (Quantity == 0) return "Out of Stock";
        if (Quantity <= ReorderLevel) return "Low Stock";
        if (daysUntilExpiry <= 30 && daysUntilExpiry >= 0) return "Expiring Soon";
        return "Good Stock";
    }

    public string StatusClass => CalculateStatus() switch
    {
        "Out of Stock" => "status-low-stock",
        "Low Stock" => "status-running-low",
        "Expiring Soon" => "status-running-low",
        _ => "status-good"
    };

    public string BarClass => CalculateStatus() switch
    {
        "Out of Stock" => "fill-low-stock",
        "Low Stock" => "fill-running-low",
        "Expiring Soon" => "fill-running-low",
        _ => "fill-good"
    };
}
