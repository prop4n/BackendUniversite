using UniversiteDomain.DataAdapters;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Entities;
using UniversiteDomain.Exceptions.ParcoursExceptions;

namespace UniversiteDomain.UseCases.ParcoursUseCases.Create;

public class CreateParcoursUseCase
{
    private readonly IParcoursRepository _parcoursRepository;
    
    public CreateParcoursUseCase(IRepositoryFactory factory)
    {
        _parcoursRepository = factory.ParcoursRepository();
    }
    
    public async Task<Parcours> ExecuteAsync(string nomParcours, int anneeFormation)
    {
        var parcours = new Parcours{NomParcours = nomParcours, AnneeFormation = anneeFormation};
        return await ExecuteAsync(parcours);
    }
    
    public async Task<Parcours> ExecuteAsync(Parcours parcours)
    {
        await CheckBusinessRules(parcours);
        Parcours pa = await _parcoursRepository.CreateAsync(parcours);
        await _parcoursRepository.SaveChangesAsync();
        return pa;
    }
    
    private async Task CheckBusinessRules(Parcours parcours)
    {
        ArgumentNullException.ThrowIfNull(parcours);
        ArgumentNullException.ThrowIfNull(parcours.NomParcours);
        
        List<Parcours> existe = await _parcoursRepository.FindByConditionAsync(e=>e.NomParcours.Equals(parcours.NomParcours));

        if (existe is {Count:>0}) throw new DuplicateNomParcoursException(parcours.NomParcours+ " - ce nom de parcours existe déjà");
        
        if (parcours.AnneeFormation < 0) throw new InvalidAnneeFormationException(parcours.AnneeFormation +" incorrect - l'année de formation doit-être supérieure à 0");
    }
}