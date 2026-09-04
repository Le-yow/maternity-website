using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MaterniTrack.Models;

namespace MaterniTrack.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Apply migrations automatically
        await context.Database.EnsureCreatedAsync();

        // 1. Seed Roles: Doctor (Admin) & Staff
        string[] roles = { "Doctor", "Staff" };
        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // 2. Seed Default Doctor/Admin User
        var adminEmail = "admin@clinic.local";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Dr. Leyo Mendoza",
                ClinicRole = "Doctor",
                Status = "Active",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Doctor");
            }
        }

        // 3. Seed Default Staff User
        var staffEmail = "staff@clinic.local";
        var staffUser = await userManager.FindByEmailAsync(staffEmail);
        if (staffUser == null)
        {
            staffUser = new ApplicationUser
            {
                UserName = staffEmail,
                Email = staffEmail,
                FullName = "Juan Santos",
                ClinicRole = "Staff",
                Status = "Active",
                EmailConfirmed = true
            };
            var result = await userManager.CreateAsync(staffUser, "Staff123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(staffUser, "Staff");
            }
        }

        // 4. Seed Patients if none exist
        if (!await context.Patients.AnyAsync())
        {
            var patients = new List<Patient>
            {
                new()
                {
                    FullName = "Maria Santos",
                    Age = 28,
                    Contact = "0917-123-4567",
                    Email = "maria.santos@email.com",
                    Address = "Barangay 1, Real, Laguna",
                    MedicalHistory = "No previous surgeries. Regular prenatal check-ups.",
                    Allergies = "Penicillin (mild rash)",
                    Status = "active",
                    DateAdded = new DateOnly(2026, 1, 10)
                },
                new()
                {
                    FullName = "Rosa Reyes",
                    Age = 32,
                    Contact = "0918-234-5678",
                    Email = "rosa.reyes@email.com",
                    Address = "Barangay 2, Real, Laguna",
                    MedicalHistory = "Thyroid condition managed with medication. Previous C-section in 2020.",
                    Allergies = "None known",
                    Status = "active",
                    DateAdded = new DateOnly(2026, 1, 15)
                },
                new()
                {
                    FullName = "Ana Villalobos",
                    Age = 25,
                    Contact = "0919-345-6789",
                    Email = "ana.villalobos@email.com",
                    Address = "Barangay 3, Real, Laguna",
                    MedicalHistory = "First-time pregnancy. Good overall health.",
                    Allergies = "Latex",
                    Status = "active",
                    DateAdded = new DateOnly(2026, 2, 20)
                },
                new()
                {
                    FullName = "Carmen Gonzales",
                    Age = 35,
                    Contact = "0920-456-7890",
                    Email = "carmen.gonzales@email.com",
                    Address = "Barangay 4, Real, Laguna",
                    MedicalHistory = "Gestational diabetes managed with diet. Monitor closely.",
                    Allergies = "Aspirin",
                    Status = "active",
                    DateAdded = new DateOnly(2026, 3, 1)
                }
            };
            await context.Patients.AddRangeAsync(patients);
            await context.SaveChangesAsync();
        }

        // 5. Seed Inventory Items if none exist
        if (!await context.InventoryItems.AnyAsync())
        {
            var items = new List<InventoryItem>
            {
                new()
                {
                    Name = "Oxytocin",
                    Category = "Medicine",
                    Quantity = 80,
                    Unit = "vial",
                    ReorderLevel = 20,
                    ExpirationDate = new DateOnly(2026, 12, 31),
                    Supplier = "PhilMed Supplies Inc.",
                    DateAdded = new DateOnly(2026, 1, 15),
                    LastUpdated = DateOnly.FromDateTime(DateTime.Today)
                },
                new()
                {
                    Name = "Gauze Pads",
                    Category = "Supplies",
                    Quantity = 5,
                    Unit = "pack",
                    ReorderLevel = 15,
                    ExpirationDate = new DateOnly(2027, 6, 30),
                    Supplier = "MedChoice PH",
                    DateAdded = new DateOnly(2026, 1, 15),
                    LastUpdated = DateOnly.FromDateTime(DateTime.Today)
                },
                new()
                {
                    Name = "Surgical Gloves",
                    Category = "Supplies",
                    Quantity = 8,
                    Unit = "box",
                    ReorderLevel = 10,
                    ExpirationDate = new DateOnly(2027, 3, 15),
                    Supplier = "MedChoice PH",
                    DateAdded = new DateOnly(2026, 1, 15),
                    LastUpdated = DateOnly.FromDateTime(DateTime.Today)
                },
                new()
                {
                    Name = "IV Fluids",
                    Category = "Medicine",
                    Quantity = 45,
                    Unit = "bag",
                    ReorderLevel = 15,
                    ExpirationDate = new DateOnly(2026, 9, 30),
                    Supplier = "PhilMed Supplies Inc.",
                    DateAdded = new DateOnly(2026, 1, 15),
                    LastUpdated = DateOnly.FromDateTime(DateTime.Today)
                },
                new()
                {
                    Name = "Delivery Kit",
                    Category = "Equipment",
                    Quantity = 0,
                    Unit = "set",
                    ReorderLevel = 2,
                    ExpirationDate = new DateOnly(2028, 1, 1),
                    Supplier = "MediTech Solutions",
                    DateAdded = new DateOnly(2026, 2, 10),
                    LastUpdated = DateOnly.FromDateTime(DateTime.Today)
                },
                new()
                {
                    Name = "Sterilization Unit",
                    Category = "Equipment",
                    Quantity = 3,
                    Unit = "unit",
                    ReorderLevel = 1,
                    ExpirationDate = new DateOnly(2030, 6, 15),
                    Supplier = "PhilMed Supplies Inc.",
                    DateAdded = new DateOnly(2025, 11, 20),
                    LastUpdated = DateOnly.FromDateTime(DateTime.Today)
                }
            };
            await context.InventoryItems.AddRangeAsync(items);
            await context.SaveChangesAsync();
        }

        // 6. Seed Sample Appointments for Today
        if (!await context.Appointments.AnyAsync())
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var appointments = new List<Appointment>
            {
                new()
                {
                    PatientName = "Maria Santos",
                    ContactNumber = "0917-123-4567",
                    AppointmentDate = today,
                    AppointmentTime = "09:00",
                    AppointmentType = "prenatal",
                    AssignedStaff = "dr-real-mendoza",
                    Status = "confirmed",
                    Notes = "Routine monthly prenatal visit",
                    DateCreated = today
                },
                new()
                {
                    PatientName = "Rosa Reyes",
                    ContactNumber = "0918-234-5678",
                    AppointmentDate = today,
                    AppointmentTime = "10:30",
                    AppointmentType = "consultation",
                    AssignedStaff = "juan-santos",
                    Status = "pending",
                    Notes = "Thyroid prescription evaluation",
                    DateCreated = today
                },
                new()
                {
                    PatientName = "Ana Villalobos",
                    ContactNumber = "0919-345-6789",
                    AppointmentDate = today,
                    AppointmentTime = "14:00",
                    AppointmentType = "prenatal",
                    AssignedStaff = "dr-real-mendoza",
                    Status = "confirmed",
                    Notes = "Ultrasound review",
                    DateCreated = today
                }
            };
            await context.Appointments.AddRangeAsync(appointments);
            await context.SaveChangesAsync();
        }

        // 7. Seed Clinic Profile Settings if none exist
        if (!await context.ClinicSettings.AnyAsync())
        {
            var clinicSetting = new ClinicProfileSetting
            {
                ClinicName = "Real-Mendoza Maternity Clinic",
                Specialization = "Maternal & Neonatal Healthcare Facility",
                MedicalDirector = "Dr. Leyo Mendoza, MD, OB-GYN",
                DohLicense = "DOH-NCR-CL-2026-08492",
                PhilHealthAccreditation = "PH-ACCRED-9482710",
                ContactPhone = "+63 917 123 4567",
                ContactEmail = "admin@clinic.local",
                Address = "District 2, Marikina City, Metro Manila",
                OperatingHours = "Monday - Saturday: 8:00 AM - 5:00 PM (Emergency: 24/7)",
                EmergencyHotline = "161 / (02) 8646 0427",
                SmsRemindersEnabled = true,
                EmailRemindersEnabled = true,
                ConflictAlertsEnabled = true,
                HighRiskFlagsEnabled = true,
                LowStockAlertsEnabled = true,
                DailyDigestEnabled = false
            };
            await context.ClinicSettings.AddAsync(clinicSetting);
            await context.SaveChangesAsync();
        }

        // 8. Seed Initial Activity Logs if none exist
        if (!await context.ActivityLogs.AnyAsync())
        {
            var now = DateTime.Now;
            var logs = new List<ActivityLog>
            {
                new()
                {
                    Timestamp = now.AddHours(-26),
                    Category = "Authentication",
                    Action = "Staff Login",
                    PerformedBy = "Dr. Leyo Mendoza",
                    Details = "Administrator session initiated from local workstation.",
                    Severity = "Success"
                },
                new()
                {
                    Timestamp = now.AddHours(-24),
                    Category = "Patients",
                    Action = "New Patient Enrolled",
                    PerformedBy = "Juan Santos",
                    Details = "Registered patient record for Maria Santos (Prenatal Case #2026-0104).",
                    Severity = "Info"
                },
                new()
                {
                    Timestamp = now.AddHours(-21),
                    Category = "Appointments",
                    Action = "Appointment Scheduled",
                    PerformedBy = "Dr. Leyo Mendoza",
                    Details = "Scheduled routine prenatal checkup for Rosa Reyes at 10:30 AM.",
                    Severity = "Info"
                },
                new()
                {
                    Timestamp = now.AddHours(-18),
                    Category = "Inventory",
                    Action = "Stock Level Alert",
                    PerformedBy = "System Automated Monitor",
                    Details = "Delivery Kit stock reached critical level (0 sets remaining). Immediate restock requested.",
                    Severity = "Danger"
                },
                new()
                {
                    Timestamp = now.AddHours(-12),
                    Category = "Inventory",
                    Action = "Stock Replenishment",
                    PerformedBy = "Juan Santos",
                    Details = "Received 45 bags of IV Fluids from PhilMed Supplies Inc.",
                    Severity = "Success"
                },
                new()
                {
                    Timestamp = now.AddHours(-5),
                    Category = "Settings",
                    Action = "User Account Verified",
                    PerformedBy = "Dr. Leyo Mendoza",
                    Details = "Verified and activated staff credentials for Maria Reyes.",
                    Severity = "Info"
                },
                new()
                {
                    Timestamp = now.AddMinutes(-35),
                    Category = "Appointments",
                    Action = "Appointment Status Updated",
                    PerformedBy = "Dr. Leyo Mendoza",
                    Details = "Marked appointment for Maria Santos as confirmed.",
                    Severity = "Success"
                }
            };
            await context.ActivityLogs.AddRangeAsync(logs);
            await context.SaveChangesAsync();
        }
    }
}
