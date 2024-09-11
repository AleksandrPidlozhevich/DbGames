using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class NotionService
{
    private static readonly HttpClient client = new HttpClient();

    public NotionService(string notionToken)
    {
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", notionToken);
        client.DefaultRequestHeaders.Add("Notion-Version", "2022-06-28");
    }

    public async Task<Dictionary<int, string>> GetNotionGamesAsync(string databaseId)
    {
        string queryUrl = $"https://api.notion.com/v1/databases/{databaseId}/query";
        var notionGames = new Dictionary<int, string>();
        string startCursor = null;
        bool hasMore = true;

        while (hasMore)
        {
            var requestBody = new Dictionary<string, object>
            {
                ["page_size"] = 100
            };

            if (!string.IsNullOrEmpty(startCursor))
            {
                requestBody["start_cursor"] = startCursor;
            }

            var response = await client.PostAsync(
                queryUrl,
                new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json")
            );

            if (!response.IsSuccessStatusCode)
            {
                break;
            }

            var result = await response.Content.ReadAsStringAsync();
            dynamic pages = JsonConvert.DeserializeObject<dynamic>(result);

            foreach (var page in pages.results)
            {
                if (page.properties.GameID != null && page.properties.GameID.number != null &&
                    page.properties.Name != null && page.properties.Name.title.Count > 0)
                {
                    int gameId = (int)page.properties.GameID.number;
                    string name = page.properties.Name.title[0].text.content;
                    notionGames[gameId] = name;
                }
            }

            hasMore = pages.has_more;
            startCursor = hasMore ? pages.next_cursor : null;
        }

        return notionGames;
    }

    public async Task AddGameToNotionAsync(string databaseId, int gameId, string gameName)
    {
        var requestUrl = $"https://api.notion.com/v1/pages";
        var requestBody = new
        {
            parent = new { database_id = databaseId },
            properties = new
            {
                GameID = new { number = gameId },
                Name = new { title = new[] { new { text = new { content = gameName } } } }
            }
        };

        var requestContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
        var response = await client.PostAsync(requestUrl, requestContent);

        if (!response.IsSuccessStatusCode)
        {
            // TODO: add logging
        }
    }

    public async Task AddGamesToNotionAsync(string databaseId, Dictionary<int, string> steamGames, Dictionary<int, string> notionGames)
    {
        foreach (var game in steamGames)
        {
            if (!notionGames.ContainsKey(game.Key))
            {
                await AddGameToNotionAsync(databaseId, game.Key, game.Value);
            }
        }
    }
}
