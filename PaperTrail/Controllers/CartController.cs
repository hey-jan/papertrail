using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PaperTrail.Data;
using PaperTrail.Extensions;
using PaperTrail.Models;
using System.Security.Claims;

namespace PaperTrail.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private const string CartSessionKey = "Cart";

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            ViewBag.Total = cart.Sum(i => i.Total);
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int id, int quantity = 1)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null) return NotFound();

            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey) ?? new List<CartItem>();
            var item = cart.FirstOrDefault(i => i.BookId == id);

            if (item != null)
            {
                item.Quantity += quantity;
            }
            else
            {
                cart.Add(new CartItem
                {
                    BookId = book.Id,
                    Title = book.Title,
                    Price = book.Price,
                    ImageUrl = book.ImageUrl,
                    Quantity = quantity
                });
            }

            HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Remove(int id)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey);
            if (cart != null)
            {
                var item = cart.FirstOrDefault(i => i.BookId == id);
                if (item != null)
                {
                    cart.Remove(item);
                    HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey);
            if (cart != null)
            {
                var item = cart.FirstOrDefault(i => i.BookId == id);
                if (item != null && quantity > 0)
                {
                    item.Quantity = quantity;
                    HttpContext.Session.SetObjectAsJson(CartSessionKey, cart);
                }
            }
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpGet]
        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey);
            if (cart == null || !cart.Any()) return RedirectToAction(nameof(Index));

            ViewBag.Total = cart.Sum(i => i.Total);
            return View(new Order { TotalAmount = cart.Sum(i => i.Total) });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(Order order)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>(CartSessionKey);
            if (cart == null || !cart.Any()) return RedirectToAction(nameof(Index));

            if (ModelState.IsValid)
            {
                order.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
                order.OrderDate = DateTime.UtcNow;
                order.Status = OrderStatus.Pending;
                order.TotalAmount = cart.Sum(i => i.Total);

                foreach (var item in cart)
                {
                    var book = await _context.Books.FindAsync(item.BookId);
                    if (book == null || book.Stock < item.Quantity)
                    {
                        ModelState.AddModelError(string.Empty, $"Insufficient stock for {item.Title}. Available: {(book?.Stock ?? 0)}");
                        ViewBag.Total = cart.Sum(i => i.Total);
                        return View(order);
                    }

                    order.OrderItems.Add(new OrderItem
                    {
                        BookId = item.BookId,
                        Quantity = item.Quantity,
                        Price = item.Price
                    });

                    // Update Stock
                    book.Stock -= item.Quantity;
                    _context.Update(book);
                }

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // Clear Cart
                HttpContext.Session.Remove(CartSessionKey);

                return RedirectToAction(nameof(Success), new { id = order.Id });
            }

            ViewBag.Total = cart.Sum(i => i.Total);
            return View(order);
        }

        public IActionResult Success(int id)
        {
            ViewBag.OrderId = id;
            return View();
        }
    }
}
