using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace PaperTrail.Models
{
    public enum OrderStatus
    {
        Pending,
        Paid,
        Shipped,
        Completed,
        Cancelled
    }

    public class Order
    {
        private const string OrderCodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser? User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // Shipping Details
        [Required]
        public string ShippingAddress { get; set; } = string.Empty;
        [Required]
        public string City { get; set; } = string.Empty;
        [Required]
        public string PostalCode { get; set; } = string.Empty;

        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public string OrderNumber => $"{OrderDate:yyMMdd}{GenerateOrderSuffix()}";

        private string GenerateOrderSuffix()
        {
            var seed = $"{Id}|{UserId}|{OrderDate:O}";
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
            var builder = new StringBuilder(8);

            for (int i = 0; i < 8; i++)
            {
                builder.Append(OrderCodeAlphabet[hash[i] % OrderCodeAlphabet.Length]);
            }

            return builder.ToString();
        }
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public virtual Order? Order { get; set; }

        public int BookId { get; set; }
        public virtual Book? Book { get; set; }

        public int Quantity { get; set; }
        public decimal Price { get; set; } // Price at time of purchase
    }

    public class CartItem
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal Total => Price * Quantity;
    }
}
