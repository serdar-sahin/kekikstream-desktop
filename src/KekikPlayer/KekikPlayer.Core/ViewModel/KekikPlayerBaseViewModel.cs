using CommunityToolkit.Mvvm.ComponentModel;
using KekikPlayer.Core.Services;
using KekikPlayer.Core.Model;
using LibMpv.MVVM;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.Input;
using KekikPlayer.Core.Models;
using System.Diagnostics;
using System.ComponentModel.DataAnnotations;
using KekikPlayer.Core.Extensions;
using System.Xml.Linq;
using Avalonia.Threading;
using Newtonsoft.Json.Linq;
using LibMpv.Client;
using LibMpv.Avalonia;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia;
using Avalonia.Controls;
using MsBox.Avalonia.Dto;
using System.Net.Mail;
using MsBox.Avalonia.Models;
using System.Runtime.InteropServices;

namespace KekikPlayer.Core.ViewModel;

public abstract partial class KekikPlayerBaseViewModel: BaseMpvContextViewModel
{
    [ObservableProperty] private bool isPlayListVisible = false;
    
    [ObservableProperty] private bool isSearchResultListVisible = false;
    
    [ObservableProperty] private bool isPluginListVisible = true;
    
    [ObservableProperty] private bool isSettigsVisible = false;

    [ObservableProperty] private bool isSearchVisible = false;

    [ObservableProperty] private bool isSideBarVisible = true;

    [ObservableProperty] private bool isMediaInfoVisible = false;

    [ObservableProperty] private bool isEpisodes = false;

    [ObservableProperty] private bool isBusy = false;

    [ObservableProperty] private Plugin? currentPlugin;

    [ObservableProperty] private SearchResult? currentSearchResult;

    [ObservableProperty] private MediaInfo? currentMediaInfo;

    [ObservableProperty] private Episode? currentEpisode;

    [ObservableProperty] private VideoLink? currentVideoLink;

    [ObservableProperty] private VideoSource? currentVideSource;

    [ObservableProperty] private string currentPluginName = "Seçili Eklenti Yok";

    [ObservableProperty] private string searchQuery;

    [ObservableProperty] private bool searchAll = false;

    [ObservableProperty] private string playListSource;

    [ObservableProperty] private bool isError = false;

    [ObservableProperty] private string errorMessage;

    public ObservableCollection<SearchResult> SearchResults { get; } = new();
    public ObservableCollection<Plugin> PluginGroups { get; } = new();
    public ObservableCollection<VideoLink> VideoLinks { get; } = new();
    public ObservableCollection<VideoSource> VideoSources { get; } = new();

    //public IRelayCommand<Plugin> ShowSearch { get; }

    private List<Plugin>? plugins;

    private PythonService pythonService;

    public KekikPlayerBaseViewModel()
    {

        InitializePython();

        //ShowSearch = new RelayCommand<Plugin>((plugin) =>
        //{
        //    Debug.WriteLine($"Plugin name: {plugin.Name}");
        //});

        PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(CurrentPlugin))
            {
                //Debug.WriteLine($"Changed: {CurrentPlugin?.Name}");
                CurrentPluginName = CurrentPlugin is null ? "Seçili Eklenti Yok" : CurrentPlugin.Name;
                //OnPropertyChanged(nameof(currentPluginName));
            }

            if (args.PropertyName == nameof(CurrentMediaInfo))
            {
                IsEpisodes = (CurrentMediaInfo?.Episodes != null && CurrentMediaInfo?.Episodes.Count > 0 ) ? true : false;
            }

            if (args.PropertyName == nameof(PlayerState))
            {
                if(PlayerState == PlayerState.Error)
                {
                    if (SearchResults != null)
                    {
                        IsSearchResultListVisible = false;
                        ShowSearchResultlList();
                    }

                    ShowMessage("Video oynatılamıyor!");
                }
            }

            //if (args.PropertyName == nameof(TrackList))
            //{
            //    Debug.WriteLine("track-list"); // show_text ${track-list} 
            //    this.Command("show_text ${track-list}");
            //}
        };
    }

    public async void InitializePython()
    {
        pythonService = new PythonService();
        if (pythonService.CheckLocalPython())
        {
            InitializeModel();
        }
        else
        {
            IsError = true;
            ErrorMessage = "KekikStream.Desktop" 
                + Environment.NewLine + Environment.NewLine 
                + "KekikStream.Python kuruluyor..."
                + Environment.NewLine
                + "İnternet hızınıza göre işlem süresi değişebilir, bekleyiniz.";
            await Task.Delay(1000);

            //var result = pythonService.InstallPython();
            //var result = await pythonService.InstallPythonAsync().ConfigureAwait(false);
            var result = await Task.Run(async () => { return await pythonService.InstallPythonAsync(); });

            if (result)
            {
                if (pythonService.CheckLocalPython())
                {
                    ErrorMessage = "";
                    IsError = false;

                    InitializeModel();
                }
                else
                {
                    IsError = true;
                    ErrorMessage = "Kurulumlar başarısız!";
                }
            }
            else
            {
                IsError = true;
                ErrorMessage = "Kurulumlar başarısız!";
            }
        }
    }

    public void InitializeModel()
    {
        if(pythonService != null)
        {
            plugins = pythonService.GetPlugins();

            InvokeInUIThread(() =>
            {
                SearchResults.Clear();
                PluginGroups.Clear();

                //CurrentMediaInfo = new MediaInfo()
                //{
                //    Title = "",
                //    Url = "",
                //    Description = "",
                //    Poster = "",
                //    Year = "",
                //    Rating = ""
                //};

                if(plugins != null)
                {
                    foreach (var plugin in plugins)
                    {
                        plugin.Icon = plugin.GetIcon();
                        PluginGroups.Add(plugin);
                    }
                }
            });
        }

       //ShowVideoSourceDialog();
       //PlayVideoTest();
    }

    
    [RelayCommand]
    private async void Search()
    {
        IsBusy = true;
        ////OnPropertyChanged(nameof(IsBusy));
        
        // todo: frozen loading indicator
        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await Task.Delay(100);
            //var b = SearchFunc();
            //var b = await Task.Factory.StartNew(SearchTask);
            //var b = await Task.Run(SearchFunc);
            var b = await SearchAsync();
            //bool b = await Task.Run(async () => await SearchAsync());
            //bool b = await Task.Run(() => SearchAsync());
          
            Debug.WriteLine("Task: " + b);

            CloseSearch();

            if (SearchResults != null && SearchResults.Count > 0)
            {
                IsPluginListVisible = false;
                IsSearchResultListVisible = true;
                IsSideBarVisible = true;
            }

            await Task.Delay(100);
        });

        
        IsBusy = false;
        //OnPropertyChanged(nameof(IsBusy));
    }

    private async Task<bool> SearchAsync()
    {
        SearchResults.Clear();
        
        if (string.IsNullOrEmpty(SearchQuery))
        {
            CloseSearch();
            return false; //Task.CompletedTask;
        }

        //var taskResult = await Task.Run(() =>
        //{
        if (SearchAll)
        {
            Debug.WriteLine("SearchAll");
            var results = await pythonService.SearchAllAsync(SearchQuery);
            //var results = await Task.Run(async () => { return await pythonService.SearchAllAsync(SearchQuery); });

            if (results != null)
            {
                Debug.WriteLine(results.ToJson());
                foreach (var result in results)
                {
                    SearchResults.Add(result);
                }
            }
        }
        else
        {
            Debug.WriteLine("Search");
            if (CurrentPlugin != null)
            {
                var results = await pythonService.SearchAsync(CurrentPlugin.Name, SearchQuery);
                //var results = await Task.Run(async () => { return await pythonService.SearchAsync(CurrentPlugin.Name, SearchQuery); });

                if (results != null)
                {
                    Debug.WriteLine(results.ToJson());
                    foreach (var result in results)
                    {
                        SearchResults.Add(result);
                    }
                }
            }
        }

        //    return true;
        //});

        //Debug.WriteLine("Task: " + taskResult);

        return true; //Task.CompletedTask;
    }

    private bool SearchFunc()
    {
        SearchResults.Clear();

        if (string.IsNullOrEmpty(SearchQuery))
        {
            CloseSearch();
            return false;
        }

        //var taskResult = await Task.Run(() =>
        //{
        if (SearchAll)
        {
            var results = pythonService.SearchAll(SearchQuery);
            if (results != null)
            {
                Debug.WriteLine(results.ToJson());
                foreach (var result in results)
                {
                    SearchResults.Add(result);
                }
            }
        }
        else
        {
            if (CurrentPlugin != null)
            {
                var results = pythonService.Search(CurrentPlugin.Name, SearchQuery);
                if (results != null)
                {
                    Debug.WriteLine(results.ToJson());
                    foreach (var result in results)
                    {
                        SearchResults.Add(result);
                    }
                }
            }
        }

        //    return true;
        //});

        //Debug.WriteLine("Task: " + taskResult);

        return true;
    }

    private async Task<bool> TaskTest()
    {
        await Task.Delay(10000);
        return true;
    }

    [RelayCommand]
    private void ShowSearch(Plugin plugin)
    {
        Debug.WriteLine("ShowSearch: " + plugin.Name);
        CurrentPlugin = plugin;
        IsSearchVisible = true;
    }

    
    [RelayCommand]
    private void ShowMediaInfo(SearchResult result)
    {
        Debug.WriteLine("ShowMediaInfo: " + result.Title);
        IsBusy = true;
        CurrentSearchResult = result;
        CurrentMediaInfo = null;
        VideoLinks.Clear();

        if (CurrentPlugin != null)
        {
            CurrentMediaInfo = pythonService.GetMediaInfo(CurrentPlugin.Name, result.Url);
            if(CurrentMediaInfo != null)
            {
                Debug.WriteLine("MediaInfo: " + CurrentMediaInfo.ToJson());

                //// series
                //if(CurrentMediaInfo.Episodes != null && CurrentMediaInfo.Episodes.Count > 0)
                //{
                //    // todo
                //}
                //else
                //{
                //    // movie
                //    //VideoLinks = pythonService.GetVideoLinks(CurrentPlugin.Name, CurrentMediaInfo.Url);
                //    GetMovieVideoLinks();
                //}

                IsBusy = false;
                IsMediaInfoVisible = true; 
            }
        }
    }

    [RelayCommand]
    private void CloseMediaInfo()
    {
        Debug.WriteLine("CloseMediaInfo");
        IsMediaInfoVisible = false;
        CurrentMediaInfo = null;
        CurrentVideoLink = null;
        //VideoLinks.Clear();
        CurrentVideSource = null;
        //VideoSources?.Clear();
    }

    [RelayCommand]
    private void GetMovieVideoLinks()
    {
        Debug.WriteLine("GetMovieVideoLinks");
        VideoLinks.Clear();

        if (CurrentPlugin != null && CurrentMediaInfo != null)
        {
            var links = pythonService.GetVideoLinks(CurrentPlugin.Name, CurrentMediaInfo.Url);
            if(links != null)
            {
                foreach (var link in links)
                {
                    VideoLinks.Add(link);
                }
            }

            Debug.WriteLine("MovieVideoLinks: " + VideoLinks.ToJson());

            GetVideoSources();
        }
    }

    [RelayCommand]
    private void GetEpisodeVideoLinks(Episode episode)
    {
        Debug.WriteLine("GetEpisodeVideoLinks");
        CurrentEpisode = episode;
        VideoLinks.Clear();

        if (CurrentPlugin != null && CurrentEpisode != null)
        {
            Debug.WriteLine("CurrentEpisode: " + CurrentEpisode.ToJson());

            var links = pythonService.GetVideoLinks(CurrentPlugin.Name, CurrentEpisode.Url);
            if (links != null)
            {
                foreach (var link in links)
                {
                    VideoLinks.Add(link);
                }
            }

            Debug.WriteLine("EpisodeVideoLinks: " + VideoLinks.ToJson());

            GetVideoSources();
        }
    }

    [RelayCommand]
    private async void GetVideoSources()
    {
         Debug.WriteLine("GetVideoSources");
        VideoSources.Clear();

        if (CurrentPlugin != null)
        {
            IsBusy = true;
            await Task.Delay(100);

            foreach (var link in VideoLinks)
            {
                var sources = pythonService.GetVideoSources(CurrentPlugin.Name, link.Url);
                if (sources != null)
                {
                    foreach (var source in sources)
                    {
                        VideoSources?.Add(source);
                    }
                }
            }

            if (VideoSources != null && VideoSources.Count > 0)
            {
                // show sources dialog
                if (VideoSources.Count > 1)
                {
                    IsBusy = false;
                    CloseMediaInfo();

                    var source = await ShowVideoSourceDialog(); //.Result;
                    if (source != null)
                    {
                        PlayVideoSource(source);

                        IsSideBarVisible = false;
                        IsSearchVisible = false;
                    }
                }
                else
                {
                    // direct play from videosource
                    var source = VideoSources.FirstOrDefault();
                    if (source != null)
                    {
                        PlayVideoSource(source);

                        IsSideBarVisible = false;
                        IsSearchVisible = false;

                        CloseMediaInfo();
                        IsBusy = false;
                    }
                }
            }
            else
            {
                IsBusy = false;
                ShowMessage("Video oynatılamıyor!");

                //todo: direct play from videolink
                //PlayVideoLink(VideoLinks?.FirstOrDefault().Url);

                //IsSideBarVisible = false;
                //IsSearchVisible = false;

                //CloseMediaInfo();

                //IsBusy = false;
            }
        }

        Debug.WriteLine("VideoSources: " + VideoSources?.ToJson());
    }

    private void GetVideoSourcesX(VideoLink link)
    {
        Debug.WriteLine("GetVideoSources");
        VideoSources.Clear();
        
        CurrentVideoLink = link;

        if (CurrentPlugin != null && CurrentVideoLink != null)
        {
            var sources = pythonService.GetVideoSources(CurrentPlugin.Name, CurrentVideoLink.Url);
            if (sources != null)
            {
                foreach (var source in sources)
                {
                    VideoSources.Add(source);
                }
            }

            if (VideoSources != null && VideoSources.Count > 0)
            {
                if(VideoSources.Count > 1)
                {
                    ShowVideoSourceDialog();
                }
                else
                {
                    var source = VideoSources.FirstOrDefault();
                    if (source != null)
                    {
                        PlayVideoSource(source);
                    }
                }
            }
            else
            {
                PlayVideoLink(link.Url);
            }

            Debug.WriteLine("VideoSources: " + VideoSources?.ToJson());
            IsSideBarVisible = false;
            IsSearchVisible = false;

            CloseMediaInfo();
        }
    }

    private void PlayVideoLink(string url)
    {
        this.Stop();
        
        this.LoadFile(url);
        this.Play();
    }

    private void PlayVideoSource (VideoSource value)
    {
        this.Stop();
        if (value != null)
        {
            if(!string.IsNullOrEmpty(value.Referer))
            {
                this.SetOptionString("referrer", value.Referer);
            }

            if (value.Headers != null && value.Headers.Count > 0)
            {
                this.SetOptionString("referrer", value.Referer);
            }

            this.LoadFile(value.Url);
            this.Play();
        }

        IsSideBarVisible = false;
    }

    private void PlayVideoTest()
    {
        // https://www.diziyou1.com/silo-1-sezon-1-bolum/
        // VideoSources: [{"Name":"DiziYou | Orjinal Dil |  Silo 1. Sezon 1. Bölüm - Freedom Day","Url":"https://storage.diziyou1.com/episodes/72101/play.m3u8","Referer":"https://www.diziyou1.com/silo-1-sezon-1-bolum/","Headers":[],"Subtitles":[{"Name":"Türkçe Altyazılı","Url":"https://storage.diziyou1.com/subtitles/72101/tr.vtt"},{"Name":"İngilizce Altyazılı","Url":"https://storage.diziyou1.com/subtitles/72101/en.vtt"}]}]
        
        
        // VideoSources: [{"Name":"VideoSeyred","Url":"https://sithmanifest.com/enCoder/fromdusktilldawn/1/1/manifest.m3u8","Referer":"https://videoseyred.in","Headers":[],"Subtitles":[{"Name":"Türkçe","Url":"https://videoseyred.in/wp-content/uploads/10977_265758_1564275165.vtt"},{"Name":"İngilizce","Url":"https://videoseyred.in/wp-content/uploads/10977_265758_1564274943.vtt"}]}]

        Debug.WriteLine("PlayVideoTest");

        this.Stop();

        // referrer ok
        //this.SetOptionString("referrer", "https://www.diziyou1.com/silo-1-sezon-1-bolum/");
        this.SetOptionString("referrer", "https://videoseyred.in");

        // headers?
        // mpv --http-header-fields="User-Agent: Mozilla/5.0 (X11; Linux x86_64; rv:83.0) Gecko/20100101 Firefox/83.0" <url>"
        // mpv --http-header-fields='Field1: value1','Field2: value2'

        // subtitle
        // mpv --sub-file=subtitle.srt video.mkv
        //this.SetOptionString("sub-file", "https://storage.diziyou1.com/subtitles/72101/tr.vtt");

        // separated by : (Unix) or ; (Windows)
        //this.SetOptionString("sub-files", "['https://storage.diziyou1.com/subtitles/72101/tr.vtt'; 'https://storage.diziyou1.com/subtitles/72101/en.vtt']");
        //this.SetPropertyLong("sid", 1);

        //this.Command("show-text", "Freedom Day", "10000");
        this.Command("show-text", "Test Media", "10000");

        // test for audio video tracks
        //this.Command("show_text ${track-list}"); (give error?)
        //this.Command("show_text", "'${track-list}'");

        //this.LoadFile("https://storage.diziyou1.com/episodes/72101/play.m3u8");
        this.LoadFile("https://sithmanifest.com/enCoder/fromdusktilldawn/1/1/manifest.m3u8");

        this.Play();

        // subtitles ok but libmpv host problem. mpv player opens in a separate window?
        // https://github.com/mOrfiUs/VTTmanager (c# vtt download)
        //this.Command("sub-add", "https://storage.diziyou1.com/subtitles/72101/tr.vtt");
        //this.Command("sub-add", "https://videoseyred.in/wp-content/uploads/10977_265758_1564275165.vtt");
        //this.Command("sub-add", @"C:\Projeler\Medialonia\kekikstream\Media\test_tr.vtt");
    }


    [RelayCommand]
    private void StopVideo()
    {
        this.Stop();

        // todo: for last picture 

        // does not affect
        //this.SetPropertyString("keep-open", "no");
        //this.SetPropertyFlag("keep-open", false);

        // give errors?
        //this.Command("seek", "0", "absolute");
        //this.Command("set", "osd", "no");
        //this.Command("flush");

        //this.SetPropertyString("keep-open", "no");
        //this.SetPropertyString("vid", "no");
        //this.Command("set", "no-osd", "yes");
        //this.Command("set", "osd-bar", "no");
        //this.Command("seek", "0", "absolute");

        if (SearchResults != null)
        {
            IsSearchResultListVisible = false;
            ShowSearchResultlList();
        }
    }


    [RelayCommand]
    private void ToggleSearchVisibility()
    {
        IsSearchVisible = !IsSearchVisible;

        if (IsSearchVisible)
            IsPlayListVisible = false;
    }

    [RelayCommand]
    private void CloseSearch()
    {
        IsSearchVisible = false;
        SearchQuery = string.Empty;
    }


    [RelayCommand]
    private void CloseSettings()
    {
        IsSettigsVisible = false;
    }

    [RelayCommand]
    private void ShowPluginList()
    {
        //if (SearchResults != null)
        //{
        //    ShowSearchResultlList();
        //    return;
        //}

        IsPluginListVisible = !IsPluginListVisible;
        IsSideBarVisible = IsPluginListVisible;

        if (IsPluginListVisible)
        {
            IsSearchResultListVisible = false;
            IsPluginListVisible = true;
        }
    }

    [RelayCommand]
    private void ShowSearchResultlList()
    {
        IsSearchResultListVisible = !IsSearchResultListVisible;
        IsSideBarVisible = IsSearchResultListVisible;

        if (IsSearchResultListVisible)
        {
            IsPluginListVisible = false;
            IsSearchResultListVisible = true;
        }
    }

    [RelayCommand]
    private void ToggleSettingsVisibility()
    {
        IsSettigsVisible = !IsSettigsVisible;

        if (IsSettigsVisible)
            IsPlayListVisible = false;
    }

    [RelayCommand]
    private void SaveSettings()
    {
        if (!String.IsNullOrEmpty(playListSource))
        {
            SettingsService.Instance.SetPlayList(playListSource);
        }
        IsSettigsVisible = false;
        InitializeModel();
    }

    private async void ShowMessage(string message)
    {
        // https://github.com/AvaloniaCommunity/MessageBox.Avalonia/blob/master/Exmaple2.0/Views/MainWindow.axaml.cs

        int maxWidth = 300;
        int maxHeight = 200;

        //var box = MessageBoxManager
        //    .GetMessageBoxStandard("KekikStream.Desktop", message,
        //        ButtonEnum.Ok);

        var box = MessageBoxManager.GetMessageBoxStandard(
            new MessageBoxStandardParams
            {
                ButtonDefinitions = ButtonEnum.Ok,
                ContentTitle = "KekikStream.Desktop",
                //ContentHeader = header,
                ContentMessage = message,
                Icon = MsBox.Avalonia.Enums.Icon.Warning,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                MaxWidth = maxWidth,
                MaxHeight = maxHeight,
                SizeToContent = SizeToContent.WidthAndHeight,
                ShowInCenter = true,
                Topmost = true,
                SystemDecorations = SystemDecorations.None,
            });

        var result = await box.ShowWindowAsync();
        //if(result == ButtonResult.Ok){}
        //var result = await box.ShowAsPopupAsync(this);
    }

    private async Task<VideoSource?> ShowVideoSourceDialog()
    {
        var buttonDefinitions = new List<ButtonDefinition>();

        int indx = 1;

        foreach (var source in VideoSources)
        {

            string name = source.Name;

            if (string.IsNullOrEmpty(name))
            {
                //name = $"Kaynak {indx}";
                continue;
            }
            else
            {
                if (name.Length >= 20)
                {
                    name = name.Substring(0, 20);
                }
            }
                
            //var buttonDefinition = new ButtonDefinition { Name = string.IsNullOrEmpty(name)? $"Kaynak {indx}": source.Name.Substring(1, 20) };
            //var buttonDefinition = new ButtonDefinition { Name = $"Kaynak {indx}"};
            var buttonDefinition = new ButtonDefinition { Name = name};
            buttonDefinitions.Add(buttonDefinition);
            indx++;
        }

        buttonDefinitions.Add(new ButtonDefinition { Name = "İptal" });

        var box = MessageBoxManager.GetMessageBoxCustom(
           new MessageBoxCustomParams
           {
               ButtonDefinitions = buttonDefinitions,
               //ContentTitle = "title",
               ContentMessage = "İzlemek için bir video kaynağı seçiniz",
               Icon = MsBox.Avalonia.Enums.Icon.Question,
               WindowStartupLocation = WindowStartupLocation.CenterOwner,
               CanResize = false,
               Width = 400,
               Height = 150,
               SizeToContent = SizeToContent.WidthAndHeight,
               ShowInCenter = true,
               Topmost = true,
               SystemDecorations = SystemDecorations.None,
               HyperLinkParams = new HyperLinkParams
               {
                   Text = "",
                    Action = () => 
                    { 

                    }
               }
           });

        var result = await box.ShowWindowAsync();
        if (result == "İptal")
        {
            return null;
        }

        foreach (var source in VideoSources)
        {
            if (source.Name.Contains(result))
            {
                return source;
            }
        }

        return null;
    }
}
