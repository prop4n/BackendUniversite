namespace UniversiteDomain.Entities;

public class Note
{
    public long Id { get; set; }
    
    // Clés étrangères pour la relation ternaire Etudiant-Ue-Parcours
    public long EtudiantId { get; set; }
    public long UeId { get; set; }
    
    // La valeur de la note
    public double Valeur { get; set; }
    
    // Navigation properties
    public Etudiant? Etudiant { get; set; }
    public Ue? Ue { get; set; }
    
    public override string ToString()
    {
        return $"Note ID {Id} : Etudiant {EtudiantId} - UE {UeId} - {Valeur}/20";
    }
}