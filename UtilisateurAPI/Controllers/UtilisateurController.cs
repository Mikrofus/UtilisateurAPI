using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UtilisateurAPI.Controllers.Dto;
using UtilisateurAPI.Ef;
using UtilisateurAPI.Ef.Data;

namespace UtilisateurAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UtilisateurController : ControllerBase
    {
        private readonly DataContext _dataContext;
        public UtilisateurController(DataContext context)
        {
            _dataContext = context;
        }

        // a. Récupérer une liste de personnes avec des filtres optionnels sur nom et prénom
        [HttpGet]
        public async Task<ActionResult<List<Utilisateur>>> GetAllUtilisateursWithFilters([FromQuery] string prenom = "", [FromQuery] string nom = "")
        {
            var query = _dataContext.Utilisateurs.AsQueryable();

            //Filtre sur le prenom
            if (!string.IsNullOrEmpty(prenom))
            {
                var prenomPattern = $"%{prenom}%";
                query = query.Where(u => EF.Functions.Like(u.Prenom, prenomPattern));
            }
            
            //Filtre sur le nom
            if (!string.IsNullOrEmpty(nom))
            {
                var nomPattern = $"%{nom}%";
                query = query.Where(u => EF.Functions.Like(u.Nom, nomPattern));
            }

            var utilisateurs = await query.ToListAsync();
            return Ok(utilisateurs);
        }


        // b. Récupérer une personne par son ID
        [HttpGet("{id}")]
        public async Task<ActionResult<Utilisateur>> GetUtilisateurById(Guid id)
        {
            var utilisateur = await _dataContext.Utilisateurs.FindAsync(id);
            if (utilisateur == null)
            {
                return NotFound("Utilisateur non trouvé");
            }
            return Ok(utilisateur);
        }


        // c. Ajouter une nouvelle personne
        [HttpPost]
        public async Task<ActionResult<Utilisateur>> AddUtilisateur(DtoCreateUtilisateur utilisateur )
        {
            if (string.IsNullOrEmpty(utilisateur.Nom) || string.IsNullOrEmpty(utilisateur.Prenom))
            {
                return BadRequest("Le nom et le prénom sont obligatoires");
            }

            Utilisateur utilisateurDb = new Utilisateur();
            utilisateurDb.Prenom = utilisateur.Prenom;
            utilisateurDb.Nom = utilisateur.Nom;
            

            _dataContext.Utilisateurs.Add(utilisateurDb);
            await _dataContext.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUtilisateurById), new { id = utilisateurDb.Id }, utilisateurDb);
        }

        // d. Modifier une personne par son ID
        [HttpPut("{id}")]
        public async Task<ActionResult<Utilisateur>> UpdateUtilisateur(Guid id, DtoUpdateUtilisateur dtoUpdateUtilisateur)
        {
            
            var utilisateur = await _dataContext.Utilisateurs.FindAsync(id);

            if (utilisateur == null)
            {
                return NotFound("Utilisateur non trouvé");
            }

           
            if (!string.IsNullOrEmpty(dtoUpdateUtilisateur.Prenom))
            {
                utilisateur.Prenom = dtoUpdateUtilisateur.Prenom;
            }

            if (!string.IsNullOrEmpty(dtoUpdateUtilisateur.Nom))
            {
                utilisateur.Nom = dtoUpdateUtilisateur.Nom;
            }

           
            _dataContext.Utilisateurs.Update(utilisateur);
            await _dataContext.SaveChangesAsync();

            return Ok(utilisateur);
        }


        // e. Supprimer une personne par son ID
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUtilisateur(Guid id)
        {
            var utilisateur = await _dataContext.Utilisateurs.FindAsync(id);
            if (utilisateur == null)
            {
                return NotFound("Utilisateur non trouvé");
            }

            _dataContext.Utilisateurs.Remove(utilisateur);
            await _dataContext.SaveChangesAsync();

            return Ok("Utilisateur supprimé avec succès");
        }
    }
}
