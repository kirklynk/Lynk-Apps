using Microsoft.JSInterop;

namespace DMS.WebClient.Services
{
    public class SettingsService(IJSRuntime jSRuntime)
    {
        private const string SettingsKey = "dms_settings";
        private Settings _settings = new Settings();
        public async Task<Settings> GetSettingsAsync()
        {
            var settingsJson = await jSRuntime.InvokeAsync<string>("localStorageAccessor.getItem", SettingsKey);
            if (!string.IsNullOrEmpty(settingsJson))
            {
                _settings = System.Text.Json.JsonSerializer.Deserialize<Settings>(settingsJson) ?? new Settings();
            }
            return _settings;
        }
        public async Task SaveSettingsAsync(Settings settings)
        {
            _settings = settings;
            var settingsJson = System.Text.Json.JsonSerializer.Serialize(settings);
            await jSRuntime.InvokeVoidAsync("localStorageAccessor.setItem", SettingsKey, settingsJson);
        }
    }

    public class Settings
    {
        public bool IsDarkMode { get; set; } = false;
    }
}
