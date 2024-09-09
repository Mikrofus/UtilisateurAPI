using System.ComponentModel.DataAnnotations;

namespace UtilisateurAPI.Controllers.Dto
{
    public class DtoCreateUtilisateur
    {
        [Required(ErrorMessage = "Le prénom est requis")]
        public string Prenom { get; set; }

        [Required(ErrorMessage = "Le nom est requis")]
        public string Nom { get; set; }
    }
}
