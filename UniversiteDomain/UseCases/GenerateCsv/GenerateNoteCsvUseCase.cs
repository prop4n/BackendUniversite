using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Dtos;
using UniversiteDomain.Entities;

namespace UniversiteDomain.UseCases.NoteUseCases.GenerateCsv;

public class GenerateNoteCsvUseCase(IRepositoryFactory factory)
{
    public async Task<List<NoteCsvDto>> ExecuteAsync(long idUe)
    {
        await CheckBusinessRules(idUe);
        
        IUeRepository ueRepository = factory.UeRepository();
        IEtudiantRepository etudiantRepository = factory.EtudiantRepository();
        INoteRepository noteRepository = factory.NoteRepository();
        
        // Récupérer l'UE
        Ue? ue = await ueRepository.FindAsync(idUe);
        if (ue == null) throw new ArgumentException($"L'UE avec l'ID {idUe} n'existe pas");
        
        // Récupérer tous les étudiants qui suivent cette UE via les parcours
        List<Etudiant> etudiants = await etudiantRepository.FindByConditionAsync(
            e => e.ParcoursSuivi != null && e.ParcoursSuivi.UesEnseignees.Any(u => u.Id == idUe)
        );
        
        // Créer les lignes CSV
        List<NoteCsvDto> csvData = new List<NoteCsvDto>();
        
        foreach (var etudiant in etudiants.OrderBy(e => e.Nom).ThenBy(e => e.Prenom))
        {
            // Chercher si une note existe déjà
            var noteExistante = await noteRepository.FindAsync(etudiant.Id, idUe);
            
            csvData.Add(new NoteCsvDto
            {
                NumEtud = etudiant.NumEtud,
                Nom = etudiant.Nom,
                Prenom = etudiant.Prenom,
                NumeroUe = ue.NumeroUe,
                IntituleUe = ue.Intitule,
                Note = noteExistante?.Valeur.ToString() ?? string.Empty
            });
        }
        
        return csvData;
    }

    private async Task CheckBusinessRules(long idUe)
    {
        ArgumentNullException.ThrowIfNull(factory);
        if (idUe <= 0) throw new ArgumentException("L'ID de l'UE doit être positif");
    }

    public bool IsAuthorized(string role)
    {
        return role.Equals(Roles.Scolarite);
    }
}