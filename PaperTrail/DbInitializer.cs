using Microsoft.AspNetCore.Identity;
using PaperTrail.Data;
using PaperTrail.Models;

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

            if (context.Categories.Any()) return;

            var categories = new Category[]
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

            // Seed Books
            if (!context.Books.Any())
            {
                var fictionId = categories.First(c => c.Name == "Fiction").Id;
                var scifiId = categories.First(c => c.Name == "Sci-Fi").Id;
                var mysteryId = categories.First(c => c.Name == "Mystery").Id;

                context.Books.AddRange(
                    new Book { Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", Price = 15.99m, Stock = 50, CategoryId = fictionId, Description = "A classic of American literature." },
                    new Book { Title = "1984", Author = "George Orwell", Price = 12.50m, Stock = 30, CategoryId = scifiId, Description = "Dystopian masterpiece." },
                    new Book { Title = "The Hobbit", Author = "J.R.R. Tolkien", Price = 20.00m, Stock = 25, CategoryId = fictionId, Description = "An epic fantasy adventure." },
                    new Book { Title = "Sherlock Holmes", Author = "Arthur Conan Doyle", Price = 18.00m, Stock = 40, CategoryId = mysteryId, Description = "The world's most famous detective." }
                );
                context.SaveChanges();
            }
        }
    }
}
