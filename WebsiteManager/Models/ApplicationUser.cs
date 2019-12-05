using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebsiteManager.Models
{
    public class ApplicationUser : IdentityUser
    {
        //https://docs.microsoft.com/en-us/aspnet/core/security/authentication/customize-identity-model?view=aspnetcore-3.0#change-the-primary-key-type
        public virtual ICollection<InstanceConfiguration> InstanceConfigurations { get; set; }
    }
}
