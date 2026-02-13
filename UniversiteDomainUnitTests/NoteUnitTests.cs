using Moq;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.UseCases.NoteUseCases.Create;

namespace UniversiteDomainUnitTests;

public class NoteUnitTests
{
    
    [SetUp]
    public void Setup()
    {
    }
    
    [Test]
    public async Task CreateNoteUseCase()
    {
        long idNote = 1;
        long idEtudiant = 2;
        long idUe = 3;
        long idParcours = 4;
        double valeurNote = 15.5;
        
        Etudiant etudiant = new Etudiant { Id = idEtudiant, NumEtud = "E001", Nom = "Dupont", Prenom = "Jean", Email = "jean@test.fr" };
        Ue ue = new Ue { Id = idUe, NumeroUe = "UE01", Intitule = "Programmation" };
        Parcours parcours = new Parcours { Id = idParcours, NomParcours = "MIAGE", AnneeFormation = 1 };
        parcours.Inscrits.Add(etudiant);
        parcours.UesEnseignees.Add(ue);
        
        // On initialise des faux repositories
        var mockEtudiant = new Mock<IEtudiantRepository>();
        var mockUe = new Mock<IUeRepository>();
        var mockParcours = new Mock<IParcoursRepository>();
        var mockNote = new Mock<INoteRepository>();
        
        // Mock de la recherche de l'étudiant
        List<Etudiant> etudiants = new List<Etudiant> { etudiant };
        mockEtudiant
            .Setup(repo => repo.FindByConditionAsync(e => e.Id.Equals(idEtudiant)))
            .ReturnsAsync(etudiants);
        
        // Mock de la recherche de l'UE
        List<Ue> ues = new List<Ue> { ue };
        mockUe
            .Setup(repo => repo.FindByConditionAsync(u => u.Id.Equals(idUe)))
            .ReturnsAsync(ues);
        
        // Mock de la vérification qu'aucune note n'existe déjà
        mockNote
            .Setup(repo => repo.FindByConditionAsync(n => n.EtudiantId.Equals(idEtudiant) && n.UeId.Equals(idUe)))
            .ReturnsAsync(new List<Note>()); // Liste vide = pas de note existante
        
        // Mock de la recherche du parcours de l'étudiant
        List<Parcours> parcourses = new List<Parcours> { parcours };
        mockParcours
            .Setup(repo => repo.FindByConditionAsync(p => p.Inscrits != null && p.Inscrits.Any(e => e.Id.Equals(idEtudiant))))
            .ReturnsAsync(parcourses);
        
        // Mock de la création de la note
        Note noteFinal = new Note { Id = idNote, EtudiantId = idEtudiant, UeId = idUe, Valeur = valeurNote };
        mockNote
            .Setup(repo => repo.CreateAsync(It.Is<Note>(n => n.EtudiantId == idEtudiant && n.UeId == idUe && n.Valeur == valeurNote)))
            .ReturnsAsync(noteFinal);
        
        // Création d'une fausse factory
        var mockFactory = new Mock<IRepositoryFactory>();
        mockFactory.Setup(facto => facto.EtudiantRepository()).Returns(mockEtudiant.Object);
        mockFactory.Setup(facto => facto.UeRepository()).Returns(mockUe.Object);
        mockFactory.Setup(facto => facto.ParcoursRepository()).Returns(mockParcours.Object);
        mockFactory.Setup(facto => facto.NoteRepository()).Returns(mockNote.Object);
        
        // Création du use case
        CreateNoteUseCase useCase = new CreateNoteUseCase(mockFactory.Object);
        
        // Appel du use case
        var noteTest = await useCase.ExecuteAsync(idEtudiant, idUe, valeurNote);
        
        // Vérification du résultat
        Assert.That(noteTest.Id, Is.EqualTo(noteFinal.Id));
        Assert.That(noteTest.EtudiantId, Is.EqualTo(idEtudiant));
        Assert.That(noteTest.UeId, Is.EqualTo(idUe));
        Assert.That(noteTest.Valeur, Is.EqualTo(valeurNote));
    }
}