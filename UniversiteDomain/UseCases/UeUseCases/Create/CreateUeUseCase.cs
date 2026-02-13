using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.UeExceptions;

namespace UniversiteDomain.UseCases.UeUseCases.Create;

/*
 *
 *
 *
 *
 *
 * namespace UniversiteDomain.Entities;
   
   public class Ue
   {
       public long Id { get; set; }
       public string NumeroUe { get; set; } = String.Empty;
       public string Intitule { get; set; } = String.Empty;
       // ManyToMany : une Ue est enseignée dnas plusieurs parcours
       public List<Parcours>? EnseigneeDans { get; set; } = new();
       
       public override string ToString()
       {
           return "ID "+Id +" : "+NumeroUe+" - "+Intitule;
       }
   }
 */

public class CreateUeUseCase
{
    
    private readonly IUeRepository _ueRepository;
    
    public CreateUeUseCase(IRepositoryFactory factory)
    {
        _ueRepository = factory.UeRepository();
    }

    public async Task<Ue> ExecuteAsync(string numUe, string intitule)
    {
        var ue = new Ue { NumeroUe = numUe, Intitule = intitule };
        return await _ueRepository.CreateAsync(ue);
    }

    public async Task<Ue> ExecuteAsync(Ue ue)
    {
        await CheckBusinessRules(ue);
        Ue u = await _ueRepository.CreateAsync(ue);
        await _ueRepository.SaveChangesAsync();
        return u;
    }
    
    private async Task CheckBusinessRules(Ue ue)
    {
        ArgumentNullException.ThrowIfNull(ue);
        ArgumentNullException.ThrowIfNull(ue.NumeroUe);
        ArgumentNullException.ThrowIfNull(ue.Intitule);
    
        // On recherche une Ue avec le même numéro
        List<Ue> existe = await _ueRepository.FindByConditionAsync(u => u.NumeroUe.Equals(ue.NumeroUe));

        // Si une Ue avec le même numéro existe déjà, on lève une exception personnalisée
        if (existe is {Count: > 0}) 
            throw new DuplicateNumeroUeException(ue.NumeroUe + " - ce numéro d'UE est déjà affecté à une UE");
    
        // On vérifie si l'intitulé est déjà utilisé
        existe = await _ueRepository.FindByConditionAsync(u => u.Intitule.Equals(ue.Intitule));
        if (existe is {Count: > 0}) 
            throw new DuplicateIntituleUeException(ue.Intitule + " - cet intitulé est déjà affecté à une UE");
    
        // Validation de l'intitulé (doit contenir plus de 3 caractères)
        if (ue.Intitule.Length < 3) 
            throw new InvalidIntituleUeException(ue.Intitule + " incorrect - L'intitulé d'une UE doit contenir plus de 3 caractères");
    
        // Validation du numéro d'UE (doit contenir plus de 2 caractères par exemple)
        if (ue.NumeroUe.Length < 2) 
            throw new InvalidNumeroUeException(ue.NumeroUe + " incorrect - Le numéro d'une UE doit contenir au moins 2 caractères");
    }
    
}

