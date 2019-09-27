using Microsoft.AspNetCore.Identity;

namespace idsrv4testaspid.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser	
    {
	    public int TenantId { get; set; }
    }
}
