using Microsoft.EntityFrameworkCore;
using UniversiteDomain.DataAdapters;
using UniversiteDomain.Entities;
using UniversiteEFDataProvider.Data;

namespace UniversiteEFDataProvider.Repositories;

public class ParcoursRepository(UniversiteDbContext context) : Repository<Parcours>(context), IParcoursRepository
{
    // Ajouter un étudiant à un parcours (par entités)
    public async Task<Parcours> AddEtudiantAsync(Parcours parcours, Etudiant etudiant)
    {
        return await AddEtudiantAsync(parcours.Id, etudiant.Id);
    }

    // Ajouter un étudiant à un parcours (par IDs)
    public async Task<Parcours> AddEtudiantAsync(long idParcours, long idEtudiant)
    {
        ArgumentNullException.ThrowIfNull(Context.Parcours);
        ArgumentNullException.ThrowIfNull(Context.Etudiants);
        
        Parcours parcours = (await Context.Parcours
            .Include(p => p.Inscrits)
            .FirstOrDefaultAsync(p => p.Id == idParcours))!;
        Etudiant etudiant = (await Context.Etudiants.FindAsync(idEtudiant))!;
        
        parcours.Inscrits ??= new List<Etudiant>();
        parcours.Inscrits.Add(etudiant);
        
        await Context.SaveChangesAsync();
        return parcours;
    }

    // Ajouter plusieurs étudiants à un parcours (par entités)
    public async Task<Parcours> AddEtudiantAsync(Parcours? parcours, List<Etudiant> etudiants)
    {
        ArgumentNullException.ThrowIfNull(parcours);
        long[] idEtudiants = etudiants.Select(e => e.Id).ToArray();
        return await AddEtudiantAsync(parcours.Id, idEtudiants);
    }

    // Ajouter plusieurs étudiants à un parcours (par IDs)
    public async Task<Parcours> AddEtudiantAsync(long idParcours, long[] idEtudiants)
    {
        ArgumentNullException.ThrowIfNull(Context.Parcours);
        ArgumentNullException.ThrowIfNull(Context.Etudiants);
        
        Parcours parcours = (await Context.Parcours
            .Include(p => p.Inscrits)
            .FirstOrDefaultAsync(p => p.Id == idParcours))!;
        
        parcours.Inscrits ??= new List<Etudiant>();
        
        foreach (long idEtudiant in idEtudiants)
        {
            Etudiant etudiant = (await Context.Etudiants.FindAsync(idEtudiant))!;
            parcours.Inscrits.Add(etudiant);
        }
        
        await Context.SaveChangesAsync();
        return parcours;
    }

    // Ajouter une UE à un parcours (par IDs)
    public async Task<Parcours> AddUeAsync(long idParcours, long idUe)
    {
        ArgumentNullException.ThrowIfNull(Context.Parcours);
        ArgumentNullException.ThrowIfNull(Context.Ues);
        
        Parcours parcours = (await Context.Parcours
            .Include(p => p.UesEnseignees)
            .FirstOrDefaultAsync(p => p.Id == idParcours))!;
        Ue ue = (await Context.Ues.FindAsync(idUe))!;
        
        parcours.UesEnseignees ??= new List<Ue>();
        parcours.UesEnseignees.Add(ue);
        
        await Context.SaveChangesAsync();
        return parcours;
    }

    // Ajouter plusieurs UEs à un parcours (par IDs)
    public async Task<Parcours> AddUeAsync(long idParcours, long[] idUes)
    {
        ArgumentNullException.ThrowIfNull(Context.Parcours);
        ArgumentNullException.ThrowIfNull(Context.Ues);
        
        Parcours parcours = (await Context.Parcours
            .Include(p => p.UesEnseignees)
            .FirstOrDefaultAsync(p => p.Id == idParcours))!;
        
        parcours.UesEnseignees ??= new List<Ue>();
        
        foreach (long idUe in idUes)
        {
            Ue ue = (await Context.Ues.FindAsync(idUe))!;
            parcours.UesEnseignees.Add(ue);
        }
        
        await Context.SaveChangesAsync();
        return parcours;
    }
}