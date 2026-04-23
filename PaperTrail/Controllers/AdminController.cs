using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PaperTrail.Data;
using PaperTrail.Models;
using PaperTrail.ViewModels;

namespace PaperTrail.Controllers
{
    [Authorize] // Should be [Authorize(Roles = "Admin")] in production
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.UserCount = await _context.Users.CountAsync();
            ViewBag.BookCount = await _context.Books.CountAsync();
            // TODO: Order and Sales analytics
            ViewBag.OrderCount = 0;
            ViewBag.TotalSales = 0;

            return View();
        }

        #region Book Management
        public async Task<IActionResult> Books()
        {
            var books = await _context.Books.Include(b => b.Category).ToListAsync();
            return View(books);
        }

        [HttpGet]
        public async Task<IActionResult> CreateBook()
        {
            var model = new BookViewModel
            {
                Categories = await GetCategorySelectList()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBook(BookViewModel model)
        {
            if (ModelState.IsValid)
            {
                string? imageUrl = null;
                if (model.ImageFile != null)
                {
                    imageUrl = await UploadImage(model.ImageFile);
                }

                var book = new Book
                {
                    Title = model.Title,
                    Author = model.Author,
                    Description = model.Description,
                    Price = model.Price,
                    Stock = model.Stock,
                    CategoryId = model.CategoryId,
                    ImageUrl = imageUrl
                };

                _context.Books.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Books));
            }

            model.Categories = await GetCategorySelectList();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            var model = new BookViewModel
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Description = book.Description,
                Price = book.Price,
                Stock = book.Stock,
                CategoryId = book.CategoryId,
                ImageUrl = book.ImageUrl,
                Categories = await GetCategorySelectList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBook(BookViewModel model)
        {
            if (ModelState.IsValid)
            {
                var book = await _context.Books.FindAsync(model.Id);
                if (book == null) return NotFound();

                book.Title = model.Title;
                book.Author = model.Author;
                book.Description = model.Description;
                book.Price = model.Price;
                book.Stock = model.Stock;
                book.CategoryId = model.CategoryId;

                if (model.ImageFile != null)
                {
                    book.ImageUrl = await UploadImage(model.ImageFile);
                }

                _context.Update(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Books));
            }

            model.Categories = await GetCategorySelectList();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Books));
        }
        #endregion

        #region Category Management
        public async Task<IActionResult> Categories()
        {
            return View(await _context.Categories.ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                _context.Categories.Add(new Category { Name = name });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Categories));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Categories));
        }
        #endregion

        private async Task<List<SelectListItem>> GetCategorySelectList()
        {
            return await _context.Categories
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name })
                .ToListAsync();
        }

        private async Task<string> UploadImage(IFormFile file)
        {
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "books");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return "/uploads/books/" + uniqueFileName;
        }
    }
}
