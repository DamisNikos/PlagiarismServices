//using System.Data.Entity;

//namespace Common.DataModels
//{
//    public class OldDocumentContext : DbContext
//    {
//        public DbSet<OldDocument> Documents { get; set; }
//        public DbSet<OldProfileStopWord> Profiles { get; set; }
//        public DbSet<OldStopNGram> StopNGrams { get; set; }
//        public DbSet<OldWord> Word { get; set; }

//        public OldDocumentContext()
//        : base("Server=172.26.190.254;Initial Catalog=DocumentDatabase;Persist Security Info=False;User ID=sa;Password=Ceid@5202;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=3000;")
//        {
//            System.Data.Entity.Database.SetInitializer(new CreateDatabaseIfNotExists<OldDocumentContext>());
//        }
//    }
//}