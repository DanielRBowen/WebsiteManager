using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebsiteManager.Models;

namespace WebsiteManager.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<InstanceConfiguration> InstanceConfigurations { get; set; }

        //public DbSet<Media> Medias { get; set; }

        //public DbSet<InstanceConfigurationMedia> InstanceConfigurationMedia { get; set; }

        //public DbSet<UserInstanceConfiguration> UserInstanceConfigurations { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);

        //    var instanceConfigurationMedia = modelBuilder.Entity<InstanceConfigurationMedia>();
        //    instanceConfigurationMedia.HasKey(instanceConfigurationMedia0 => new { instanceConfigurationMedia0.InstanceConfigurationId, instanceConfigurationMedia0.MediaId });

        //    instanceConfigurationMedia.HasOne(instanceConfigurationMedia0 => instanceConfigurationMedia0.InstanceConfiguration)
        //        .WithMany(instanceConfiguration => instanceConfiguration.InstanceConfigurationMedia)
        //        .HasForeignKey(instanceConfigurationMedia0 => instanceConfigurationMedia0.InstanceConfigurationId);

        //    instanceConfigurationMedia.HasOne(instanceConfigurationMedia0 => instanceConfigurationMedia0.Media)
        //        .WithMany()
        //        .HasForeignKey(instanceConfigurationMedia0 => instanceConfigurationMedia0.MediaId);

        //    var userInstanceConfiguration = modelBuilder.Entity<UserInstanceConfiguration>();
        //    userInstanceConfiguration.HasKey(userInstanceConfiguration0 => new { userInstanceConfiguration0.UserId, userInstanceConfiguration0.InstanceConfigurationId });


        //    userInstanceConfiguration.HasOne(userInstanceConfiguration0 => userInstanceConfiguration0.User)
        //        .WithMany(user => user.UserInstanceConfigurations)
        //        .HasForeignKey(userInstanceConfiguration0 => userInstanceConfiguration0.InstanceConfigurationId);

        //    userInstanceConfiguration.HasOne(userInstanceConfiguration0 => userInstanceConfiguration0.InstanceConfiguration)
        //        .WithMany()
        //        .HasForeignKey(userInstanceConfiguration0 => userInstanceConfiguration0.UserId);

        //    return;
        //}
    }

}
