using PaperTrail.Data;
using PaperTrail.Models;

namespace PaperTrail
{
    public static class DbInitializer
    {
        public static void Seed(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

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
        }
    }
}
