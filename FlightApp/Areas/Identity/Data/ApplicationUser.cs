using Microsoft.AspNetCore.Identity;

namespace FlightApp.Areas.Identity.Data
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        public string FirstName { get; set; }
        [PersonalData]
        public string LastName { get; set; }
        [PersonalData]
        public DateOnly DateOfBirth { get; set; }
}
}
