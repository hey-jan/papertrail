using System.ComponentModel.DataAnnotations;

namespace PaperTrail.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public virtual ICollection<Book>? Books { get; set; }
    }

    public class Book
    {
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Author { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [Range(0.01, 10000.00)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, 1000000)]
        public int Stock { get; set; }

        public string? ImageUrl { get; set; }

        [Range(0, 5)]
        public double Rating { get; set; } = 0;

        [Required]
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
