using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Dtos;
using UniversiteDomain.Entities;

namespace UniversiteDomain.UseCases.NoteUseCases.ImportCsv;

public class ImportNoteCsvUseCase(IRepositoryFactory factory)
{
    public async Task ExecuteAsync(List<NoteCsvDto> csvData)
    {
        // Validation complète AVANT toute écriture en base
        await ValidateCsvData(csvData);
        
        IEtudiantRepository etudiantRepository = factory.EtudiantRepository();
        IUeRepository ueRepository = factory.UeRepository();
        INoteRepository noteRepository = factory.NoteRepository();
        
        // Si on arrive ici, toutes les validations sont OK
        foreach (var ligne in csvData)
        {
            // Ignorer les lignes sans note
            if (string.IsNullOrWhiteSpace(ligne.Note)) continue;
            
            float valeurNote = float.Parse(ligne.Note);
            
            // Trouver l'étudiant et l'UE
            var etudiant = await etudiantRepository.FindByConditionAsync(e => e.NumEtud == ligne.NumEtud);
            var ue = await ueRepository.FindByConditionAsync(u => u.NumeroUe == ligne.NumeroUe);
            
            if (etudiant.Count == 0 || ue.Count == 0) continue;
            
            long etudiantId = etudiant[0].Id;
            long ueId = ue[0].Id;
            
            // Vérifier si la note existe déjà
            var noteExistante = await noteRepository.FindAsync(etudiantId, ueId);
            
            if (noteExistante != null)
            {
                // Mettre à jour
                noteExistante.Valeur = valeurNote;
                await noteRepository.UpdateAsync(noteExistante);
            }
            else
            {
                // Créer
                Note nouvelleNote = new Note
                {
                    EtudiantId = etudiantId,
                    UeId = ueId,
                    Valeur = valeurNote
                };
                await noteRepository.CreateAsync(nouvelleNote);
            }
        }
        
        await factory.SaveChangesAsync();
    }

    private async Task ValidateCsvData(List<NoteCsvDto> csvData)
    {
        ArgumentNullException.ThrowIfNull(factory);
        if (csvData == null || csvData.Count == 0)
            throw new ArgumentException("Le fichier CSV est vide");
        
        List<string> erreurs = new List<string>();
        IEtudiantRepository etudiantRepository = factory.EtudiantRepository();
        IUeRepository ueRepository = factory.UeRepository();
        
        int ligne = 1;
        foreach (var row in csvData)
        {
            ligne++;
            
            // Validation de la note (si présente)
            if (!string.IsNullOrWhiteSpace(row.Note))
            {
                if (!float.TryParse(row.Note, out float valeur))
                {
                    erreurs.Add($"Ligne {ligne}: La note '{row.Note}' n'est pas un nombre valide");
                }
                else if (valeur < 0 || valeur > 20)
                {
                    erreurs.Add($"Ligne {ligne}: La note {valeur} doit être entre 0 et 20");
                }
            }
            
            // Validation de l'étudiant
            var etudiant = await etudiantRepository.FindByConditionAsync(e => e.NumEtud == row.NumEtud);
            if (etudiant.Count == 0)
            {
                erreurs.Add($"Ligne {ligne}: L'étudiant {row.NumEtud} n'existe pas");
            }
            
            // Validation de l'UE
            var ue = await ueRepository.FindByConditionAsync(u => u.NumeroUe == row.NumeroUe);
            if (ue.Count == 0)
            {
                erreurs.Add($"Ligne {ligne}: L'UE {row.NumeroUe} n'existe pas");
            }
        }
        
        if (erreurs.Count > 0)
        {
            throw new InvalidOperationException("Erreurs dans le fichier CSV:\n" + string.Join("\n", erreurs));
        }
    }

    public bool IsAuthorized(string role)
    {
        return role.Equals(Roles.Scolarite);
    }
}