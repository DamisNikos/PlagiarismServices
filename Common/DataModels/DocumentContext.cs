using Common.DataModels;
using System.Data.Entity;


namespace Common.DataModels
{
    public class DocumentContext : DbContext
    {
        public DbSet<Document> Documents { get; set; }
        public DbSet<ProfileStopWord> Profiles { get; set; }
        public DbSet<StopNGram> StopNGrams { get; set; }
        public DbSet<Word> Word { get; set; }


        public DocumentContext()
        : base("Data Source= 94.64.96.46;Initial Catalog=DocumentDatabase;Persist Security Info=True;User ID=sa;Password=Ceid@5202;MultipleActiveResultSets=True")
        {
            System.Data.Entity.Database.SetInitializer(new CreateDatabaseIfNotExists<DocumentContext>());
        }

    }
}
