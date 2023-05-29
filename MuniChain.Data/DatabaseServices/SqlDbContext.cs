using Domain.Models.DealComponents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Shared.Models.AppComponents;
using Shared.Models.Chat;
using Shared.Models.DealComponents;
using Shared.Models.Users;

namespace Data.DatabaseServices
{
    public class SqlDbContext : DbContext
    {
        public SqlDbContext(DbContextOptions<SqlDbContext> options)
           : base(options)
        {
        }

        public DbSet<DealModel> Deals { get; set; }
        public DbSet<DealParticipant> DealParticipants { get; set; }
        public DbSet<Series> Series { get; set; }
        public DbSet<Maturity> Maturity { get; set; }
        public DbSet<Performance> Performance { get; set; }
        public DbSet<TopAccount> TopAccount { get; set; }
        public DbSet<Firm> Firm { get; set; }
        public DbSet<Reference> Reference { get; set; }
        public DbSet<FirmMember> FirmMember { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<BoardMessage> BoardMessages { get; set; }
        public DbSet<Document> Documents { get; set; }
        public DbSet<DocumentAnnotation> Annotations { get; set; }
        public DbSet<AnnotationComment> AnnotationComment { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<DealExpenditure> DealExpenditure { get; set; }
        public DbSet<AppointmentData> AppointmentData { get; set; }
        public DbSet<UserNotificationPreference> UserNotificationPreference { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            var splitStringConverter = new ValueConverter<List<string>, string>(v => string.Join(";", v), v => v.Split(new[] { ';' }).ToList());
            var splitArrayStringConverter = new ValueConverter<string[], string>(v => string.Join(";", v), v => v.Split(new[] { ';' }));
            builder.Entity<User>()
                   .Property(nameof(User.AreasOfExpertise))
                   .HasConversion(splitStringConverter);
            builder.Entity<DealParticipant>()
                   .Property(nameof(DealParticipant.DealPermissions))
                   .HasConversion(splitStringConverter);
            builder.Entity<Document>()
                   .Property(nameof(Document.UserPermissions))
                   .HasConversion(splitStringConverter);
            builder.Entity<DealModel>()
            .Property(a => a.RowVersion)
            .IsRowVersion();
            builder.Entity<DealParticipant>()
            .Property(a => a.RowVersion)
            .IsRowVersion();
            builder.Entity<Document>()
            .Property(a => a.RowVersion)
            .IsRowVersion();
            builder.Entity<Performance>()
            .Property(a => a.RowVersion)
            .IsRowVersion();


            foreach (var foreignKey in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                foreignKey.DeleteBehavior = DeleteBehavior.Cascade;
            }
        }
    }
}
