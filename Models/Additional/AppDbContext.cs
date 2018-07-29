using HelpDesk.Models.Articles;
using HelpDesk.Models.Clients;
using HelpDesk.Models.Common;
using HelpDesk.Models.Notifications;
using HelpDesk.Models.SLAs;
using HelpDesk.Models.Task;
using HelpDesk.Models.Tickets;
using HelpDesk.Models.Users;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HelpDesk.Models.Additional
{
    public class AppDbContext : DbContext
    {
        /***   Arcticles   ***/
        public DbSet<Article> Articles { get; set; }
        public DbSet<ArticleAttachment> ArticleAttachments { get; set; }
        public DbSet<ArticleSection> ArticleSections { get; set; }
        public DbSet<ArticleType> ArticleTypes { get; set; }
        /***   Arcticles   ***/

        /***    Clients    ***/
        public DbSet<Client> Clients { get; set; }
        public DbSet<Company> Companies { get; set; }
        /***    Clients    ***/

        /***     Common    ***/
        public DbSet<Departament> Departaments { get; set; }
        public DbSet<Objective> Objectives { get; set; }
        public DbSet<Priority> Priorities { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<RegistrationToken> RegistrationTokens { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        /***     Common    ***/

        /*** Notifications ***/
        public DbSet<Event> Events { get; set; }
        public DbSet<EventType> EventTypes { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        /*** Notifications ***/

        /***      SLAs     ***/
        public DbSet<SLA> SLAs { get; set; }
        public DbSet<SLAAllowedDepartament> SLAAllowedDepartaments { get; set; }
        public DbSet<SLAAllowedObjective> SLAAllowedObjectives { get; set; }
        public DbSet<SLAAllowedPriority> SLAAllowedPriorities { get; set; }
        public DbSet<SLAAllowedType> SLAAllowedTypes { get; set; }
        public DbSet<SLATime> SLATime { get; set; }
        /***      SLAs     ***/

        /***     Tasks     ***/
        public DbSet<TicketTask> TicketTasks { get; set; }
        public DbSet<TaskComment> TaskComments { get; set; }
        public DbSet<TicketTaskStatus> TaskStatuses { get; set; }
        /***     Tasks     ***/

        /***    Tickets    ***/
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<TicketAttachment> TicketAttachments { get; set; }
        public DbSet<TicketComment> TicketComments { get; set; }
        public DbSet<TicketStatus> TicketStatuses { get; set; }
        public DbSet<TicketType> TicketTypes { get; set; }
        /***    Tickets    ***/

        /***     Users     ***/
        public DbSet<User> Users { get; set; }
        public DbSet<UserStatus> UserStatuses { get; set; }
        /***     Users     ***/

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Notification>().HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Notification>().HasOne(x => x.Client).WithMany().HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>().HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Ticket>().HasOne(x => x.Client).WithMany().HasForeignKey(x => x.ClientId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Ticket>().HasOne(x => x.Departament).WithMany().HasForeignKey(x => x.DepartamentId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Ticket>().HasOne(x => x.Objective).WithMany().HasForeignKey(x => x.ObjectiveId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Ticket>().HasOne(x => x.Priority).WithMany().HasForeignKey(x => x.PriorityId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Ticket>().HasOne(x => x.Status).WithMany().HasForeignKey(x => x.StatusId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Ticket>().HasOne(x => x.Type).WithMany().HasForeignKey(x => x.TypeId).OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>().Property(x => x.StatusId).HasDefaultValue(6);
            modelBuilder.Entity<Ticket>().Property(x => x.DateCreated).HasDefaultValueSql("getdate()");
            modelBuilder.Entity<Ticket>().Property(x => x.DateClosed).HasDefaultValue(null);
            modelBuilder.Entity<User>().Property(x => x.StatusId).HasDefaultValue(1);
            modelBuilder.Entity<TicketTask>().Property(x => x.StatusId).HasDefaultValue(1);
            modelBuilder.Entity<Event>().Property(x => x.Date).HasDefaultValueSql("getdate()");
            modelBuilder.Entity<Notification>().Property(x => x.UseEmail).HasDefaultValue(true);

            modelBuilder.Entity<SLAAllowedPriority>(entity =>
            {
                entity.Property(x => x.Unique).IsRequired();
                entity.HasIndex(x => x.Unique).IsUnique();
            });
            modelBuilder.Entity<SLAAllowedDepartament>(entity =>
            {
                entity.Property(x => x.Unique).IsRequired();
                entity.HasIndex(x => x.Unique).IsUnique();
            });
            modelBuilder.Entity<SLAAllowedObjective>(entity =>
            {
                entity.Property(x => x.Unique).IsRequired();
                entity.HasIndex(x => x.Unique).IsUnique();
            });
            modelBuilder.Entity<SLAAllowedType>(entity =>
            {
                entity.Property(x => x.Unique).IsRequired();
                entity.HasIndex(x => x.Unique).IsUnique();
            });
            modelBuilder.Entity<SLATime>(entity =>
            {
                entity.Property(x => x.Unique).IsRequired();
                entity.HasIndex(x => x.Unique).IsUnique();
            });

            modelBuilder.Entity<TicketComment>().HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<TaskComment>().HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<TicketTask>().HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
        
    }

    public enum EventTypes
    {
        TicketAdded = 1,
        TicketChanged,
        TicketComment,
        TaskAdded,
        TaskChanged,
        TaskCommented
    }
}
