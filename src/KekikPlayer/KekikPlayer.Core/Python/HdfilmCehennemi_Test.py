# Bu araç @keyiflerolsun tarafýndan | @KekikAkademi için yazýlmýþtýr.

from KekikStream.Core import PluginBase, SearchResult, MovieInfo, ExtractResult, Subtitle
from Kekik.Sifreleme  import Packer, HexCodec
from parsel           import Selector
import re, base64, random, string

class HdFilmCehennemi(PluginBase):
    name     = "HdFilmCehennemi"
    main_url = "https://www.hdfilmcehennemi.nl"
    

    async def search(self, query: str) -> list[SearchResult]:
        istek = await self.oturum.get(
            url     = f"{self.main_url}/search?q={query}",
            headers = {"Referer": f"{self.main_url}/", "X-Requested-With": "fetch", "authority": f"{self.main_url}"}
        )
        
        secici = Selector(istek.text)

        results = []
        for veri in istek.json().get("results"):
            secici = Selector(veri)
            title  = secici.css("h4.title::text").get()
            href   = secici.css("a::attr(href)").get()
            poster = secici.css("img::attr(data-src)").get() or secici.css("img::attr(src)").get()
            # print(veri)
            # print(self.fix_url(href.strip()))

            if title and href:
                results.append(
                    SearchResult(
                        title  = title.strip(),
                        url    = self.fix_url(href.strip()),
                        poster = self.fix_url(poster.strip()) if poster else None,
                    )
                )
            
        return results

    async def load_item(self, url: str) -> MovieInfo:
        istek  = await self.oturum.get(url, headers = {"Referer": f"{self.main_url}/"})
        secici = Selector(istek.text)

        title       = secici.css("h1.section-title::text").get().strip()
        # print(title)
        poster      = secici.css("aside.post-info-poster img.lazyload::attr(data-src)").get().strip()
        # print(poster)
        description = secici.css("article.post-info-content > p::text").get().strip()
        # print(description)
        tags        = secici.css("div.post-info-genres a::text").getall()
        # print(tags)
        rating      = secici.css("div.post-info-imdb-rating span::text").get().strip()
        # print(rating)
        year        = secici.css("div.post-info-year-country a::text").get().strip()
        # print(year)
        actors      = secici.css("div.post-info-cast a > strong::text").getall()
        # print(actors)
        duration    = secici.css("div.post-info-duration::text").get().replace("dakika", "").strip()
        # print(duration)

        duration_minutes = 0
        try:
          duration_minutes = int(duration[2:-1])
        except Exception as ex:
          print(ex)
        
        print(duration_minutes)

        return MovieInfo(
            url         = url,
            poster      = self.fix_url(poster),
            title       = title,
            description = description,
            tags        = tags,
            rating      = rating,
            year        = year,
            actors      = actors,
            duration    = duration_minutes
        )
    
    def generate_random_cookie(self):
        return ''.join(random.choices(string.ascii_letters + string.digits, k=16))

    async def load_links_old(self, url: str) -> list[str]:
        istek  = await self.oturum.get(url)
        secici = Selector(istek.text)

        
        lang_code = secici.css("div.alternative-links::attr(data-lang)").get().upper()
        buttons = secici.css("div.alternative-links > button")
        # print(buttons)

        link_list = []
        
        for button in buttons:
            # print(button)
            source = button.css("button.alternative-link::text").get().replace("(HDrip Xbet)", "").strip() + " " + lang_code
            # print(source)
            video_id = button.css("button.alternative-link::attr(data-video)").get()
            # print(video_id)

            istek = await self.oturum.get(
             url     = f"{self.main_url}/video/{video_id}/",
             headers = {"Referer": f"{self.main_url}/", 
            "X-Requested-With": "fetch", 
            "authority": f"{self.main_url}"}
            )

            #print(istek.text)

            data = istek.json().get("data")
            # print(data["html"])
            
            secici = Selector(data["html"])
            iframe_url = secici.css("iframe::attr(src)").get() or secici.css("iframe::attr(data-src)").get()

            if "?rapidrame_id=" in iframe_url:
                iframe_url = f"{self.main_url}/playerr/" + iframe_url.split("?rapidrame_id=")[1]

           
            iframe_url = self.fix_url(iframe_url)
            link_list.append(iframe_url)
            print(f"HDCH {source} » {video_id} » {iframe_url}")

            response = await self.oturum.get(
                url =iframe_url, 
                headers = {"Referer": f"{self.main_url}/", 
                "X-Requested-With": "fetch", 
                "authority": f"{self.main_url}"}
            )
            script = None

            # print(response.text)

            for element in response.text.split("script"):
                # print(element)
                if "sources:" in element:
                    script = element
                    print(script)
                    break

            if not script:
              continue

            packed_url = script.split('file_link"')[1].split('";')[0]
            print("unpack url: " + packed_url)
            encoded_url = Packer.unpack(packed_url)
            decoded_url = base64.base64Decode(encoded_url) 
            print(decoded_url)
            sub_data = script.split("tracks: [")[1].split("]")[0]

            self._data[self.fix_url(decoded_url)] = {
                "ext_name"  : self.name,
                "name"      : f"{source} | {self.name}",
                "referer"   : f"{self.main_url}/",
                "subtitles" : []
            }
           

        #iframe_src = secici.css("div.video-container iframe::attr(src)").get() or secici.css("div.video-container iframe::attr(data-src)").get()
        
        return list(self._data.keys())
        #return link_list if link_list else []
        #return [self.fix_url(iframe_url)] if iframe_url else []

    async def load_links(self, url: str) -> list[str]:
        istek  = await self.oturum.get(url)
        secici = Selector(istek.text)

        lang_code = secici.css("div.alternative-links::attr(data-lang)").get().upper()
        buttons = secici.css("div.alternative-links > button")
        # print(buttons)

        link_list = []
        
        for button in buttons:
            # print(button)
            source = button.css("button.alternative-link::text").get().replace("(HDrip Xbet)", "").strip() + " " + lang_code
            # print(source)
            video_id = button.css("button.alternative-link::attr(data-video)").get()
            print(video_id)

            istek = await self.oturum.get(
                url     = f"{self.main_url}/video/{video_id}/",
                headers = {"Referer": f"{self.main_url}/", 
                "X-Requested-With": "fetch", 
                "authority": f"{self.main_url}"}
            )

            data = istek.json().get("data")
            secici = Selector(data["html"])
            iframe_url = secici.css("iframe::attr(src)").get() or secici.css("iframe::attr(data-src)").get()
            
            if "?rapidrame_id=" in iframe_url:
                # https://hdfilmcehennemi.mobi/video/embed/uQcCR9nhaNz/?rapidrame_id=j4b4kvc0s24l\
                video_id = iframe_url.split('=')[1]
            else:
                # https://www.hdfilmcehennemi.nl/rplayer/j4b4kvc0s24l/
                video_id = iframe_url.split('/')[-1]
            
            print(video_id)
            if(video_id):
               break

        # selected_quality: low
        random_cookie = self.generate_random_cookie()
        istek = await self.oturum.post(
                url     = f"https://cehennempass.pw/process_quality_selection.php",
                headers = {"Referer": f"https://cehennempass.pw/download/{video_id}", 
                "X-Requested-With": "fetch", 
                "authority": "cehennempass.pw",
                'Cookie': f'PHPSESSID={random_cookie}'},
                data    = {"video_id": video_id, "selected_quality": "low"},
                )

        video_url = istek.json().get("download_link")
        print(video_url)

        self._data[self.fix_url(video_url)] = {
            "ext_name"  : self.name,
            "name"      : "Düþük Kalite",
            "referer"   : f"https://cehennempass.pw/download/{video_id}",
            "subtitles" : []
        }

        # selected_quality: high
        random_cookie = self.generate_random_cookie()
        istek = await self.oturum.post(
                url     = f"https://cehennempass.pw/process_quality_selection.php",
                headers = {"Referer": f"https://cehennempass.pw/download/{video_id}", 
                "X-Requested-With": "fetch", 
                "authority": "cehennempass.pw",
                'Cookie': f'PHPSESSID={random_cookie}'},
                data    = {"video_id": video_id, "selected_quality": "high"},
                )

        video_url = istek.json().get("download_link")
        print(video_url)

        self._data[self.fix_url(video_url)] = {
            "ext_name"  : self.name,
            "name"      : "Yüksek Kalite",
            "referer"   : f"https://cehennempass.pw/download/{video_id}",
            "subtitles" : []
        }
           
        return list(self._data.keys())
        #return link_list if link_list else []
        #return [self.fix_url(iframe_url)] if iframe_url else []


    async def play(self, name: str, url: str, referer: str, subtitles: list[Subtitle]):
        extract_result = ExtractResult(name=name, url=url, referer=referer, subtitles=subtitles)
        self.media_handler.title = name
        if self.name not in self.media_handler.title:
            self.media_handler.title = f"{self.name} | {self.media_handler.title}"

        self.media_handler.play_media(extract_result)