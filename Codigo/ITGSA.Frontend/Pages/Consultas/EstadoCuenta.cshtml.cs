using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ITGSA.Frontend.Services;

namespace ITGSA.Frontend.Pages.Consultas;

public class EstadoCuentaModel : PageModel
{
    private readonly ApiClient _apiClient;

    public EstadoCuentaModel(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    [BindProperty(SupportsGet = true)]
    public string? NIT { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool Todos { get; set; }

    public ClienteEstado? Resultado { get; set; }
    public string? Error { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // Si no hay consulta y no es "todos", mostrar página vacía
        if (string.IsNullOrEmpty(NIT) && !Todos)
        {
            return Page();
        }

        try
        {
            string jsonResponse;
            
            if (Todos)
            {
                jsonResponse = await _apiClient.DevolverEstadoCuenta(null, true);
            }
            else
            {
                jsonResponse = await _apiClient.DevolverEstadoCuenta(NIT, false);
            }

            // Parsear la respuesta JSON
            var data = JsonConvert.DeserializeObject<JToken>(jsonResponse);
            
            if (Todos)
            {
                // Si son múltiples clientes, mostramos el primero por simplicidad
                var clientes = data?.ToObject<List<ClienteEstado>>();
                if (clientes != null && clientes.Any())
                {
                    Resultado = clientes.FirstOrDefault();
                    if (clientes.Count > 1)
                    {
                        ViewData["TotalClientes"] = clientes.Count;
                    }
                }
                else
                {
                    Error = "No hay clientes registrados";
                }
            }
            else
            {
                Resultado = data?.ToObject<ClienteEstado>();
                if (Resultado == null)
                {
                    Error = "No se encontró el cliente";
                }
            }
        }
        catch (Exception ex)
        {
            Error = $"Error al consultar: {ex.Message}";
        }

        return Page();
    }
}

// Modelo para el estado de cuenta
public class ClienteEstado
{
    public string NIT { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public decimal SaldoActual { get; set; }
    public List<TransaccionEstado> Transacciones { get; set; } = new();
}

public class TransaccionEstado
{
    public string Fecha { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public decimal Cargo { get; set; }
    public decimal Abono { get; set; }
}