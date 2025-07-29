using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace DiversityPub.Services
{
    public class GeocodingService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeocodingService> _logger;

        public GeocodingService(HttpClient httpClient, ILogger<GeocodingService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        /// <summary>
        /// Convertit une adresse en coordonnées GPS en utilisant Nominatim (OpenStreetMap)
        /// </summary>
        /// <param name="address">Adresse à géocoder</param>
        /// <returns>Tuple avec latitude et longitude, ou null si échec</returns>
        public async Task<(double Latitude, double Longitude)?> GeocodeAddressAsync(string address)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(address))
                {
                    _logger.LogWarning("Adresse vide pour le géocodage");
                    return null;
                }

                // Encoder l'adresse pour l'URL
                var encodedAddress = Uri.EscapeDataString(address);
                var url = $"https://nominatim.openstreetmap.org/search?q={encodedAddress}&format=json&limit=1";

                // Ajouter un User-Agent pour respecter les conditions d'utilisation
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "DiversityPub/1.0");

                var response = await _httpClient.GetStringAsync(url);
                var results = JsonSerializer.Deserialize<JsonElement[]>(response);

                if (results != null && results.Length > 0)
                {
                    var firstResult = results[0];
                    if (firstResult.TryGetProperty("lat", out var latElement) && 
                        firstResult.TryGetProperty("lon", out var lonElement))
                    {
                        if (double.TryParse(latElement.GetString(), out var latitude) &&
                            double.TryParse(lonElement.GetString(), out var longitude))
                        {
                            _logger.LogInformation("Géocodage réussi pour '{Address}': {Lat}, {Lon}", 
                                address, latitude, longitude);
                            return (latitude, longitude);
                        }
                    }
                }

                _logger.LogWarning("Aucun résultat trouvé pour l'adresse: {Address}", address);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du géocodage de l'adresse: {Address}", address);
                return null;
            }
        }

        /// <summary>
        /// Convertit des coordonnées GPS en adresse (géocodage inverse)
        /// </summary>
        /// <param name="latitude">Latitude</param>
        /// <param name="longitude">Longitude</param>
        /// <returns>Adresse formatée ou null si échec</returns>
        public async Task<string> ReverseGeocodeAsync(double latitude, double longitude)
        {
            try
            {
                var url = $"https://nominatim.openstreetmap.org/reverse?lat={latitude}&lon={longitude}&format=json";

                // Ajouter un User-Agent pour respecter les conditions d'utilisation
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "DiversityPub/1.0");

                var response = await _httpClient.GetStringAsync(url);
                var result = JsonSerializer.Deserialize<JsonElement>(response);

                if (result.TryGetProperty("display_name", out var displayNameElement))
                {
                    var address = displayNameElement.GetString();
                    _logger.LogInformation("Géocodage inverse réussi: {Address}", address);
                    return address;
                }

                _logger.LogWarning("Aucune adresse trouvée pour les coordonnées: {Lat}, {Lon}", latitude, longitude);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du géocodage inverse pour {Lat}, {Lon}", latitude, longitude);
                return null;
            }
        }
    }
} 