namespace EventSpaceBookingSystem.Model
{
    public class Booking
    {
        public string BookingId { get; set; }  // ✅ Unique identifier

        public string Image { get; set; }
        public string Headline { get; set; }
        public string PublishedDate { get; set; }
        public string Status { get; set; }
        public DateTime BookingDate { get; set; }
        public int GuestCount { get; set; }
        public string EventTime { get; set; }
        public string ExtraChairs { get; set; }
        public string TimeExtension { get; set; }
        public decimal Price { get; set; }
        public string Category { get; set; }
        public int Rating { get; set; } // ⭐ New field

        public int OwnerUsedId { get; set; }  // Identifier for Booking Info Page
    }
}
