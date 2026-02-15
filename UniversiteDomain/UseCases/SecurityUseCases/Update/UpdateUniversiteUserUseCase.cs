using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;

namespace UniversiteDomain.UseCases.SecurityUseCases.Update;

public class UpdateUniversiteUserUseCase(IRepositoryFactory factory)
{
    public async Task ExecuteAsync(Etudiant etudiant)
    {
        await CheckBusinessRules(etudiant);
        IUniversiteUserRepository userRepository = factory.UniversiteUserRepository();
        
        // Trouver l'utilisateur par email
        IUniversiteUser? user = await userRepository.FindByEmailAsync(etudiant.Email);
        if (user == null)
        {
            throw new InvalidOperationException($"Aucun utilisateur trouvé avec l'email {etudiant.Email}");
        }

        // Mettre à jour l'utilisateur
        await userRepository.UpdateAsync(user, etudiant.Email, etudiant.Email);
    }

    private async Task CheckBusinessRules(Etudiant etudiant)
    {
        ArgumentNullException.ThrowIfNull(factory);
        ArgumentNullException.ThrowIfNull(etudiant);
        IUniversiteUserRepository userRepository = factory.UniversiteUserRepository();
        ArgumentNullException.ThrowIfNull(userRepository);
    }

    public bool IsAuthorized(string role)
    {
        return role.Equals(Roles.Scolarite) || role.Equals(Roles.Responsable);
    }
}