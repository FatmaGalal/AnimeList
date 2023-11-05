using AnimeCharactersMaui.State;
using Microsoft.VisualBasic;
using MvvmHelpers;
using StrawberryShake;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace AnimeCharactersMaui.ViewModels;

public class MainPageViewModel : BindableBase, INavigatedAware
{
    private int pageNumber = 249;
    private int pageSize = 20;
    private int? currentPage;
    private bool? hasNextPage;
    private ISemanticScreenReader _screenReader { get; }

    private ObservableRangeCollection<IGetAnimeList_Page_Characters> _animeCharactersList;

    public ObservableRangeCollection<IGetAnimeList_Page_Characters> AnimeCharactersList
    {
        get { return _animeCharactersList; }
        set { SetProperty(ref _animeCharactersList, value); }
    }

    private bool _isLoading;

    public bool IsLoading
    {
        get { return _isLoading; }
        set { SetProperty(ref _isLoading, value); }
    }

    public ICommand LoadMoreItemsCommand { get; set; }

    public MainPageViewModel(ISemanticScreenReader screenReader)
    {
        _screenReader = screenReader;

        LoadMoreItemsCommand = new DelegateCommand(LoadMoreItemsAsync);
        

    }

    private void LoadMoreItemsAsync()
    {
        if (IsLoading)
            return;

        IsLoading = true;
        if (AnimeCharactersList.Count > 0)
        {
            if (hasNextPage != null && hasNextPage == true)
            {
                //Task.Delay(2000);
                LoadAnimeList(pageSize, (int)currentPage + 1).ConfigureAwait(true);
            }
        }
        IsLoading = false;
    }

    public string Title => "Main Page";

    private async Task LoadAnimeList(int pageSize, int pageNumber)
    {

        var client = SetupServer();

        IOperationResult<IGetAnimeListResult> result = await client.GetAnimeList.ExecuteAsync(pageSize, pageNumber);

        if (result.IsErrorResult())
        {

        }
        else
        {
            IGetAnimeListResult res = result.Data;

            if (AnimeCharactersList == null || AnimeCharactersList.Count == 0)
                AnimeCharactersList = new ObservableRangeCollection<IGetAnimeList_Page_Characters>(res.Page.Characters);
            else
                AnimeCharactersList.AddRange(res.Page.Characters);

            currentPage = result.Data.Page.PageInfo.CurrentPage;
            hasNextPage = result.Data.Page.PageInfo.HasNextPage;
        }
    }

    private static IAnimeCharactersMaui SetupServer()
    {
        IServiceCollection serviceConnection = new ServiceCollection();
        serviceConnection.AddAnimeCharactersMaui().ConfigureHttpClient(c => c.BaseAddress = new Uri(Constants.ServerConstants.BaseURL));

        //serviceConnection.AddTeamGraphqlApp().ConfigureHttpClient(h => h.DefaultRequestHeaders.Add(EndPointsConstants.AdminSecretKey, EndPointsConstants.AdminSecretValue));
        IServiceProvider serviceProvider = serviceConnection.BuildServiceProvider();

        IAnimeCharactersMaui client = serviceProvider.GetRequiredService<IAnimeCharactersMaui>();
        return client;
    }

    public void OnNavigatedFrom(INavigationParameters parameters)
    {
        //throw new NotImplementedException();
    }

    public void OnNavigatedTo(INavigationParameters parameters)
    {
        LoadAnimeList(pageSize, pageNumber).ConfigureAwait(true);
    }
}
