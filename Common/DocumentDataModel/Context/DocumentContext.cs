using Common.ResultsModel;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Common.DataModels
{
    public class DocumentContext : DbContext
    {
        public DbSet<Document> Documents { get; set; }
        public DbSet<Comparison> Comparisons { get; set; }
        public DbSet<CommonPassage> Passages { get; set; }

        public DocumentContext()
        : base("Server=188.4.221.59;Initial Catalog=Documents;Persist Security Info=False;User ID=sa;Password=Ceid@5202;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=3000;")
        {
            var objectContext = (this as IObjectContextAdapter).ObjectContext;
            objectContext.CommandTimeout = 600;
            Database.SetInitializer(new CreateDatabaseIfNotExists<DocumentContext>());
        }
    }
}