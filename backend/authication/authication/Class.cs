namespace authication
{
    // Project: CaravanReservatie
    // Framework: ASP.NET Core 8.0 (Web API) + EF Core

    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.ComponentModel.DataAnnotations;

    // --- ENTITEITEN ---

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "User"; // Admin of User
    }

    public class Reservation
    {
        public int Id { get; set; }
        [Required]
        public string SchoolName { get; set; } = string.Empty;
        [Required]
        public string ContactPerson { get; set; } = string.Empty;
        [Required]
        public DateTime Date { get; set; }
        public string? Note { get; set; }
    }

    public class BlockedDate
    {
        public int Id { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public string? Reason { get; set; }
    }

    // --- DATABASE CONTEXT ---

    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Reservation> Reservations => Set<Reservation>();
        public DbSet<BlockedDate> BlockedDates => Set<BlockedDate>();
    }

    // --- CONTROLLERS ---

    [ApiController]
    [Route("api/reservations")]
    public class ReservationsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReservationsController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _context.Reservations.ToListAsync());

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(Reservation reservation)
        {
            var year = reservation.Date.Year;
            var blocked = await _context.BlockedDates.AnyAsync(b => b.Date.Date == reservation.Date.Date);
            var taken = await _context.Reservations.AnyAsync(r => r.Date.Date == reservation.Date.Date);
            var count = await _context.Reservations.CountAsync(r => r.SchoolName == reservation.SchoolName && r.Date.Year == year);

            if (blocked) return BadRequest("Datum is geblokkeerd.");
            if (taken) return BadRequest("Datum is al gereserveerd.");
            if (count >= 10) return BadRequest("Maximum aantal boekingen bereikt voor dit jaar.");

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            // Hier zou je email kunnen triggeren
            return Ok(reservation);
        }
    }

    [ApiController]
    [Route("api/blocked")]
    public class BlockedDatesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BlockedDatesController(AppDbContext context) => _context = context;

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> BlockDate(BlockedDate date)
        {
            if (await _context.BlockedDates.AnyAsync(d => d.Date.Date == date.Date.Date))
                return Conflict("Datum is al geblokkeerd.");

            _context.BlockedDates.Add(date);
            await _context.SaveChangesAsync();
            return Ok(date);
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll() => Ok(await _context.BlockedDates.ToListAsync());
    }

    // JWT-authenticatie en EmailService moeten nog toegevoegd worden
    // Configureer Startup.cs / Program.cs met services, EF, authenticatie, enz.

}
