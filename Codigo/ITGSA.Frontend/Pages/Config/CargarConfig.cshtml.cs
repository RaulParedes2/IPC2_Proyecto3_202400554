using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ITGSA.Frontend.Services;

namespace ITGSA.Frontend.Pages.Config;

public class CargarConfigModel : PageModel
{
    private readonly ApiClient _apiClient;

    public CargarConfigModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty]
    public IFormFile? XmlFile { get; set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (XmlFile == null || XmlFile.Length == 0)
        {
            ViewData["Error"] = "No se seleccionó ningún archivo";
            return Page();
        }

        if (Path.GetExtension(XmlFile.FileName).ToLower() != ".xml")
        {
            ViewData["Error"] = "El archivo debe ser XML";
            return Page();
        }

        try
        {
            using var stream = XmlFile.OpenReadStream();
            var resultado = await _apiClient.GrabarConfiguracion(stream, XmlFile.FileName);
            ViewData["Resultado"] = resultado;
        }
        catch (Exception ex)
        {
            ViewData["Error"] = $"Error al procesar el archivo: {ex.Message}";
        }

        return Page();
    }
}