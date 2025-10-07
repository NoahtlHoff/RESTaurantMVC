using System.ComponentModel.DataAnnotations;

namespace RESTaurantMVC.Models
{
    public class TableVM
    {
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Number { get; set; } = string.Empty;

        [Range(1, 100, ErrorMessage = "Kapacitet måste vara mellan 1 och 100")]
        public int Capacity { get; set; }
    }
}
