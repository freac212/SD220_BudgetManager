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

        [InverseProperty(nameof(Household.Members))]
        public virtual List<Household> HouseholdMembers { get; set; }

        [InverseProperty(nameof(Household.InvitedUsers))]
        public virtual List<Household> HouseholdsInvitedTo { get; set; }
        public virtual List<Transaction> Transactions { get; set; }

        public ApplicationUser()
        {
            HouseholdMembers = new List<Household>();
            CreatedHouseholds = new List<Household>();
            HouseholdsInvitedTo = new List<Household>();
            Transactions = new List<Transaction>();
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
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ensures on delete of a transaction, it doesn't create an inf loop of deleting. 
            // However, it's required that all transactions of a category are deleted before the category can be deleted.
            modelBuilder.Entity<Transaction>()
                .HasRequired(p => p.Category)
                .WithMany(p => p.Transactions)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(p => p.HouseholdMembers)
                .WithMany(p => p.Members)
                .Map(p => p.ToTable("HouseholdMembers"));

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(p => p.HouseholdsInvitedTo)
                .WithMany(p => p.InvitedUsers)
                .Map(p => p.ToTable("HouseholdInvitedUsers"));

        }
    }
}