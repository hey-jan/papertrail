using Microsoft.AspNetCore.Identity;
using PaperTrail.Data;
using PaperTrail.Models;
using System.Text.Json;

namespace PaperTrail
{
    public static class DbInitializer
    {
        public static async Task Seed(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            context.Database.EnsureCreated();

            // Seed Roles
            string[] roleNames = { "Admin", "Customer" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Seed Admin User
            string adminEmail = "admin@papertrail.com";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Admin",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            // Seed Categories from JSON or default
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Fiction" },
                    new Category { Name = "Non-Fiction" },
                    new Category { Name = "Academic" },
                    new Category { Name = "Bestsellers" },
                    new Category { Name = "Mystery" },
                    new Category { Name = "Sci-Fi" }
                };
                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            // Seed Books from books.json. Existing titles are preserved; missing books are added.
            try
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "books.json");
                if (File.Exists(filePath))
                {
                    string jsonString = File.ReadAllText(filePath);
                    var booksData = JsonSerializer.Deserialize<List<BookJson>>(jsonString);

                    if (booksData != null)
                    {
                        var categories = context.Categories.ToList();
                        var existingBooks = context.Books.ToList();
                        var retiredSeedBooks = new[]
                        {
                            ("The Seven Husbands of Evelyn Hugo", "Taylor Jenkins Reid")
                        };

                        var booksToRemove = existingBooks
                            .Where(b => retiredSeedBooks.Any(retired =>
                                b.Title == retired.Item1 && b.Author == retired.Item2))
                            .ToList();

                        if (booksToRemove.Count > 0)
                        {
                            context.Books.RemoveRange(booksToRemove);
                            existingBooks = existingBooks.Except(booksToRemove).ToList();
                        }

                        foreach (var bookData in booksData)
                        {
                            var category = categories.FirstOrDefault(c => c.Name == bookData.Category)
                                           ?? categories.First(c => c.Name == "Fiction");

                            var existingBook = existingBooks.FirstOrDefault(b =>
                                (!string.IsNullOrWhiteSpace(bookData.ISBN) && b.ISBN == bookData.ISBN) ||
                                (b.Title == bookData.Title && b.Author == bookData.Author));

                            if (existingBook != null)
                            {
                                existingBook.Description = bookData.Description;
                                existingBook.Price = bookData.Price;
                                existingBook.Stock = bookData.Stock;
                                existingBook.CategoryId = category.Id;
                                existingBook.ImageUrl = bookData.ImageUrl;
                                existingBook.ISBN = bookData.ISBN;
                                existingBook.Publisher = bookData.Publisher;
                                existingBook.PublishedDate = bookData.PublishedDate;
                                existingBook.Rating = bookData.Rating;
                                continue;
                            }

                            var book = new Book
                            {
                                Title = bookData.Title,
                                Author = bookData.Author,
                                Description = bookData.Description,
                                Price = bookData.Price,
                                Stock = bookData.Stock,
                                CategoryId = category.Id,
                                ImageUrl = bookData.ImageUrl,
                                ISBN = bookData.ISBN,
                                Publisher = bookData.Publisher,
                                PublishedDate = bookData.PublishedDate,
                                Rating = bookData.Rating
                            };

                            context.Books.Add(book);
                            existingBooks.Add(book);
                        }

                        context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error or handle gracefully
                Console.WriteLine($"Error seeding books: {ex.Message}");
            }
        }

        private class BookJson
        {
            public string Title { get; set; } = string.Empty;
            public string Author { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public int Stock { get; set; }
            public string Category { get; set; } = string.Empty;
            public string ImageUrl { get; set; } = string.Empty;
            public string? ISBN { get; set; }
            public string? Publisher { get; set; }
            public string? PublishedDate { get; set; }
            public double Rating { get; set; }
        }
    }
}
