from .Core  import PluginManager, ExtractorManager, UIManager, MediaManager, PluginBase, ExtractorBase, SeriesInfo
import asyncio
from contextlib import suppress

pluginManager = PluginManager()
extManager = ExtractorManager()
selected_plugin: PluginBase = None

def get_plugin_names():
    return pluginManager.get_plugin_names()


def get_plugins():
    results = []
        
    for plugin_name, plugin in pluginManager.plugins.items():
            
            if not isinstance(plugin, PluginBase):
                continue

            if plugin_name in ["Shorten"]:
                continue

            # print(f"plugin_name: {plugin_name} plugin_url: {plugin.main_url}")

            try:
                info = {"name": plugin_name, "url": plugin.main_url}
                results.append(info)
                    
            except Exception as ex:
                print(f"error: {ex}")
                pass
                
    return results


def select_plugin(name):
    selected_plugin = pluginManager.select_plugin(name)
    return selected_plugin.name


def selected_plugin_info(name):
    selected_plugin = pluginManager.select_plugin(name)
    info = {"name": selected_plugin.name, "url": selected_plugin.main_url}
    return info


async def search_async(plugin_name, query) -> list:
    selected_plugin = pluginManager.select_plugin(plugin_name)
    results = await selected_plugin.search(query)
    return results


def search(plugin_name, query):
    selected_plugin = pluginManager.select_plugin(plugin_name)

    # try:
    #   loop = asyncio.get_running_loop()
    # except RuntimeError:
    #   loop = asyncio.new_event_loop()
    #   asyncio.set_event_loop(loop)
    
    # #results = loop.run_until_complete(selected_plugin.search(query)) 
    # #results = asyncio.run(selected_plugin.search(query))
    # # results = selected_plugin.search(query)
    # loop.close() 

    # for pythonnet async functions loop closed errors
    for _ in range(2):
            with suppress(Exception):
                return asyncio.run(selected_plugin.search(query))

    # results = asyncio.run(selected_plugin.search(query))
    # return results


def search_all(query):
    all_results = []
        
    for plugin_name, plugin in pluginManager.plugins.items():
            
            if not isinstance(plugin, PluginBase):
                continue

            if plugin_name in ["Shorten"]:
                continue

            try:
                results = []

                # for pythonnet async functions loop closed errors
                for _ in range(2):
                   with suppress(Exception):
                       results = asyncio.run(plugin.search(query))
                
                if results:
                    all_results.extend(
                        [
                            {
                                "plugin" : plugin_name,
                                "title"  : result.title,
                                "url"    : result.url,
                                "poster" : result.poster
                            }
                                for result in results
                        ]
                    )
            except Exception as ex:
                print(f"error: {ex}")
                pass
                
    return all_results


def get_media_info(plugin_name, url, count: int = 3):
        selected_plugin = pluginManager.select_plugin(plugin_name)

        # for pythonnet async functions loop closed errors
        for _ in range(count):
            with suppress(Exception):
                return asyncio.run(selected_plugin.load_item(url))

        return None


def get_video_links(plugin_name, url):
        selected_plugin = pluginManager.select_plugin(plugin_name)

        # for pythonnet async functions loop closed errors
        for _ in range(2):
            with suppress(Exception):
                 links =  asyncio.run(selected_plugin.load_links(url))
                 print(f"links: {links}")
                 return links
            
        
def get_video_sources(plugin_name, url):
     selected_plugin = pluginManager.select_plugin(plugin_name)
     
     if hasattr(selected_plugin, "play") and callable(getattr(selected_plugin, "play", None)):
        print("direct")
        return get_direct_video_sources(selected_plugin, url)
     else:
        print("extractor")
        return get_extractor_video_sources(selected_plugin, url)
     

def get_direct_video_sources(plugin, url):
        results = []

        data = plugin._data.get(url, {})
        print(f"data: {data}")
        source = {
            "name" : data.get("name"),
            "url"  : url,
            "referer" : data.get("referer"),
            "subtitles" : data.get("subtitles"),
            "headers" : data.get("headers")
        }

        results.append(source)

        return results

def get_extractor_video_sources(plugin, url):
        results = []

        ext: ExtractorBase = extManager.find_extractor(url)
        if not ext:
            print(f"no extractor")
            return results
        
        try:
            for _ in range(2):
               with suppress(Exception):
                   extract_data = asyncio.run(ext.extract(url, referer=plugin.main_url))
            
            print(f"extractor: {ext.name}")
            print(f"extract_data: {extract_data}")
        except Exception as ex:
            print(f"extractor: {ext.name} error: {ex}")
            return results

        if isinstance(extract_data, list):
            for data in extract_data:
                # print(f"data: {data}")
                source = {
                    "name" : data.name,
                    "url"  : data.url,
                    "referer" : data.referer,
                    "subtitles" : data.subtitles,
                    "headers" : data.headers
                }

                results.append(source)
        else:
            # print(f"extract_data: {extract_data}")
            source = {
                    "name" : extract_data.name,
                    "url"  : extract_data.url,
                    "referer" : extract_data.referer,
                    "subtitles" : extract_data.subtitles,
                    "headers" : extract_data.headers
            }

            results.append(source)

        return results

def close_loop():
    try:
      loop = asyncio.get_event_loop()
      loop.close()
    except RuntimeError: 
      print("loop error")