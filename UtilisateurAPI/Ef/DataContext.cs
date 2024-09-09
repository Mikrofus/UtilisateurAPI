using Microsoft.EntityFrameworkCore;
using UtilisateurAPI.Ef.Data;

namespace UtilisateurAPI.Ef
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }
        public DbSet<Utilisateur> Utilisateurs { get; set; }

    }
}
