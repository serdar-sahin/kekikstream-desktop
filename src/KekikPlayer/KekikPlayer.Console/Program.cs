
using KekikPlayer.Core.Extensions;
using KekikPlayer.Core.Services;
using System.Diagnostics;

var pythonService = new PythonService();

Console.WriteLine("KekikStream.Console");

if (!pythonService.CheckLocalPython())
{
    Console.WriteLine("Python.Engine not initialized!");
    return;
}

//var result = await pythonService.InstallPythonAsync();
//Debug.WriteLine("Install Python: " + result);

//var result = await pythonService.InstallKekikStream();
//Debug.WriteLine("Install Kekik: " + result);

//var result = await pythonService.UpdateKekikStream();
//Debug.WriteLine("Update Kekik: " + result);


pythonService.ConsoleTest();


//var pluginNames = pythonService.GetPluginNames();
//Console.WriteLine(pluginNames.ToJson());


//var plugins = pythonService.GetPlugins();
//foreach (var plugin in plugins)
//{
//    plugin.Icon = plugin.GetIcon();
//}
//Console.WriteLine(plugins.ToJson());


//var searcResults = pythonService.Search("FilmMakinesi", "matrix");
//var searcResults = pythonService.Search("DiziYou", "silo");
//var searcResults = pythonService.Search("SezonlukDizi", "silo");
//var searcResults = pythonService.Search("Dizilla", "silo");
//var searcResults = pythonService.Search("SineWix", "matrix");
//Console.WriteLine(searcResults?.ToJson());


//var searcAllResults = pythonService.SearchAll("matrix");
//Console.WriteLine(searcAllResults?.ToJson());


//var mediaInfo = pythonService.GetMediaInfo("FilmMakinesi", "https://filmmakinesi.de/film/matrix-resurrections-izle-2021-fm7/");
//var mediaInfo = pythonService.GetMediaInfo("DiziYou", "https://www.diziyou1.com/silo59/");
//var mediaInfo = pythonService.GetMediaInfo("Dizilla", "https://dizilla11.com/dizi/silo");
//Console.WriteLine(mediaInfo?.ToJson());


//var links = pythonService.GetVideoLinks("FilmMakinesi", "https://filmmakinesi.de/film/matrix-resurrections-izle-2021-fm7/");
//var links = pythonService.GetVideoLinks("DiziYou", "https://www.diziyou1.com/silo-1-sezon-1-bolum/");
//var links = pythonService.GetVideoLinks("Dizilla", "https://dizilla11.com/silo-1-sezon-3-bolum");
//var links = pythonService.GetVideoLinks("SezonlukDizi", "https://sezonlukdizi6.com/silo/1-sezon-1-bolum.html");
//Console.WriteLine(links?.ToJson());


//var sources = pythonService.GetVideoSources("FilmMakinesi", "https://closeload.filmmakinesi.de/video/embed/fgvP8tldQUR/");
//var sources = pythonService.GetVideoSources("DiziYou", "https://www.diziyou1.com/silo-1-sezon-1-bolum/");
//var sources = pythonService.GetVideoSources("Dizilla", "https://pichive.online/multiplayer.php?v=71f39bf37391a61ff0b4a3d12e6eac80");


//[{"PluginName":"DiziYou","Title":"Silo","Url":"https://www.diziyou1.com/silo59/","Poster":"https://cdn.diziyou1.com/wp-content/uploads/posters/silo.jpg"}]
//Task: True
//ShowMediaInfo: Silo
//Exception thrown: 'Python.Runtime.PythonException' in Python.Runtime.dll
//MediaInfo: { "Url":"https://www.diziyou1.com/silo59/","Poster":"https://cdn.diziyou1.com/wp-content/uploads/posters/silo.jpg","Title":"Silo","Description":"Dizi Amerikalı bilimkurgu yazarı Hugh Howey’in bilim-kurgu distopik drama serisi Wool'dan uyarlandı. Dünyamızın atmosferi toksik maddelerle kaplanıp, artık insanlar için yaşanamaz hâle gelir. İnsanların nüfusu 10bine kadar düşer ve insanlık yaşamını yerin altında yüzlerce kat uzanan dev bir siloda sürdürmek zorunda kalır. Öyle büyük bir yapı ki içinde kendi başkanı, emniyet güçleri ve yargısı bulunmakta ve binlerce insan zehirli atmosferden kendilerini koruyan tek şey olan bu Silo'nun içindeki garipliklere dikkat etmeye başlar. Silo'nun en alt katlarındaki jeneratörleriyle alakalı çalışan mühendis Juliette Nichols sevgilisinin ölümünden sonra yanıtlar arar ve gizli geçitlerden tehlikeli bölgelere geçer. Juliette'in bu arayışı Silo'yu tehlikeye mi sokacaktır?","Tags":"Bilim Kurgu, Dram, Fantazi","Rating":"8.1","Year":"2023","Duration":null,"Actors":"Rebecca Ferguson, Tim Robbins, Common, Harriet Walter, Avi Nash","Episodes":[{ "Season":1,"EpisodeNumber":1,"Title":"Freedom Day","Url":"https://www.diziyou1.com/silo-1-sezon-1-bolum/"},{ "Season":1,"EpisodeNumber":2,"Title":"Holston's Pick","Url":"https://www.diziyou1.com/silo-1-sezon-2-bolum/"},{ "Season":1,"EpisodeNumber":3,"Title":"Machines","Url":"https://www.diziyou1.com/silo-1-sezon-3-bolum/"},{ "Season":1,"EpisodeNumber":4,"Title":"Truth","Url":"https://www.diziyou1.com/silo-1-sezon-4-bolum/"},{ "Season":1,"EpisodeNumber":5,"Title":"The Janitor's Boy","Url":"https://www.diziyou1.com/silo-1-sezon-5-bolum/"},{ "Season":1,"EpisodeNumber":6,"Title":"The Relic","Url":"https://www.diziyou1.com/silo-1-sezon-6-bolum/"},{ "Season":1,"EpisodeNumber":7,"Title":"The Flamekeepers","Url":"https://www.diziyou1.com/silo-1-sezon-7-bolum/"},{ "Season":1,"EpisodeNumber":8,"Title":"Hanna","Url":"https://www.diziyou1.com/silo-1-sezon-8-bolum/"},{ "Season":1,"EpisodeNumber":9,"Title":"The Getaway","Url":"https://www.diziyou1.com/silo-1-sezon-9-bolum/"},{ "Season":1,"EpisodeNumber":10,"Title":"Outside","Url":"https://www.diziyou1.com/silo-1-sezon-10-bolum/"},{ "Season":2,"EpisodeNumber":1,"Title":"Mühendis","Url":"https://www.diziyou1.com/silo-2-sezon-1-bolum/"},{ "Season":2,"EpisodeNumber":2,"Title":"Düzen","Url":"https://www.diziyou1.com/silo-2-sezon-2-bolum/"},{ "Season":2,"EpisodeNumber":3,"Title":"Solo","Url":"https://www.diziyou1.com/silo-2-sezon-3-bolum/"},{ "Season":2,"EpisodeNumber":4,"Title":"Harmonyum","Url":"https://www.diziyou1.com/silo-2-sezon-4-bolum/"},{ "Season":2,"EpisodeNumber":5,"Title":"İniş","Url":"https://www.diziyou1.com/silo-2-sezon-5-bolum/"},{ "Season":2,"EpisodeNumber":6,"Title":"Barikatlar","Url":"https://www.diziyou1.com/silo-2-sezon-6-bolum/"},{ "Season":2,"EpisodeNumber":7,"Title":"Dalış","Url":"https://www.diziyou1.com/silo-2-sezon-7-bolum/"},{ "Season":2,"EpisodeNumber":8,"Title":"Quinn'in Kitabı","Url":"https://www.diziyou1.com/silo-2-sezon-8-bolum/"},{ "Season":2,"EpisodeNumber":9,"Title":"Safeguard","Url":"https://www.diziyou1.com/silo-2-sezon-9-bolum/"},{ "Season":2,"EpisodeNumber":10,"Title":"Ateşin İçine","Url":"https://www.diziyou1.com/silo-2-sezon-10-bolum/"}]}
//GetEpisodeVideoLinks
//CurrentEpisode: { "Season":1,"EpisodeNumber":1,"Title":"Freedom Day","Url":"https://www.diziyou1.com/silo-1-sezon-1-bolum/"}
//EpisodeVideoLinks: [{ "Name":null,"Url":"https://storage.diziyou1.com/episodes/72101/play.m3u8"}]
//VideoSources: [{"Name":"DiziYou | Orjinal Dil |  Silo 1. Sezon 1. Bölüm - Freedom Day","Url":"https://storage.diziyou1.com/episodes/72101/play.m3u8","Referer":"https://www.diziyou1.com/silo-1-sezon-1-bolum/","Headers":[],"Subtitles":[]}]
//var sources = pythonService.GetVideoSources("DiziYou", "https://storage.diziyou1.com/episodes/72101/play.m3u8");
//Console.WriteLine(sources?.ToJson());
