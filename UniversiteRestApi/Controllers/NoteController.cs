using System.Globalization;
using System.Security.Claims;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using UniversiteDomain.DataAdapters.DataAdaptersFactory;
using UniversiteDomain.Dtos;
using UniversiteDomain.Entities;
using UniversiteDomain.UseCases.NoteUseCases.GenerateCsv;
using UniversiteDomain.UseCases.NoteUseCases.ImportCsv;
using UniversiteDomain.UseCases.SecurityUseCases.Get;
using CsvWriter = CsvHelper.CsvWriter;  // ✅ Ajoute cette ligne
using CsvReader = CsvHelper.CsvReader;  // ✅ Ajoute cette ligne

namespace UniversiteRestApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NoteController(IRepositoryFactory repositoryFactory) : ControllerBase
{
    // GET api/Note/csv/5 - Télécharger le CSV pour une UE
    [HttpGet("csv/{idUe}")]
    public async Task<IActionResult> GetCsvAsync(long idUe)
    {
        string role = "";
        string email = "";
        IUniversiteUser user = null;
        try
        {
            CheckSecu(out role, out email, out user);
        }
        catch (Exception)
        {
            return Unauthorized();
        }

        GenerateNoteCsvUseCase uc = new GenerateNoteCsvUseCase(repositoryFactory);
        if (!uc.IsAuthorized(role)) return Unauthorized();

        List<NoteCsvDto> csvData;
        try
        {
            csvData = await uc.ExecuteAsync(idUe);
        }
        catch (Exception e)
        {
            ModelState.AddModelError(nameof(e), e.Message);
            return ValidationProblem();
        }

        // Générer le fichier CSV
        using var memoryStream = new MemoryStream();
        using (var writer = new StreamWriter(memoryStream))
        using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = ";"
        }))
        {
            csv.WriteRecords(csvData);
        }

        var fileBytes = memoryStream.ToArray();
        return File(fileBytes, "text/csv", $"notes_ue_{idUe}.csv");
    }

    // POST api/Note/csv - Uploader le CSV rempli
    [HttpPost("csv")]
    public async Task<IActionResult> PostCsvAsync(IFormFile file)
    {
        string role = "";
        string email = "";
        IUniversiteUser user = null;
        try
        {
            CheckSecu(out role, out email, out user);
        }
        catch (Exception)
        {
            return Unauthorized();
        }

        ImportNoteCsvUseCase uc = new ImportNoteCsvUseCase(repositoryFactory);
        if (!uc.IsAuthorized(role)) return Unauthorized();

        if (file == null || file.Length == 0)
            return BadRequest("Aucun fichier fourni");

        List<NoteCsvDto> csvData;
        try
        {
            using var reader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";"
            });
            csvData = csv.GetRecords<NoteCsvDto>().ToList();
        }
        catch (Exception e)
        {
            return BadRequest($"Erreur lors de la lecture du fichier CSV: {e.Message}");
        }

        try
        {
            await uc.ExecuteAsync(csvData);
        }
        catch (Exception e)
        {
            ModelState.AddModelError(nameof(e), e.Message);
            return ValidationProblem();
        }

        return Ok("Notes importées avec succès");
    }

    private void CheckSecu(out string role, out string email, out IUniversiteUser user)
    {
        role = "";
        ClaimsPrincipal claims = HttpContext.User;
        if (claims.FindFirst(ClaimTypes.Email) == null) throw new UnauthorizedAccessException();
        email = claims.FindFirst(ClaimTypes.Email).Value;
        if (email == null) throw new UnauthorizedAccessException();
        user = new FindUniversiteUserByEmailUseCase(repositoryFactory).ExecuteAsync(email).Result;
        if (user == null) throw new UnauthorizedAccessException();
        if (claims.Identity?.IsAuthenticated != true) throw new UnauthorizedAccessException();
        var ident = claims.Identities.FirstOrDefault();
        if (ident == null) throw new UnauthorizedAccessException();
        if (claims.FindFirst(ClaimTypes.Role) == null) throw new UnauthorizedAccessException();
        role = ident.FindFirst(ClaimTypes.Role).Value;
        if (role == null) throw new UnauthorizedAccessException();
    }
}