using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.EtudiantExceptions;
using UniversiteDomain.Exceptions.NoteExceptions;
using UniversiteDomain.Exceptions.ParcoursExceptions;
using UniversiteDomain.Exceptions.UeExceptions;

namespace UniversiteDomain.UseCases.NoteUseCases.Create;

public class CreateNoteUseCase(IRepositoryFactory repositoryFactory)
{
    public async Task<Note> ExecuteAsync(long etudiantId, long ueId, double valeur)
    {
        await CheckBusinessRules(etudiantId, ueId, valeur);
        return await repositoryFactory.NoteRepository().CreateAsync(
            new Note { EtudiantId = etudiantId, UeId = ueId, Valeur = valeur }
        );
    }
    
    private async Task CheckBusinessRules(long etudiantId, long ueId, double valeur)
    {
        // Vérification des paramètres
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(etudiantId);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(ueId);
        
        // Vérification de la connexion aux datasources
        ArgumentNullException.ThrowIfNull(repositoryFactory);
        ArgumentNullException.ThrowIfNull(repositoryFactory.EtudiantRepository());
        ArgumentNullException.ThrowIfNull(repositoryFactory.UeRepository());
        ArgumentNullException.ThrowIfNull(repositoryFactory.ParcoursRepository());
        ArgumentNullException.ThrowIfNull(repositoryFactory.NoteRepository());
        
        // Règle 1 : La note doit être entre 0 et 20
        if (valeur < 0 || valeur > 20)
            throw new InvalidNoteValueException($"La note {valeur} n'est pas valide. Elle doit être entre 0 et 20.");
        
        // Vérifier que l'étudiant existe
        List<Etudiant>? etudiants = await repositoryFactory.EtudiantRepository()
            .FindByConditionAsync(e => e.Id.Equals(etudiantId));
        if (etudiants == null || etudiants.Count == 0)
            throw new EtudiantNotFoundException(etudiantId.ToString());
        
        Etudiant etudiant = etudiants[0];
        
        // Vérifier que l'UE existe
        List<Ue>? ues = await repositoryFactory.UeRepository()
            .FindByConditionAsync(u => u.Id.Equals(ueId));
        if (ues == null || ues.Count == 0)
            throw new UeNotFoundException(ueId.ToString());
        
        // Règle 2 : Un étudiant n'a qu'une note au maximum par UE
        List<Note>? notesExistantes = await repositoryFactory.NoteRepository()
            .FindByConditionAsync(n => n.EtudiantId.Equals(etudiantId) && n.UeId.Equals(ueId));
        if (notesExistantes != null && notesExistantes.Count > 0)
            throw new DuplicateNoteException($"L'étudiant {etudiantId} a déjà une note dans l'UE {ueId}.");
        
        // Règle 3 : L'étudiant ne peut avoir une note que dans une UE de son parcours
        // On récupère le parcours de l'étudiant
        List<Parcours>? parcours = await repositoryFactory.ParcoursRepository()
            .FindByConditionAsync(p => p.Inscrits != null && p.Inscrits.Any(e => e.Id.Equals(etudiantId)));
        
        if (parcours == null || parcours.Count == 0)
            throw new ParcoursNotFoundException($"L'étudiant {etudiantId} n'est inscrit dans aucun parcours.");
        
        Parcours parcoursEtudiant = parcours[0];
        
        // Vérifier que l'UE fait partie du parcours
        if (parcoursEtudiant.UesEnseignees == null || 
            !parcoursEtudiant.UesEnseignees.Any(u => u.Id.Equals(ueId)))
        {
            throw new UeNotInParcoursException(
                $"L'UE {ueId} ne fait pas partie du parcours {parcoursEtudiant.Id} de l'étudiant {etudiantId}.");
        }
    }
}