using System.Linq.Expressions;
using Moq;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.UseCases.EtudiantUseCases;
using UniversiteDomain.UseCases.EtudiantUseCases.Create;

namespace UniversiteDomainUnitTests;

public class EtudiantUnitTest
{
    [SetUp]
    public void Setup()
    {
    }
    
    [Test]
    public async Task CreateEtudiantUseCase()
    {
        long id = 1;
        String numEtud = "et1";
        string nom = "Durant";
        string prenom = "Jean";
        string email = "jean.durant@etud.u-picardie.fr";
        
        // On crée l'étudiant qui doit être ajouté en base
        Etudiant etudiantSansId = new Etudiant{NumEtud=numEtud, Nom = nom, Prenom=prenom, Email=email};
        
        // Créons le mock du repository
        var mockEtudiant = new Mock<IEtudiantRepository>();
        
        // Simulation de la fonction FindByConditionAsync
        // On dit à ce mock que l'étudiant n'existe pas déjà
        // La réponse à l'appel FindByCondition est donc une liste vide
        var reponseFindByCondition = new List<Etudiant>();
        // On crée un bouchon dans le mock pour la fonction FindByCondition
        // Quelque soit le paramètre de la fonction FindByCondition, on renvoie la liste vide
        mockEtudiant
            .Setup(repo=>repo.FindByConditionAsync(It.IsAny<Expression<Func<Etudiant, bool>>>()))
            .ReturnsAsync(reponseFindByCondition);
        
        // Simulation de la fonction CreateAsync
        // On lui dit que l'ajout d'un étudiant renvoie un étudiant avec l'Id 1
        Etudiant etudiantCree = new Etudiant{Id=id, NumEtud=numEtud, Nom = nom, Prenom=prenom, Email=email};
        mockEtudiant
            .Setup(repo=>repo.CreateAsync(etudiantSansId))
            .ReturnsAsync(etudiantCree);
        
        // Simulation de SaveChangesAsync
        mockEtudiant
            .Setup(repo=>repo.SaveChangesAsync())
            .Returns(Task.CompletedTask);
        
        // Création du mock de la factory
        var mockFactory = new Mock<IRepositoryFactory>();
        mockFactory
            .Setup(factory=>factory.EtudiantRepository())
            .Returns(mockEtudiant.Object);
        
        // Création du use case en injectant la factory
        CreateEtudiantUseCase useCase = new CreateEtudiantUseCase(mockFactory.Object);
        
        // Appel du use case
        var etudiantTeste = await useCase.ExecuteAsync(etudiantSansId);
        
        // Vérification du résultat
        Assert.That(etudiantTeste.Id, Is.EqualTo(etudiantCree.Id));
        Assert.That(etudiantTeste.NumEtud, Is.EqualTo(etudiantCree.NumEtud));
        Assert.That(etudiantTeste.Nom, Is.EqualTo(etudiantCree.Nom));
        Assert.That(etudiantTeste.Prenom, Is.EqualTo(etudiantCree.Prenom));
        Assert.That(etudiantTeste.Email, Is.EqualTo(etudiantCree.Email));
    }
}