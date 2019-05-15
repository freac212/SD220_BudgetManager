using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using SD220_Deliverable_1_DGrouette.Models.Domain;

namespace SD220_Deliverable_1_DGrouette.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }
        [InverseProperty(nameof(Household.Creator))]
        public virtual List<Household> CreatedHouseholds { get; set; }

        [InverseProperty(nameof(Household.Users))]
        public virtual List<Household> Households { get; set; }

        [InverseProperty(nameof(Household.InvitedUsers))]
        public virtual List<Household> HouseholdsInvitedTo { get; set; }

        public ApplicationUser()
        {
            Households = new List<Household>();
            CreatedHouseholds = new List<Household>();
            HouseholdsInvitedTo = new List<Household>();
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public DbSet<Household> Households { get; set; }
        public DbSet<Category> Categories { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }
}