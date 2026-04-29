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
    public List<string> Bancos { get; set; } = new List<string>();  // Inicializado
    public List<Dictionary<string, decimal>> DatosGrafica { get; set; } = new List<Dictionary<string, decimal>>();  // Inicializado
    public List<string> NombresMeses { get; set; } = new List<string>();  // Inicializado

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var jsonResponse = await _apiClient.DevolverIngresosPorBanco(Mes, Anio);
            
            var data = JsonConvert.DeserializeObject<JObject>(jsonResponse);
            
            if (data != null)
            {
                MesElegido = data["mesElegido"]?.ToString() ?? "";
                
                var bancosTemp = data["bancos"]?.ToObject<List<string>>();
                if (bancosTemp != null)
                {
                    Bancos = bancosTemp;
                }
                
                var datosTemp = data["datos"]?.ToObject<List<Dictionary<string, decimal>>>();
                if (datosTemp != null)
                {
                    DatosGrafica = datosTemp;
                }
                
                // Generar nombres de meses
                NombresMeses = new List<string>();
                for (int i = 0; i < 3; i++)
                {
                    int mesActual = Mes - i;
                    int anioActual = Anio;
                    
                    if (mesActual <= 0)
                    {
                        mesActual += 12;
                        anioActual--;
                    }
                    
                    var nombreMes = new DateTime(anioActual, mesActual, 1).ToString("MMM/yy");
                    NombresMeses.Add(nombreMes.ToLower());
                }
            }
        }
        catch (Exception ex)
        {
            ViewData["Error"] = ex.Message;
        }

        return Page();
    }
}