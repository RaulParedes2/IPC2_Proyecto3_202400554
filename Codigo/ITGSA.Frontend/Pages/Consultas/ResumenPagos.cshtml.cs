using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ITGSA.Frontend.Services;

namespace ITGSA.Frontend.Pages.Consultas;

public class ResumenPagosModel : PageModel
{
    private readonly ApiClient _apiClient;

    public ResumenPagosModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty(SupportsGet = true)]
    public int Mes { get; set; } = DateTime.Now.Month;

    [BindProperty(SupportsGet = true)]
    public int Anio { get; set; } = DateTime.Now.Year;

    public string? MesElegido { get; set; }
    public List<IngresoMensual>? Ingresos { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var jsonResponse = await _apiClient.DevolverResumenPagos(Mes, Anio);
            
            var data = JsonConvert.DeserializeObject<JObject>(jsonResponse);
            
            if (data != null)
            {
                MesElegido = data["mesElegido"]?.ToString();
                Ingresos = data["ultimosTresMeses"]?.ToObject<List<IngresoMensual>>();
            }
        }
        catch (Exception ex)
        {
            // Si hay error, mostramos mensaje en la vista
            ViewData["Error"] = ex.Message;
        }

        return Page();
    }
}

public class IngresoMensual
{
    public string Mes { get; set; } = string.Empty;
    public decimal Total { get; set; }
}