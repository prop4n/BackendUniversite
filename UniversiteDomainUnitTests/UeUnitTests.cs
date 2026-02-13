using Moq;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.UeExceptions;
using UniversiteDomain.UseCases.UeUseCases.Create;

namespace UniversiteDomainUnitTests;

public class UeUnitTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task CreateUeUseCase_ValidUe_ShouldCreateUe()
    {
        // Arrange
        long idUe = 1;
        string numUe = "UE01";
        string intitule = "Mathématiques";
        
        Ue ueAvant = new Ue { NumeroUe = numUe, Intitule = intitule };
        
        var mockUe = new Mock<IUeRepository>();
        
        // On dit que le numéro n'existe pas déjà
        mockUe
            .Setup(repo => repo.FindByConditionAsync(u => u.NumeroUe.Equals(numUe)))
            .ReturnsAsync((List<Ue>)null);
        
        // On dit que l'intitulé n'existe pas déjà
        mockUe
            .Setup(repo => repo.FindByConditionAsync(u => u.Intitule.Equals(intitule)))
            .ReturnsAsync((List<Ue>)null);
        
        Ue ueFinal = new Ue { Id = idUe, NumeroUe = numUe, Intitule = intitule };
        mockUe.Setup(repo => repo.CreateAsync(ueAvant)).ReturnsAsync(ueFinal);
        mockUe.Setup(repo => repo.SaveChangesAsync()).Returns(Task.CompletedTask);
        
        var mockFactory = new Mock<IRepositoryFactory>();
        mockFactory.Setup(facto => facto.UeRepository()).Returns(mockUe.Object);
        
        CreateUeUseCase useCase = new CreateUeUseCase(mockFactory.Object);
        
        // Act
        var ueTeste = await useCase.ExecuteAsync(ueAvant);
        
        // Assert
        Assert.That(ueTeste.Id, Is.EqualTo(ueFinal.Id));
        Assert.That(ueTeste.NumeroUe, Is.EqualTo(ueFinal.NumeroUe));
        Assert.That(ueTeste.Intitule, Is.EqualTo(ueFinal.Intitule));
    }
}