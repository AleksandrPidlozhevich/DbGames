using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class SteamService
{
    private static readonly HttpClient client = new HttpClient();

    public async Task<Dictionary<int, string>> GetSteamGamesAsync(string steamApiKey, string steamId)
    {
        string url = $"http://api.steampowered.com/IPlayerService/GetOwnedGames/v0001/?key={steamApiKey}&steamid={steamId}&format=json&include_appinfo=true";
        var steamGames = new Dictionary<int, string>();

        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            // TODO: add logging
            return steamGames;
        }

        var result = await response.Content.ReadAsStringAsync();
        dynamic gamesList = JsonConvert.DeserializeObject<dynamic>(result);

        foreach (var game in gamesList.response.games)
        {
            string name = game.name;
            int appId = game.appid;
            steamGames[appId] = name;
        }

        return steamGames;
    }
}