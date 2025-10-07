namespace RESTaurantMVC.Models
{
    public class BookingVM
    {
        public int Id { get; set; }
        public string? GuestName { get; set; }
        public string? Date { get; set; }
        public string? Time { get; set; }
        public int PartySize { get; set; }
        public string? TableName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
    }
}
