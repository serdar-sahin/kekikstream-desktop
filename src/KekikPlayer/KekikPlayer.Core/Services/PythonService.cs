using Python.Runtime;
using Python.Included;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Input;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using System.Collections;
using KekikPlayer.Core.Models;
using Avalonia.OpenGL;
using System.Reflection;

namespace KekikPlayer.Core.Services
{
    public class PythonService: IDisposable
    {
        //string rootPath = AppContext.BaseDirectory; //_hostEnvironment.ContentRootPath;
        
        public PythonService() 
        {
            //CheckGlobalPython();

            //if (CheckLocalPython())
            //{
            //    //InitPython();
            //    //GetPluginNames();
            //    //SelectPlugin("DiziBox");
            //    //SelectedPluginInfo("DiziBox");
            //    //Search("Dizilla", "silo");
            //    //SearchTest("FilmMakinesi", "matrix");
            //    //SearchAll("matrix");
            //    //GetMediaInfo("FilmMakinesi", "https://filmmakinesi.de/film/matrix-resurrections-izle-2021-fm7/");

            //}
            //else
            //{
            //    //InstallPython();
            //}
        }

        private bool CheckGlobalPython()
        {
            try
            {
                string pythonHome = Environment.GetEnvironmentVariable("PYTHONHOME") ?? string.Empty;
                Debug.WriteLine(pythonHome);
                if (string.IsNullOrEmpty(pythonHome))
                {
                    return false;
                }


                string path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
                Debug.WriteLine(path);
                if (string.IsNullOrEmpty(path))
                {
                    return false;
                }
                else
                {
                    // Python yolunu PATH içinde arayın
                    string[] pathDirs = path.Split(';');
                    foreach (var dir in pathDirs)
                    {
                        if (dir.Contains("python"))
                        {
                            Debug.WriteLine(dir);
                            break;
                        }
                    }
                }

                PythonEngine.Initialize();

                dynamic sys = Py.Import("sys");
                string version = (string)sys.version;

                PythonEngine.Shutdown();

                Debug.WriteLine("### Python version:\n\t" + version);
                if (!string.IsNullOrEmpty(version))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }

            return false;
        }

        public bool CheckLocalPython()
        {
            try
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string appFolder = Path.Combine(appDataPath, "kekik");
                if (!Directory.Exists(appFolder))
                {
                    Debug.WriteLine("appFolder is not exist");
                    return false;
                }

                //string kekikFolder = rootPath + @"\Lib\site-packages\KekikStream";
                //if (!Directory.Exists(kekikFolder))
                //{
                //    Debug.WriteLine("kekikFolder is not exist");
                //    return false;
                //}

                Installer.InstallPath = appFolder;
                Debug.WriteLine("appFolder: " + appFolder);
                Debug.WriteLine("EmbeddedPythonHome: " + Installer.EmbeddedPythonHome);
                
                string pythonDllPath = Path.Combine(Installer.EmbeddedPythonHome, "python311.dll");

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    pythonDllPath = Path.Combine(Installer.EmbeddedPythonHome, "python311.dll");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    pythonDllPath = Path.Combine(Installer.EmbeddedPythonHome, "libpython3.11.so");
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    pythonDllPath = Path.Combine(Installer.EmbeddedPythonHome, "libpython3.11.dylib");
                }
                else
                {
                    Debug.WriteLine("unknown platform");
                    return false;
                }

                Runtime.PythonDLL = pythonDllPath;

                PythonEngine.Initialize();

                dynamic sys = Py.Import("sys");
                string version = (string)sys.version;

                //AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization", true);
                //PythonEngine.Shutdown();
                //AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization", false);

                Debug.WriteLine("Python version: " + version);
                if (!string.IsNullOrEmpty(version))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }

            return false;
        }

        private void Initialize()
        {
            PythonEngine.Initialize();
            string rootPath = Installer.EmbeddedPythonHome;
            dynamic sys = Py.Import("sys");
            sys.path.append(rootPath + @"\Lib\site-packages\KekikStream");
            Debug.WriteLine("PythonEngine.Initialize");
        }

        private async void InitPython()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "kekik");

            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            Installer.InstallPath = appFolder;
            Installer.LogMessage += Console.WriteLine;

            string kekikPath = Path.Combine(Installer.EmbeddedPythonHome, "kekikstream");

            // download and install kekikstream from the internet
            await Installer.PipInstallModule("KekikStream");
            Debug.WriteLine(kekikPath);


            // https://github.com/henon/Python.Included/issues/52
            string pythonDllPath = Path.Combine(Installer.EmbeddedPythonHome, "python311.dll");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                pythonDllPath = Path.Combine(Installer.EmbeddedPythonHome, "python311.dll");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                pythonDllPath = Path.Combine(Installer.EmbeddedPythonHome, "libpython3.11.so");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                pythonDllPath = Path.Combine(Installer.EmbeddedPythonHome, "libpython3.11.dylib");
            }
            else
            {
                Console.WriteLine("unknown platform");
            }

            Runtime.PythonDLL = pythonDllPath;

            InstallKekikRequirements();
            //TestPython();

            //AppContext.SetSwitch("System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization", true);
            //PythonEngine.Initialize();

            //Debug.WriteLine(pythonPath);
            //Debug.WriteLine(pythonDllPath);
        }

        public async Task<bool> InstallPythonAsync()
        {
            // https://www.python.org/ftp/python/3.14.0/

            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "kekik");
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            try
            {
                // app data paths
                // windows: C: \Users\< UserName >\AppData\Roaming
                // macOS: / Users /< UserName >/ Library / Application Support
                // linux: / home /< UserName >/.config

                // set the download source
                //Python.Deployment.Installer.Source = new Python.Deployment.Installer.DownloadInstallationSource()
                //{
                //    DownloadUrl = @"https://www.python.org/ftp/python/3.14.0/python-3.14.0-embed-amd64.zip",
                //};

                // install in local app data of user account
                Installer.InstallPath = appFolder;

                // install in local directory
                //Installer.InstallPath = Path.GetFullPath(".");

                // see what the installer is doing
                Installer.LogMessage += Log;

                // install from the given source
                await Installer.SetupPython();
                Debug.WriteLine("Python Ok");

                // install pip3 for package installation
                //Debug.WriteLine("check pip");
                //if (!Installer.IsPipInstalled())
                //{
                Debug.WriteLine("installing Pip");
                await Installer.InstallPip();
                Debug.WriteLine("Pip Ok");
                //}

                // download and install kekikstream from the internet
                //Debug.WriteLine("check Kekik");
                //if (!Installer.IsModuleInstalled("KekikStream"))
                //{
                Debug.WriteLine("installing Kekik");
                await Installer.PipInstallModule("KekikStream");
                Debug.WriteLine("KekikStream Ok");
                //}

                // copy embed kekik.py to path
                string kekikPath = Installer.EmbeddedPythonHome + @"\Lib\site-packages\KekikStream";
                if (!Directory.Exists(kekikPath))
                {
                    return false;
                }

                var assembly = Assembly.GetExecutingAssembly();
                string kekikPy = "kekik.py";
                //string resourceName = $"{assembly.GetName().Name}.{resourceName}";
                string resourceName = "KekikPlayer.Core.Python.kekik.py";
                Debug.WriteLine(resourceName);

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        Debug.WriteLine("Resource not found: " + resourceName);
                        return false;
                    }

                    string outputPath = Path.Combine(kekikPath, kekikPy);

                    using (FileStream fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                    {
                        stream.CopyTo(fs);
                    }

                    Debug.WriteLine("kekik.py Ok");
                }

                //TestPython();

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }

            return false;
        }

        public bool InstallPython()
        {
            // https://www.python.org/ftp/python/3.14.0/

            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "kekik");
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }

            try
            {
                // app data paths
                // windows: C: \Users\< UserName >\AppData\Roaming
                // macOS: / Users /< UserName >/ Library / Application Support
                // linux: / home /< UserName >/.config

                // set the download source
                //Python.Deployment.Installer.Source = new Python.Deployment.Installer.DownloadInstallationSource()
                //{
                //    DownloadUrl = @"https://www.python.org/ftp/python/3.14.0/python-3.14.0-embed-amd64.zip",
                //};

                // install in local app data of user account
                Installer.InstallPath = appFolder;

                // install in local directory
                //Installer.InstallPath = Path.GetFullPath(".");

                // see what the installer is doing
                Installer.LogMessage += Log;

                // install from the given source
                //await Installer.SetupPython();
                var task1 = Installer.SetupPython(); //.ConfigureAwait(false);
                task1.Wait();
                Debug.WriteLine("Python Ok");

                // install pip3 for package installation
                //Debug.WriteLine("check pip");
                //if (!Installer.IsPipInstalled())
                //{
                Debug.WriteLine("installing Pip");
                //await Installer.InstallPip();
                var task2 = Installer.InstallPip(); //.ConfigureAwait(false);
                task2.Wait();
                Debug.WriteLine("Pip Ok");
                //}

                // download and install kekikstream from the internet
                //Debug.WriteLine("check Kekik");
                //if (!Installer.IsModuleInstalled("KekikStream"))
                //{
                Debug.WriteLine("installing Kekik");
                //await Installer.PipInstallModule("KekikStream");
                var task3 = Installer.PipInstallModule("KekikStream"); //.ConfigureAwait(false);
                task3.Wait();
                Debug.WriteLine("KekikStream Ok");
                //}

                // copy embed kekik.py to path
                string kekikPath = Installer.EmbeddedPythonHome + @"\Lib\site-packages\KekikStream";
                if (!Directory.Exists(kekikPath))
                {
                    return false;
                }

                var assembly = Assembly.GetExecutingAssembly();
                string kekikPy = "kekik.py";
                //string resourceName = $"{assembly.GetName().Name}.{resourceName}";
                string resourceName = "KekikPlayer.Core.Python.kekik.py";
                Debug.WriteLine(resourceName);

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        Debug.WriteLine("Resource not found: " + resourceName);
                        return false;
                    }

                    string outputPath = Path.Combine(kekikPath, kekikPy);

                    using (FileStream fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                    {
                        stream.CopyTo(fs);
                    }

                    Debug.WriteLine("kekik.py Ok");
                }

                //TestPython();

                return true;
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }

            return false;
        }



        public string GetCurrentVersion()
        {
            if (!PythonEngine.IsInitialized)
            {
                Initialize();
            }

            using (Py.GIL())
            {
                dynamic kekik = Py.Import("KekikStream.kekik");

                if (kekik != null)
                {
                    dynamic version = kekik.get_current_version("KekikStream");

                    string currentVersion = (string)version.ToString();

                    return currentVersion;
                }
            }

            return "0";
        }

        public bool CheckNewVersion()
        {
            if (!PythonEngine.IsInitialized)
            {
                Initialize();
            }

            using (Py.GIL())
            {
                dynamic kekik = Py.Import("KekikStream.kekik");

                if (kekik != null)
                {
                    dynamic pyresult = kekik.check_new_version("KekikStream");

                    bool result = (bool)pyresult;

                    return result;
                }
            }

            return false;
        }

        public async Task<bool> InstallKekikStream()
        {
            try
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string appFolder = Path.Combine(appDataPath, "kekik");
                Installer.InstallPath = appFolder;
                Installer.LogMessage += Log;

                // install pip3 for package installation
                if (!Installer.IsPipInstalled())
                {
                    await Installer.TryInstallPip();
                    Debug.WriteLine("Pip Ok");
                }

                // download and install kekikstream from the pip
                if (!Installer.IsModuleInstalled("KekikStream"))
                {
                    await Installer.PipInstallModule("KekikStream");
                    Debug.WriteLine("KekikStream Ok");
                }

                // copy embed kekik.py to path
                string kekikPath = Installer.EmbeddedPythonHome + @"\Lib\site-packages\KekikStream";
                if (!Directory.Exists(kekikPath))
                {
                    return false;
                }

                var assembly = Assembly.GetExecutingAssembly();
                string kekikPy = "kekik.py";
                //string resourceName = $"{assembly.GetName().Name}.{resourceName}";
                string resourceName = "KekikPlayer.Core.Python.kekik.py";
                Debug.WriteLine(resourceName);

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        Debug.WriteLine("Resource not found: " + resourceName);
                        return false;
                    }

                    string outputPath = Path.Combine(kekikPath, kekikPy);

                    using (FileStream fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                    {
                        stream.CopyTo(fs);
                    }

                    Debug.WriteLine("kekik.py Ok");
                }

                return true;

           }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }

            return false;
        }

        public async Task<bool> UpdateKekikStream()
        {
            bool result = false;

            try
            {
                string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string appFolder = Path.Combine(appDataPath, "kekik");
                Installer.InstallPath = appFolder;
                Installer.LogMessage += Log;

                // install pip3 for package installation
                if (!Installer.IsPipInstalled())
                {
                    await Installer.TryInstallPip();
                    Debug.WriteLine("Pip Ok");
                }

                // delete old one and download and install kekikstream from the internet
                //await Installer.PipInstallModule("KekikStream");
                //await Installer.PipInstallModule("KekikStream", "1.1.3", true);

                // update kekikstream from the pip
                string pipPath = Path.Combine(Installer.EmbeddedPythonHome, "Scripts", "pip");
                string moduleName = "KekikStream";

                Python.Deployment.Installer.PythonDirectoryName = Installer.EmbeddedPythonHome;
                Python.Deployment.Installer.LogMessage += (log) =>
                {
                    if (log.Contains("Successfully installed KekikStream"))
                    {
                        //Debug.WriteLine(log);
                        result = true;
                    }
                };

                Python.Deployment.Installer.RunCommand($"\"{pipPath}\" install -U \"{moduleName}\" ");

                Debug.WriteLine("KekikStream Ok");
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }

            return result;
        }

        private async void InstallKekikRequirements()
        {
            await Installer.PipInstallModule("setuptools");
            await Installer.PipInstallModule("wheel");
            await Installer.PipInstallModule("Kekik");
            await Installer.PipInstallModule("httpx[http2]");
            await Installer.PipInstallModule("cloudscraper");
            await Installer.PipInstallModule("parsel");
            await Installer.PipInstallModule("pydantic");
            await Installer.PipInstallModule("InquirerPy");
            Debug.WriteLine("InstallKekikRequirements Ok");
        }
        
        public void GetResources()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceNames = assembly.GetManifestResourceNames();

            Debug.WriteLine("Embedded Resources:");
            foreach (var name in resourceNames)
            {
                Debug.WriteLine(name);
            }
        }

        
        public void ConsoleTest()
        {
            if (!PythonEngine.IsInitialized)
            {
                Initialize();
            }

            using (Py.GIL())
            {
                dynamic kekik = Py.Import("KekikStream.__main__");

                if (kekik != null)
                {
                    dynamic main = kekik.basla();
                }
            }
        }


        private void GetPluginNamesTest()
        {
            if (!PythonEngine.IsInitialized)
            {
               Initialize();
            }

            using (Py.GIL())
            {
                dynamic kekik = Py.Import("KekikStream.kekik");

                if (kekik != null)
                {
                    dynamic pluginNames = kekik.get_plugin_names();

                    foreach (var name in pluginNames)
                    {
                        Debug.WriteLine((string)name.ToString());
                    }
                }
            }
        }

        public List<Plugin>? GetPluginNames()
        {
            try
            {
                if (!PythonEngine.IsInitialized)
                {
                    Initialize();
                }

                var pluginList = new List<Plugin>();

                using (Py.GIL())
                {
                    dynamic kekik = Py.Import("KekikStream.kekik");

                    if (kekik != null)
                    {
                        dynamic pluginNames = kekik.get_plugin_names();

                        foreach (var name in pluginNames)
                        {
                            //Debug.WriteLine((string)name.ToString());
                            var plugin = new Plugin
                            {
                                Name = name.ToString(),
                            };

                            pluginList.Add(plugin);
                        }
                    }
                }

                return pluginList;
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }

            return null;
        }

        public List<Plugin>? GetPlugins()
        {
            try
            {
                if (!PythonEngine.IsInitialized)
                {
                    Initialize();
                }

                var pluginList = new List<Plugin>();

                using (Py.GIL())
                {
                    dynamic kekik = Py.Import("KekikStream.kekik");

                    if (kekik != null)
                    {
                        dynamic plugins = kekik.get_plugins();

                        // dict{string,string}
                        foreach (var item in plugins)
                        {
                            try
                            {
                                //Debug.WriteLine((string)item.ToString());
                                var plugin = new Plugin
                                {
                                    Name = item["name"].ToString(),
                                    Url = item["url"].ToString(),
                                };

                                pluginList.Add(plugin);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine(ex.ToString());
                                continue;
                            }
                        }
                    }
                }

                return pluginList;
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }

           return null;
        }

        public void SelectPlugin(string name)
        {
            if (!PythonEngine.IsInitialized)
            {
                Initialize();
            }

            using (Py.GIL())
            {
                dynamic kekik = Py.Import("KekikStream.kekik");

                if (kekik != null)
                {
                    // string
                    dynamic selectedPlugin = kekik.select_plugin(name);
                    Debug.WriteLine((string)selectedPlugin.ToString());
                }
            }
        }

        public void SelectedPluginInfo(string name)
        {
            if (!PythonEngine.IsInitialized)
            {
                Initialize();
            }

            using (Py.GIL())
            {
                dynamic kekik = Py.Import("KekikStream.kekik");

                if (kekik != null)
                {
                    // dict
                    dynamic selectedPlugin = kekik.selected_plugin_info(name);
                    Debug.WriteLine((string)selectedPlugin["name"].ToString());
                    Debug.WriteLine((string)selectedPlugin["url"].ToString());
                }
            }
        }


        private void SearchTest(string pluginName, string query)
        {
            if (!PythonEngine.IsInitialized)
            {
                Initialize();
            }

            using (Py.GIL())
            {
                dynamic kekik = Py.Import("KekikStream.kekik");

                dynamic results = kekik.search(pluginName, query);

                //Debug.WriteLine((string)results[0]["title"]);

                // list<class>
                foreach (dynamic item in results)
                {
                    Debug.WriteLine((string)item.title.ToString());
                    Debug.WriteLine((string)item.url.ToString());
                    Debug.WriteLine((string)item.poster?.ToString());

                    //var searchResult = new SearchResult
                    //{
                    //    Title = item.title.ToString(),
                    //    Url = item.url.ToString(),
                    //    Poster = item.poster?.ToString() 
                    //};
                }
            }
        }

        public List<SearchResult>? Search(string pluginName, string query)
        {
            try
            {
                if (!PythonEngine.IsInitialized)
                {
                    Initialize();
                }

                var searchList = new List<SearchResult>();

                using (Py.GIL())
                {
                    dynamic kekik = Py.Import("KekikStream.kekik");

                    dynamic results = kekik.search(pluginName, query);

                    // list<class>
                    foreach (dynamic item in results)
                    {
                        try
                        {
                            var result = new SearchResult
                            {
                                PluginName = pluginName,
                                Title = (string)item.title.ToString(),
                                Url = (string)item.url.ToString(),
                                Poster = (string?)item.poster?.ToString()
                            };

                            searchList.Add(result);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                            continue;
                        }
                    }
                }

                return searchList;
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }

            return null;
        }

        public async Task<List<SearchResult>?> SearchAsync(string pluginName, string query)
        {
            try
            {
                if (!PythonEngine.IsInitialized)
                {
                    Initialize();
                }

                await Task.Delay(0);

                var searchList = new List<SearchResult>();

                using (Py.GIL())
                {
                    dynamic kekik = Py.Import("KekikStream.kekik");

                    // test for async functions loop closed errors
                    //dynamic asyncio = Py.Import("asyncio");
                    //dynamic loop = asyncio.new_event_loop();
                    //asyncio.set_event_loop(loop);

                    dynamic results = kekik.search(pluginName, query);

                    // list<class>
                    foreach (dynamic item in results)
                    {
                        try
                        {
                            var result = new SearchResult
                            {
                                PluginName = pluginName,
                                Title = (string)item.title.ToString(),
                                Url = (string)item.url.ToString(),
                                Poster = (string?)item.poster?.ToString()
                            };

                            searchList.Add(result);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                            continue;
                        }
                    }

                    kekik.close_loop();
                }

                //PythonEngine.Shutdown();

                return searchList;
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }

            return null;
        }

        private void SearchAllTest(string query)
        {
            if (!PythonEngine.IsInitialized)
            {
                Initialize();
            }

            using (Py.GIL())
            {
                dynamic kekik = Py.Import("KekikStream.kekik");

                dynamic results = kekik.search_all(query);

                //Debug.WriteLine((string)results[0]["title"]);

                // list<class>
                foreach (dynamic item in results)
                {
                    try
                    {
                        //Debug.WriteLine((string)item.plugin.ToString());
                        Debug.WriteLine((string)item.title.ToString());
                        Debug.WriteLine((string)item.url.ToString());
                        Debug.WriteLine((string)item.poster?.ToString());

                        //var searchResult = new SearchResult
                        //{
                        //    Title = item.title.ToString(),
                        //    Url = item.url.ToString(),
                        //    Poster = item.poster?.ToString() 
                        //};
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                        continue;
                    }
                }
            }
        }

        public List<SearchResult>? SearchAll(string query)
        {
            try
            {
                if (!PythonEngine.IsInitialized)
                {
                    Initialize();
                }

                var searchList = new List<SearchResult>();

                using (Py.GIL())
                {
                    dynamic kekik = Py.Import("KekikStream.kekik");

                    dynamic results = kekik.search_all(query);

                    // dict<class>
                    foreach (dynamic item in results)
                    {
                        try
                        {
                            var result = new SearchResult
                            {
                                PluginName = (string)item["plugin"].ToString(),
                                Title = (string)item["title"].ToString(),
                                Url = (string)item["url"].ToString(),
                                Poster = (string?)item["poster"]?.ToString()
                            };

                            searchList.Add(result);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                            continue;
                        }
                    }
                }
               
                return searchList;
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }

            return null;
        }

        public async Task<List<SearchResult>?> SearchAllAsync(string query)
        {
            Debug.WriteLine("Python SearchAllAsync Started");

            try
            {
                if (!PythonEngine.IsInitialized)
                {
                    Initialize();
                }

                await Task.Delay(0);

                var searchList = new List<SearchResult>();

                using (Py.GIL())
                {
                    dynamic kekik = Py.Import("KekikStream.kekik");

                    dynamic results = kekik.search_all(query);

                    // dict<class>
                    foreach (dynamic item in results)
                    {
                        try
                        {
                            var result = new SearchResult
                            {
                                PluginName = (string)item["plugin"].ToString(),
                                Title = (string)item["title"].ToString(),
                                Url = (string)item["url"].ToString(),
                                Poster = (string?)item["poster"]?.ToString()
                            };

                            searchList.Add(result);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                            continue;
                        }
                    }
                }

                return searchList;
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }

            return null;
        }


        private void GetMediaInfoTest(string pluginName, string url)
        {
            // https://filmmakinesi.de/film/matrix-resurrections-izle-2021-fm7/

            if (!PythonEngine.IsInitialized)
            {
                Initialize();
            }

            using (Py.GIL())
            {
                dynamic kekik = Py.Import("KekikStream.kekik");

                dynamic result = kekik.get_media_info(pluginName, url);

                Debug.WriteLine((string)result.title.ToString());
                Debug.WriteLine((string)result.url.ToString());
                Debug.WriteLine((string)result.description.ToString());
                Debug.WriteLine((string)result.poster.ToString());
                Debug.WriteLine((string)result.tags.ToString());
                Debug.WriteLine((string)result.rating.ToString());
                Debug.WriteLine((string)result.year.ToString());
                Debug.WriteLine((string)result.duration.ToString());
                Debug.WriteLine((string)result.actors.ToString());
            }
        }

        public MediaInfo? GetMediaInfo(string pluginName, string url)
        {
            try
            {
                if (!PythonEngine.IsInitialized)
                {
                    Initialize();
                }

                var info = new MediaInfo();

                using (Py.GIL())
                {
                    dynamic kekik = Py.Import("KekikStream.kekik");

                    // class
                    dynamic? result = kekik.get_media_info(pluginName, url);

                    info.Title = (string?)result?.title?.ToString();
                    info.Url = (string?)result?.url?.ToString();
                    info.Description = (string?)result?.description?.ToString();
                    info.Poster = (string?)result?.poster?.ToString();
                    info.Tags = (string?)result?.tags?.ToString();
                    info.Rating = (string?)result?.rating?.ToString();
                    info.Year = (string?)result?.year?.ToString();
                    info.Actors = (string?)result?.actors?.ToString();

                    try
                    {
                        info.Duration = (string?)result?.duration?.ToString();
                    }
                    catch { }

                    info.Episodes = new List<Episode>();

                    try
                    {
                        dynamic? episodes = result?.episodes;
                        if(episodes != null)
                        {
                            foreach (var episode in episodes)
                            {
                                info.Episodes.Add(new Episode
                                {
                                    EpisodeNumber = episode.episode,
                                    Season = episode.season,
                                    Title = episode.title,
                                    Url = episode.url,
                                });
                            }
                        }
                    }
                    catch { }
                }

                return info;
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }

            return null;
        }

        private void GetVideoLinksTest()
        {
            // https://filmmakinesi.de/film/matrix-resurrections-izle-2021-fm7/

            if (!PythonEngine.IsInitialized)
            {
                Initialize();
            }

            using (Py.GIL())
            {
                dynamic kekik = Py.Import("KekikStream.kekik");

                dynamic results = kekik.search_all();

                //Debug.WriteLine((string)results[0]["title"]);

                // list<class>
                foreach (dynamic item in results)
                {
                    try
                    {
                        //Debug.WriteLine((string)item.plugin.ToString());

                    }
                    catch (Exception ex)
                    {
                        Debug.Write(ex.ToString());
                        continue;
                    }
                }
            }
        }

        public List<VideoLink>? GetVideoLinks(string pluginName, string url)
        {
            try
            {
                if (!PythonEngine.IsInitialized)
                {
                    Initialize();
                }

                var links = new List<VideoLink>();

                using (Py.GIL())
                {
                    dynamic kekik = Py.Import("KekikStream.kekik");

                    dynamic result = kekik.get_video_links(pluginName, url);

                    // list<str>
                    foreach (var item in result)
                    {
                        links.Add(new VideoLink
                        {
                            Url = item,
                        });
                    }
                }

                return links;
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }

            return null;
        }

        public List<VideoSource>? GetVideoSources(string pluginName, string url)
        {
            try
            {
                if (!PythonEngine.IsInitialized)
                {
                    Initialize();
                }

                var sources = new List<VideoSource>();

                using (Py.GIL())
                {
                    dynamic kekik = Py.Import("KekikStream.kekik");

                    dynamic result = kekik.get_video_sources(pluginName, url);

                    // list<dict{video_link}>
                    foreach (var item in result)
                    {
                        var source = new VideoSource();
                        source.Name = item["name"];
                        source.Url = item["url"];
                        source.Referer = item["referer"];
                        source.Subtitles = new List<Subtitle>();
                        source.Headers = new List<Generic>();

                        // []
                        try
                        {
                            dynamic? subtitles = item["subtitles"];
                            //if(subtitles != null)
                            if (Object.ReferenceEquals(null, subtitles))
                            {
                                foreach (var subtitle in subtitles)
                                {
                                    var sub = new Subtitle()
                                    {
                                        Name = (string)subtitle.name.ToString(),
                                        Url = (string)subtitle.url.ToString()
                                    };

                                    source.Subtitles.Add(sub);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                        }

                        // {}
                        try
                        {
                            dynamic? headers = item["headers"];
                            //if (headers != null)
                            if (Object.ReferenceEquals(null, headers))
                                {
                                foreach (var header in headers)
                                {
                                    var head = new Generic()
                                    {
                                        Item = (string)header.ToString()
                                    };

                                    source.Headers.Add(header);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString());
                        }

                        sources.Add(source);
                    }
                }

                return sources;
            }
            catch (Exception ex)
            {
                Log(ex.ToString());
            }

            return null;
        }


        private void ModuleTest1()
        {
            if (!PythonEngine.IsInitialized)
            {
                PythonEngine.Initialize();
            }

            //Task.Run(() => {
            //    using (Py.GIL())
            //    {
            //        dynamic test1 = Py.Import("test");
            //    }
            //}).Wait();

            using (Py.GIL())
            {
                string rootPath = Installer.EmbeddedPythonHome;
                dynamic sys = Py.Import("sys");
                sys.path.append(rootPath + @"\Lib\site-packages\KekikStream");

                //sys.path.append(rootPath + @"\Lib\site-packages\KekikStream\Core");
                //sys.path.append(rootPath + @"\Lib\site-packages\KekikStream\Core\Plugin");
                //sys.path.append(rootPath + @"\Lib\site-packages\KekikStream\Extractors");
                //sys.path.append(rootPath + @"\Lib\site-packages\KekikStream\Plugins");
                //sys.path.append(rootPath + @"\Lib\site-packages\KekikStream\Core\Plugin");

                //from.Core import PluginManager, ExtractorManager, UIManager, MediaManager, PluginBase, ExtractorBase, SeriesInfo
                //dynamic pluginLoader = Py.Import("Core.Plugin.PluginLoader");
                //dynamic pluginBase = Py.Import("Core.Plugin.PluginBase");
                //dynamic pluginManager = Py.Import("Core.Plugin.PluginManager");
                //dynamic pluginModels = Py.Import("Core.Plugin.PluginModels");

                //dynamic extManager = Py.Import("Core.ExtractorManager");
                //dynamic mediaManager = Py.Import("Core.MediaManager");
                //dynamic extBase = Py.Import("Core.ExtractorBase");
                //dynamic seriesInfo = Py.Import("Core.SeriesInfo");

                dynamic test1 = Py.Import("test");
                Debug.WriteLine((string)test1.get_version());

                //dynamic test2 = Py.Import("test.PluginManager");
                dynamic pluginManager = test1.PluginManager();
                dynamic pluginNames = pluginManager.get_plugin_names();

                //PyObject pyObjectType = pluginNames.GetType();
                //Console.WriteLine($"pluginNames türü: {pyObjectType}");

                //dynamic typeOfPluginNames = PythonEngine.RunSimpleString("type(pluginNames)").ToString();
                //dynamic typeOfPluginNames = PythonEngine.Exec($@"type(pluginNames)");
                //Debug.WriteLine($"pluginNames türü: {typeOfPluginNames}");

                //IEnumerable pluginNamesEnumerable = (IEnumerable)pluginNames;
                //PyList pyList = pluginNames.As<PyList>();
                //PyDict pyDict = pluginNames.As<PyDict>();
                //PyTuple pyTuple = pluginNames.As<PyTuple>();

                //dynamic pluginManager = Py.Import("Core.Plugin.PluginManager");
                //dynamic pluginManager = Py.Import("Plugin.PluginManager");
                //dynamic pluginNames = pluginManager.get_plugin_names();

                //foreach (PyObject key in pluginNames.Keys())
                //{
                //    Console.WriteLine($"Key: {key}, Value: {pluginNames[key]}");
                //}

                //List<string> pluginNames = (test1.PluginManager.get_plugin_names as PyObject).As<List<string>>();

                //IEnumerable<PyObject> resultsEnumerable = results.As<IEnumerable<PyObject>>();
                //var list = results.As<IList<PyObject>>();

                foreach (var name in pluginNames)
                {
                    Debug.WriteLine((string)name.ToString());
                }


            }
        }

        private void ModuleTest2()
        {
            // https://pythonnet.github.io/pythonnet/reference.html#function-Python.Runtime.PyObject.FromManagedObject
            // https://github.com/pythonnet/pythonnet/issues/2415

            // ok, now use pythonnet from that installation
            if (!PythonEngine.IsInitialized)
            {
                PythonEngine.Initialize();
            }

            // call Python's sys.version to prove we are executing the right version
            dynamic sys = Py.Import("sys");
            string version = (string)sys.version;

            Debug.WriteLine("### Python version:\n\t" + version);

            // call os.getcwd() to prove we are executing the locally installed embedded python distribution
            dynamic os = Py.Import("os");
            string cwd = (string)os.getcwd();

            Debug.WriteLine("### Current working directory:\n\t" + cwd);
            Debug.WriteLine("### PythonPath:\n\t" + PythonEngine.PythonPath);
        }

        private PyObject ConvertToPyObject(dynamic value)
        {
            if (value == null)
                return PyObject.None;
            else if (value is string)
                return new PyString(value);
            else if (value is int)
                return new PyInt(value);
            else if (value is bool)
                return PyObject.FromManagedObject(value);
            else
                return PyObject.FromManagedObject(value);
        }


        // todo: add logger
        private void Log(string message)
        {
            Debug.WriteLine(message);

            //try
            //{

            //}
            //catch (Exception ex)
            //{
            //    Log(ex.ToString());
            //}
        }

        public void Dispose()
        {
            PythonEngine.Shutdown();
        }
    }
}
