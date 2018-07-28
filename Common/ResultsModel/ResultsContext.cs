using System.Data.Entity;

namespace Common.ResultsModel
{
    public class ResultsContext : DbContext
    {
        public DbSet<Comparison> Comparisons { get; set; }
        public DbSet<CommonPassage> Passages { get; set; }

        public ResultsContext()
        : base("Server=172.26.179.97;Initial Catalog=ResultsDatabase;Persist Security Info=False;User ID=sa;Password=Ceid@5202;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Connection Timeout=3000;")
        {
            System.Data.Entity.Database.SetInitializer(new CreateDatabaseIfNotExists<ResultsContext>());
        }
    }
}