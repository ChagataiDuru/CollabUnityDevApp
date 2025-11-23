using Microsoft.EntityFrameworkCore;
using UnityDevHub.API.Data.Entities;

namespace UnityDevHub.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<TaskColumn> TaskColumns { get; set; }
    public DbSet<ProjectTask> Tasks { get; set; }
    public DbSet<TaskTag> TaskTags { get; set; }
    public DbSet<TaskComment> TaskComments { get; set; }
    public DbSet<TaskAttachment> TaskAttachments { get; set; }
    public DbSet<ChecklistItem> ChecklistItems { get; set; }
    public DbSet<Whiteboard> Whiteboards { get; set; }
    public DbSet<UnityDocReference> UnityDocReferences { get; set; }
    public DbSet<SearchHistory> SearchHistory { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<ProjectMember> ProjectMembers { get; set; }
    public DbSet<Repository> Repositories { get; set; }
    public DbSet<Commit> Commits { get; set; }
    public DbSet<Build> Builds { get; set; }
    public DbSet<Sprint> Sprints { get; set; }
    public DbSet<TimeLog> TimeLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User - RefreshToken (One-to-Many)
        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Project - User (CreatedBy)
        modelBuilder.Entity<Project>()
            .HasOne(p => p.CreatedBy)
            .WithMany()
            .HasForeignKey(p => p.CreatedById);

        // TaskColumn - Project
        modelBuilder.Entity<TaskColumn>()
            .HasOne(tc => tc.Project)
            .WithMany()
            .HasForeignKey(tc => tc.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // ProjectTask - Project
        modelBuilder.Entity<ProjectTask>()
            .HasOne(t => t.Project)
            .WithMany()
            .HasForeignKey(t => t.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // ProjectTask - TaskColumn
        modelBuilder.Entity<ProjectTask>()
            .HasOne(t => t.Column)
            .WithMany()
            .HasForeignKey(t => t.ColumnId)
            .OnDelete(DeleteBehavior.Cascade);

        // ProjectTask - User (AssignedTo)
        modelBuilder.Entity<ProjectTask>()
            .HasOne(t => t.AssignedTo)
            .WithMany()
            .HasForeignKey(t => t.AssignedToId)
            .OnDelete(DeleteBehavior.SetNull);

        // TaskTag - ProjectTask
        modelBuilder.Entity<TaskTag>()
            .HasOne(tt => tt.Task)
            .WithMany(t => t.Tags)
            .HasForeignKey(tt => tt.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        // TaskComment - ProjectTask
        modelBuilder.Entity<TaskComment>()
            .HasOne(tc => tc.Task)
            .WithMany(t => t.Comments)
            .HasForeignKey(tc => tc.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        // TaskAttachment - ProjectTask
        modelBuilder.Entity<TaskAttachment>()
            .HasOne(ta => ta.Task)
            .WithMany(t => t.Attachments)
            .HasForeignKey(ta => ta.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        // ChecklistItem - ProjectTask
        modelBuilder.Entity<ChecklistItem>()
            .HasOne(ci => ci.Task)
            .WithMany(t => t.ChecklistItems)
            .HasForeignKey(ci => ci.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        // Whiteboard - Project
        modelBuilder.Entity<Whiteboard>()
            .HasOne(w => w.Project)
            .WithMany()
            .HasForeignKey(w => w.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // UnityDocReference - Project
        modelBuilder.Entity<UnityDocReference>()
            .HasOne(udr => udr.Project)
            .WithMany()
            .HasForeignKey(udr => udr.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // SearchHistory - User
        modelBuilder.Entity<SearchHistory>()
            .HasOne(sh => sh.User)
            .WithMany()
            .HasForeignKey(sh => sh.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ProjectMember - Project
        modelBuilder.Entity<ProjectMember>()
            .HasOne(pm => pm.Project)
            .WithMany()
            .HasForeignKey(pm => pm.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // ProjectMember - User
        modelBuilder.Entity<ProjectMember>()
            .HasOne(pm => pm.User)
            .WithMany()
            .HasForeignKey(pm => pm.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Repository - Project
        modelBuilder.Entity<Repository>()
            .HasOne(r => r.Project)
            .WithMany(p => p.Repositories)
            .HasForeignKey(r => r.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Commit - Repository
        modelBuilder.Entity<Commit>()
            .HasOne(c => c.Repository)
            .WithMany(r => r.Commits)
            .HasForeignKey(c => c.RepositoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Commit - ProjectTask
        modelBuilder.Entity<Commit>()
            .HasOne(c => c.Task)
            .WithMany(t => t.Commits)
            .HasForeignKey(c => c.TaskId)
            .OnDelete(DeleteBehavior.SetNull);

        // Build - Project
        modelBuilder.Entity<Build>()
            .HasOne(b => b.Project)
            .WithMany(p => p.Builds)
            .HasForeignKey(b => b.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Build - Repository
        modelBuilder.Entity<Build>()
            .HasOne(b => b.Repository)
            .WithMany(r => r.Builds)
            .HasForeignKey(b => b.RepositoryId)
            .OnDelete(DeleteBehavior.NoAction); // Avoid cycles or multiple cascade paths

        // Sprint - Project
        modelBuilder.Entity<Sprint>()
            .HasOne(s => s.Project)
            .WithMany(p => p.Sprints)
            .HasForeignKey(s => s.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Sprint - Tasks
        modelBuilder.Entity<ProjectTask>()
            .HasOne(t => t.Sprint)
            .WithMany(s => s.Tasks)
            .HasForeignKey(t => t.SprintId)
            .OnDelete(DeleteBehavior.SetNull);

        // TimeLog - Task
        modelBuilder.Entity<TimeLog>()
            .HasOne(tl => tl.Task)
            .WithMany(t => t.TimeLogs)
            .HasForeignKey(tl => tl.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        // TimeLog - User
        modelBuilder.Entity<TimeLog>()
            .HasOne(tl => tl.User)
            .WithMany()
            .HasForeignKey(tl => tl.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
