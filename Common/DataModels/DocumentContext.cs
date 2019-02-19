using System.Data.Entity;

namespace Common.DataModels
{
    public class DocumentContext : DbContext
    {
        public DbSet<Document> Documents { get; set; }
        public DbSet<ProfileStopWord> Profiles { get; set; }
        public DbSet<StopNGram> StopNGrams { get; set; }
        public DbSet<Word> Word { get; set; }

		//ALLAGI MPIL 2
        public DocumentContext()
        : base("Server=62.1.231.194;Initial Catalog=Documents;Persist Security Info=False;User ID=sa;Password=Ceid@5202;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=3000;")
        {
            System.Data.Entity.Database.SetInitializer(new CreateDatabaseIfNotExists<DocumentContext>());
        }
    }
}