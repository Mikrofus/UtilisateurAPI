using System.ComponentModel.DataAnnotations;

namespace UtilisateurAPI.Ef.Data
{
    public class Utilisateur
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Prenom { get; set; }

        public string Nom { get; set; }
    }
}
