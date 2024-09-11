using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DbGames.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        [ObservableProperty]
        private string steamApiKey;

        [ObservableProperty]
        private string steamId;

        [ObservableProperty]
        private string notionToken;

        [ObservableProperty]
        private string databaseId;

        private readonly SteamService _steamService;

        public MainWindowViewModel()
        {
            _steamService = new SteamService();
        }

        [RelayCommand]
        public async Task ExportToNotionAsync()
        {
            var _notionService = new NotionService(notionToken);
            // TODO: add logging
            Dictionary<int, string> steamGames = await _steamService.GetSteamGamesAsync(steamApiKey, steamId);
            Dictionary<int, string> notionGames = await _notionService.GetNotionGamesAsync(databaseId);
            await _notionService.AddGamesToNotionAsync(databaseId, steamGames, notionGames);
        }
    }
}
