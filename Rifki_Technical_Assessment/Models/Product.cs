using System.ComponentModel.DataAnnotations;

namespace Rifki_Technical_Assessment.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters.")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Creation date is required.")]
        public DateTime CreatedAt { get; set; }
    }
}
