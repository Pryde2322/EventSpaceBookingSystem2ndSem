namespace EventSpaceBookingSystem.Model
{
    public class Users
    {
        
        public string Email { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public string ConfirmPassword { get; set; }
        public string Status { get; set; }

        public bool IsTermsAccepted { get; set; }

        public int Id { get; set; } 

        public string RaMenCoStatus { get; set; } // "Activated" or "Deactivated"
    }
}
