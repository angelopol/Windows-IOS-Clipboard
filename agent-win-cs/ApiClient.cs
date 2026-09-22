using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ClipboardAgent
{
    public class ApiClient
    {
        private readonly HttpClient _http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        private readonly string _server;

        public event Action Unauthorized;

        public ApiClient(string server, string token)
        {
            _server = (server ?? "").TrimEnd('/');
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        // Devuelve true si NO hay que reintentar (éxito o error de cliente);
        // false solo si hubo error de red (sin conexión).
        public async Task<bool> PostClipAsync(string text)
        {
            try
            {
                var json = JsonSerializer.Serialize(new { text });
                using var body = new StringContent(json, Encoding.UTF8, "application/json");
                using var resp = await _http.PostAsync(_server + "/api/clip", body);
                if (resp.StatusCode == HttpStatusCode.Unauthorized) Unauthorized?.Invoke();
                return true;
            }
            catch (HttpRequestException) { return false; }
            catch (TaskCanceledException) { return false; }
        }

        public async Task<List<ClipItem>> GetClipsAsync(string scope = "shared")
        {
            try
            {
                using var resp = await _http.GetAsync(_server + "/api/clips?scope=" + scope);
                if (resp.StatusCode == HttpStatusCode.Unauthorized) { Unauthorized?.Invoke(); return null; }
                if (!resp.IsSuccessStatusCode) return null;

                var json = await resp.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                var list = new List<ClipItem>();
                if (doc.RootElement.TryGetProperty("clips", out var clips)
                    && clips.ValueKind == JsonValueKind.Array)
                {
                    foreach (var el in clips.EnumerateArray())
                    {
                        var item = new ClipItem();
                        if (el.TryGetProperty("id", out var id) && id.ValueKind == JsonValueKind.Number)
                            item.Id = id.GetInt64();
                        if (el.TryGetProperty("text", out var t)) item.Text = t.GetString();
                        if (el.TryGetProperty("created_at", out var c)) item.CreatedAt = c.GetString();
                        list.Add(item);
                    }
                }
                return list;
            }
            catch { return null; }
        }
    }
}
