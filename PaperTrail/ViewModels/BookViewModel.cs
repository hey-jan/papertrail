using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace PaperTrail.ViewModels
{
    public class BookViewModel
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

        [Display(Name = "Book Cover Image")]
        public IFormFile? ImageFile { get; set; }

        public string? ImageUrl { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        public List<SelectListItem>? Categories { get; set; }
    }
}
