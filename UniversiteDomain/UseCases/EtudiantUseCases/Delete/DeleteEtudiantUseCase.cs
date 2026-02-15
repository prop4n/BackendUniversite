using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;

namespace UniversiteDomain.UseCases.EtudiantUseCases.Delete;

public class DeleteEtudiantUseCase(IRepositoryFactory factory)
{
    public async Task ExecuteAsync(long idEtudiant)
    {
        await CheckBusinessRules(idEtudiant);
        IEtudiantRepository etudiantRepository = factory.EtudiantRepository();
        await etudiantRepository.DeleteAsync(idEtudiant);
        await factory.SaveChangesAsync();
    }

    private async Task CheckBusinessRules(long idEtudiant)
    {
        ArgumentNullException.ThrowIfNull(factory);
        IEtudiantRepository etudiantRepository = factory.EtudiantRepository();
        ArgumentNullException.ThrowIfNull(etudiantRepository);
        
        var etudiant = await etudiantRepository.FindAsync(idEtudiant);
        if (etudiant == null)
        {
            throw new ArgumentException($"L'étudiant avec l'ID {idEtudiant} n'existe pas.");
        }
    }

    public bool IsAuthorized(string role)
    {
        return role.Equals(Roles.Scolarite) || role.Equals(Roles.Responsable);
    }
}