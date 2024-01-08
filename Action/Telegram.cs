using System.Text;
using Newtonsoft.Json;

namespace EyeAuras.Web.Repl.Component;

public partial class Main
{
    private async Task TelegramMessage(string text)
    {
        try
        {
            var data = new TelegramData
            {
                License = "GFPA-4SEV-LC8Y-P5PA-F3GR",
                Username = _config.TelegramUsername,
                Call = false,
                Text = text,
                Product = "Telegram Calls API"
            };

            
            var json = JsonConvert.SerializeObject(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync("https://eyeapi.eyesquad.net/Call", content);
                
            }
        }
        catch (Exception ex)
        {
            Log.Warn($"Произошла ошибка: {ex.Message}");
        }
    }
}

public class TelegramData
{
    public string License { get; set; }
    public string Username { get; set; }
    public bool Call { get; set; }
    public string Text { get; set; }
    public string Product { get; set; }
}