using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PaperTrail.Data;
using PaperTrail.Models;
using PaperTrail.ViewModels;

namespace PaperTrail.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext context, 
            IWebHostEnvironment webHostEnvironment,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.UserCount = await _context.Users.CountAsync();
            ViewBag.BookCount = await _context.Books.CountAsync();
            ViewBag.OrderCount = await _context.Orders.CountAsync();
            
            var successfulStatuses = new[] { OrderStatus.Paid, OrderStatus.Shipped, OrderStatus.Completed };
            ViewBag.TotalSales = await _context.Orders
                .Where(o => successfulStatuses.Contains(o.Status))
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            return View();
        }

        #region User Management
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var userRoles = new Dictionary<string, IList<string>>();
            foreach (var user in users)
            {
                userRoles[user.Id] = await _userManager.GetRolesAsync(user);
            }
            ViewBag.UserRoles = userRoles;
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var roles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
            var userRoles = await _userManager.GetRolesAsync(user);

            var model = new EditUserViewModel
            {
                Id = user.Id,
                Email = user.Email!,
                FullName = user.FullName ?? "",
                SelectedRoles = userRoles.ToList(),
                AllRoles = roles
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null) return NotFound();

                user.FullName = model.FullName;
                user.Email = model.Email;
                user.UserName = model.Email;

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    var rolesToAdd = model.SelectedRoles.Except(currentRoles);
                    var rolesToRemove = currentRoles.Except(model.SelectedRoles);

                    await _userManager.AddToRolesAsync(user, rolesToAdd);
                    await _userManager.RemoveFromRolesAsync(user, rolesToRemove);

                    return RedirectToAction(nameof(Users));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            model.AllRoles = await _roleManager.Roles.Select(r => r.Name!).ToListAsync();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                // Prevent deleting self
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser?.Id == user.Id)
                {
                    TempData["Error"] = "You cannot delete your own admin account.";
                    return RedirectToAction(nameof(Users));
                }

                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction(nameof(Users));
        }
        #endregion

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

        #region Transaction Management
        public async Task<IActionResult> Transactions()
        {
            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
            return View(orders);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int id, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                order.Status = status;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Transactions));
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
