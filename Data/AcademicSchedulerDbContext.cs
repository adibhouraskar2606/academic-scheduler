using AcademicScheduler.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AcademicScheduler.Data;

public class AcademicSchedulerDbContext : IdentityDbContext<IdentityUser>
{
    public AcademicSchedulerDbContext(
        DbContextOptions<AcademicSchedulerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CompletedCourse> CompletedCourses => Set<CompletedCourse>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Student>()
            .HasIndex(s => s.Email)
            .IsUnique();

        modelBuilder.Entity<Course>()
            .HasIndex(c => c.CourseNumber)
            .IsUnique();

        modelBuilder.Entity<CompletedCourse>()
            .HasIndex(cc => new { cc.StudentId, cc.CourseId })
            .IsUnique();

        modelBuilder.Entity<Enrollment>()
            .HasIndex(e => new { e.StudentId, e.CourseId })
            .IsUnique();
    }
}