using Microsoft.EntityFrameworkCore;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entities;
using UniversiteEFDataProvider.Data;

namespace UniversiteEFDataProvider.Repositories;

public class NoteRepository(UniversiteDbContext context) : Repository<Note>(context), INoteRepository
{
    public async Task<Note> AddNoteAsync(long etudiantId, long ueId, float valeur)
    {
        ArgumentNullException.ThrowIfNull(Context.Notes);
        ArgumentNullException.ThrowIfNull(Context.Etudiants);
        ArgumentNullException.ThrowIfNull(Context.Ues);
        
        // Vérifier que l'étudiant et l'UE existent
        Etudiant etudiant = (await Context.Etudiants.FindAsync(etudiantId))!;
        Ue ue = (await Context.Ues.FindAsync(ueId))!;
        
        // Créer la note
        Note note = new Note
        {
            EtudiantId = etudiantId,
            UeId = ueId,
            Valeur = valeur,
            Etudiant = etudiant,
            Ue = ue
        };
        
        var result = Context.Notes.Add(note);
        await Context.SaveChangesAsync();
        
        return result.Entity;
    }
}