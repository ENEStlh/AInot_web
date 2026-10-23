using AiNotes.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AiNotes.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<NoteTag> NoteTags => Set<NoteTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => user.Id);
            entity.HasIndex(user => user.Email).IsUnique();
            entity.Property(user => user.Email).HasMaxLength(256).IsRequired();
            entity.Property(user => user.PasswordHash).IsRequired();
        });

        modelBuilder.Entity<Note>(entity =>
        {
            entity.HasKey(note => note.Id);
            entity.Property(note => note.Title).HasMaxLength(200).IsRequired();
            entity.Property(note => note.Content).HasMaxLength(50000).IsRequired();
            entity.Property(note => note.Summary).HasMaxLength(10000);
            entity.HasOne(note => note.User)
                .WithMany(user => user.Notes)
                .HasForeignKey(note => note.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(tag => tag.Id);
            entity.HasIndex(tag => tag.Name).IsUnique();
            entity.Property(tag => tag.Name).HasMaxLength(100).IsRequired();
        });

        modelBuilder.Entity<NoteTag>(entity =>
        {
            entity.HasKey(noteTag => new { noteTag.NoteId, noteTag.TagId });
            entity.HasOne(noteTag => noteTag.Note)
                .WithMany(note => note.NoteTags)
                .HasForeignKey(noteTag => noteTag.NoteId);
            entity.HasOne(noteTag => noteTag.Tag)
                .WithMany(tag => tag.NoteTags)
                .HasForeignKey(noteTag => noteTag.TagId);
        });
    }
}