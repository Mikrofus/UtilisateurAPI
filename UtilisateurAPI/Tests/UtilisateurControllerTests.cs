using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UtilisateurAPI.Controllers;
using UtilisateurAPI.Controllers.Dto;
using UtilisateurAPI.Ef.Data;
using UtilisateurAPI.Ef;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using System.Diagnostics;

namespace UtilisateurAPI.Tests
{
    public class UtilisateurControllerTests : IDisposable
    {
        private readonly UtilisateurController _controller;
        private readonly DataContext _context;

        public UtilisateurControllerTests()
        {
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new DataContext(options);
            _controller = new UtilisateurController(_context);

            // Seed data
            _context.Utilisateurs.Add(new Utilisateur
            {
                Id = Guid.NewGuid(),
                Prenom = "Valentin",
                Nom = "Dejean"
                
            });
            _context.Utilisateurs.Add(new Utilisateur
            {
                Id = Guid.NewGuid(),
                Prenom = "Guillaume",
                Nom = "Pesetti"
                
            });
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // Teste la récupération d'un utilisateur par ID lorsqu'il est trouvé
        [Fact]
        public async Task GetUtilisateurById_ReturnsUtilisateur_WhenFound()
        {
            // Arrange
            var utilisateurId = _context.Utilisateurs.First().Id;

            // Act
            var result = await _controller.GetUtilisateurById(utilisateurId);



            // Assert
            var actionResult = Assert.IsType<ActionResult<Utilisateur>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var utilisateur = Assert.IsType<Utilisateur>(okResult.Value);
            Assert.Equal(utilisateurId, utilisateur.Id);
        }


        // Teste la récupération d'un utilisateur par ID lorsqu'il n'est pas trouvé
        [Fact]
        public async Task GetUtilisateurById_ReturnsNotFound_WhenNotFound()
        {
            // Arrange
            string guidString = "e01f50d1-c99d-4465-a6f7-ca4c4e4df3e8";
            var utilisateurId = Guid.Parse(guidString); // Un ID qui n'existe pas

            // Act
            var result = await _controller.GetUtilisateurById(utilisateurId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Utilisateur>>(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            Assert.Equal("Utilisateur non trouvé", notFoundResult.Value);
        }


        // Teste la récupération de tous les utilisateurs avec filtre sur le prénom
        [Fact]
        public async Task GetAllUtilisateursWithFilters_ReturnsFilteredUtilisateurs()
        {
            // Arrange
            var prenomFilter = "Val";

            // Act
            var result = await _controller.GetAllUtilisateursWithFilters(prenom: prenomFilter, nom: "");

            // Assert
            var actionResult = Assert.IsType<ActionResult<List<Utilisateur>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var utilisateurs = Assert.IsType<List<Utilisateur>>(okResult.Value);
            Assert.Single(utilisateurs);
            Assert.Equal("Valentin", utilisateurs.First().Prenom);
        }


        // Teste la récupération de tous les utilisateurs sans filtre
        [Fact]
        public async Task GetAllUtilisateursWithFilters_ReturnsAllUtilisateurs_WhenNoFilters()
        {
            // Act
            var result = await _controller.GetAllUtilisateursWithFilters(prenom: "", nom: "");

            // Assert
            var actionResult = Assert.IsType<ActionResult<List<Utilisateur>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var utilisateurs = Assert.IsType<List<Utilisateur>>(okResult.Value);
            Assert.Equal(2, utilisateurs.Count); // Il y a 2 utilisateurs en base de données
        }


        // Teste la suppression d'un utilisateur par ID
        [Fact]
        public async Task DeleteUtilisateur_ReturnsOk_WhenFound()
        {
            // Arrange
            var utilisateurId = _context.Utilisateurs.First().Id;

            // Act
            var result = await _controller.DeleteUtilisateur(utilisateurId);

            // Assert
            
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal("Utilisateur supprimé avec succès", okResult.Value);
        }

        // Teste la suppression d'un utilisateur par ID lorsqu'il n'est pas trouvé
        [Fact]
        public async Task DeleteUtilisateur_ReturnsNotFound_WhenNotFound()
        {
            // Arrange
            var utilisateurId = Guid.NewGuid(); // Un ID qui n'existe pas

            // Act
            var result = await _controller.DeleteUtilisateur(utilisateurId);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Utilisateur non trouvé", notFoundResult.Value);
        }


        // Teste l'ajout d'un nouvel utilisateur
        [Fact]
        public async Task AddUtilisateur_ReturnsCreatedUtilisateur()
        {
            // Arrange
            var newUtilisateur = new DtoCreateUtilisateur
            {
                Prenom = "Damien",
                Nom = "Degendt"
            };

            // Act
            var result = await _controller.AddUtilisateur(newUtilisateur);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Utilisateur>>(result);
            var createdResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var utilisateur = Assert.IsType<Utilisateur>(createdResult.Value);
            Assert.Equal(newUtilisateur.Prenom, utilisateur.Prenom);
            Assert.Equal(newUtilisateur.Nom, utilisateur.Nom);
        }

        // Teste l'ajout d'un nouvel utilisateur sans prénom et sans nom
        [Fact]
        public async Task AddUtilisateur_ReturnsBadRequest_WhenPrenomIsEmpty()
        {
            // Arrange
            var newUtilisateurSansPrenom = new DtoCreateUtilisateur
            {
                Prenom = "",
                Nom = "Degendt"
            };

            var newUtilisateurSansNom = new DtoCreateUtilisateur
            {
                Prenom = "Damien",
                Nom = ""
            };

            // Act
            var result = await _controller.AddUtilisateur(newUtilisateurSansPrenom);
            var result2 = await _controller.AddUtilisateur(newUtilisateurSansNom);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Utilisateur>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);

            var actionResult2 = Assert.IsType<ActionResult<Utilisateur>>(result2);
            var badRequestResult2 = Assert.IsType<BadRequestObjectResult>(actionResult2.Result);

            Assert.Equal("Le nom et le prénom sont obligatoires", badRequestResult.Value);
            Assert.Equal("Le nom et le prénom sont obligatoires", badRequestResult2.Value);
        }



        // Teste la modification d'un utilisateur
        [Fact]
        public async Task UpdateUtilisateur_ReturnsUpdatedUtilisateur()
        {
            // Arrange
            var utilisateurId = _context.Utilisateurs.First().Id;
            var updateUtilisateur = new DtoUpdateUtilisateur
            {
                Prenom = "Val",
                Nom = "Update"
            };

            // Act
            var result = await _controller.UpdateUtilisateur(utilisateurId, updateUtilisateur);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Utilisateur>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var updatedUtilisateur = Assert.IsType<Utilisateur>(okResult.Value);

            Assert.Equal(utilisateurId, updatedUtilisateur.Id);
            Assert.Equal("Val", updatedUtilisateur.Prenom);
            Assert.Equal("Update", updatedUtilisateur.Nom);
        }



        [Fact]
        public async Task UpdateUtilisateur_ReturnsNotFound_WhenUtilisateurDoesNotExist()
        {
            // Arrange
            string guidString = "e01f50d1-c99d-4465-a6f7-ca4c4e4df3e8";
            var utilisateurId = Guid.Parse(guidString); // Id qui n'existe pas
            var updateUtilisateur = new DtoUpdateUtilisateur
            {
                Prenom = "Fail",
                Nom = "Update"
            };

            // Act
            var result = await _controller.UpdateUtilisateur(utilisateurId, updateUtilisateur);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Utilisateur>>(result);
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
            Assert.Equal("Utilisateur non trouvé", notFoundResult.Value);
        }
    }
}
