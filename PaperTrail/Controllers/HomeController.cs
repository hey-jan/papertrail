using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaperTrail.Data;
using PaperTrail.Models;
using System.Diagnostics;

namespace PaperTrail.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var featuredBooks = await _context.Books
                .Include(b => b.Categories)
                .OrderByDescending(b => b.CreatedAt)
                .Take(4)
                .ToListAsync();
            
            ViewBag.Categories = await _context.Categories.Take(4).ToListAsync();
            
            return View(featuredBooks);
        }

        public async Task<IActionResult> Books(string? category, string? search, string? sort, decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.Books.Include(b => b.Categories).AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(b => b.Title.Contains(search) || b.Author.Contains(search));
            }

            // Filter by Category
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(b => b.Categories!.Any(c => c.Name == category));
            }

            // Filter by Price
            if (minPrice.HasValue) query = query.Where(b => b.Price >= minPrice.Value);
            if (maxPrice.HasValue) query = query.Where(b => b.Price <= maxPrice.Value);

            // Sort
            query = sort switch
            {
                "price_asc" => query.OrderBy(b => b.Price),
                "price_desc" => query.OrderByDescending(b => b.Price),
                "popularity" => query.OrderByDescending(b => b.Rating),
                _ => query.OrderByDescending(b => b.CreatedAt)
            };

            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.CurrentCategory = category;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentSort = sort;

            return View(await query.ToListAsync());
        }

        public async Task<IActionResult> Details(int id)
        {
            var book = await _context.Books
                .Include(b => b.Categories)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null) return NotFound();

            var categoryIds = book.Categories?.Select(c => c.Id).ToList() ?? new List<int>();

            var relatedBooks = await _context.Books
                .Where(b => b.Id != book.Id && b.Categories != null && b.Categories.Any(c => categoryIds.Contains(c.Id)))
                .Take(4)
                .ToListAsync();

            ViewBag.RelatedBooks = relatedBooks;

            return View(book);
        }

        [HttpGet]
        public async Task<IActionResult> SearchSuggestions(string term)
        {
            var suggestions = await _context.Books
                .Where(b => b.Title.Contains(term) || b.Author.Contains(term))
                .Select(b => new { b.Id, b.Title, b.Author })
                .Take(5)
                .ToListAsync();
            return Json(suggestions);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
