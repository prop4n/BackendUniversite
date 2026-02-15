using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;

namespace UniversiteDomain.UseCases.EtudiantUseCases.Update;

public class UpdateEtudiantUseCase(IRepositoryFactory factory)
{
    public async Task ExecuteAsync(Etudiant etudiant)
    {
        await CheckBusinessRules(etudiant);
        IEtudiantRepository etudiantRepository = factory.EtudiantRepository();
        await etudiantRepository.UpdateAsync(etudiant);
        await factory.SaveChangesAsync();
    }

    private async Task CheckBusinessRules(Etudiant etudiant)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(etudiant);
        IEtudiantRepository etudiantRepository = factory.EtudiantRepository();
        ArgumentNullException.ThrowIfNull(etudiantRepository);

        // Vérifier que l'étudiant existe
        var existing = await etudiantRepository.FindAsync(etudiant.Id);
        if (existing == null)
        {
            throw new InvalidOperationException($"L'étudiant avec l'ID {etudiant.Id} n'existe pas.");
        }
    }

    public bool IsAuthorized(string role)
    {
        return role.Equals(Roles.Scolarite) || role.Equals(Roles.Responsable);
    }
}