using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RESTaurantMVC.Models
{
    public class MenuItem
    {
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string Name { get; set; } = "";

        [Range(0, 1000000, ErrorMessage = "Pris måste vara mellan 0 och 1000000")]
        public decimal Price { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public bool IsPopular { get; set; }

        [Url(ErrorMessage = "Ange en giltig URL")]
        [StringLength(500)]
        public string? ImageUrl { get; set; }
    }
}
