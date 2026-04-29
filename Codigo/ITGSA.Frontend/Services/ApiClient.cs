using System.Xml.Linq;
using Newtonsoft.Json;

namespace ITGSA.Frontend.Services
{
    public class ApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ApiClient(IConfiguration configuration)
        {
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7000";
            _httpClient = new HttpClient();
        }

        public async Task<string> LimpiarDatos()
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/System/limpiarDatos");
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GrabarConfiguracion(Stream fileStream, string fileName)
        {
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.Add("Content-Type", "application/xml");
            content.Add(streamContent, "file", fileName);

            var response = await _httpClient.PostAsync($"{_baseUrl}/System/grabarConfiguracion", content);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> GrabarTransaccion(Stream fileStream, string fileName)
        {
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.Add("Content-Type", "application/xml");
            content.Add(streamContent, "file", fileName);

            var response = await _httpClient.PostAsync($"{_baseUrl}/System/grabarTransaccion", content);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> DevolverEstadoCuenta(string? nit = null, bool todos = false)
        {
            string url = $"{_baseUrl}/System/devolverEstadoCuenta";
            if (todos)
                url += "?todos=true";
            else if (!string.IsNullOrEmpty(nit))
                url += $"?nit={nit}";

            var response = await _httpClient.GetAsync(url);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> DevolverResumenPagos(int mes, int anio)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/System/devolverResumenPagos?mes={mes}&anio={anio}");
            return await response.Content.ReadAsStringAsync();
        }

        //-----------------------------------------------------------------------------
        // En ApiClient.cs agregar:
        public async Task<string> DevolverIngresosPorBanco(int mes, int anio)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/System/devolverIngresosPorBanco?mes={mes}&anio={anio}");
            return await response.Content.ReadAsStringAsync();
        }
    }
}