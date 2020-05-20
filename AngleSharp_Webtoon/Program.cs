using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace Webtoon
{
    class Program
    {
        static string URL = String.Empty;
        static string URL_Chaps = String.Empty;
        static string URL_Eps = String.Empty;

        static string Path = String.Empty;

        static string[] Links_chapter;
        static string[] Names_chapter;
        static void Main(string[] args)
        {
            /*
            Console.Write(" Введите откуда вы хотите скачать главы : \r\n" +
                              " 1) Webtoon; \n" +
                              " 2) Readmanga; \n" +
                              " 3) Mangalib; \n" +
                              " 4) Remanga. \n");
            //int Choise = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine();
            */

            int Choise = 1;

            switch (Choise)
            {
                case 1:
                    {
                        Console.Write(" Введите URL Вебтуна главы которого вы хотите скачать : ");
                        URL = Console.ReadLine();
                        Console.WriteLine();
                        Chap_Names(URL);

                    Excep:
                        Console.Write("\n С какой по какую главу вы хотите скачать ([начало] - [конец]) : ");
                        string[] Eps = Console.ReadLine().Split('-');
                        for (int i = 0; i < Eps.Length; i++)
                            Eps[i].Trim();

                        int Eps_Beg = Convert.ToInt32(Eps[0]);
                        int Eps_End = Convert.ToInt32(Eps[1]);

                        if (Eps_Beg > Eps_End)
                        {
                            Console.WriteLine(" Индекс начала не может быть больше индекса конца!!!!");
                            goto Excep;
                        }
                        Console.Write("\n Укажите путь куда вы хотите загрузить главы : ");
                        Path = Console.ReadLine();
                        Img_Down(Links_chapter, Eps_Beg, Eps_End, Path);
                        break;
                    }
            }
        }
        // Названия глав и ссылки на главы
        static void Chap_Names(string URL)
        {
            using (WebClient Wb_Chaps = new WebClient())
            {
                var Data_Chaps = Wb_Chaps.DownloadData(URL);
                var Page_Chaps = Encoding.Default.GetString(Data_Chaps);
                // Создание строки содержащую код страницы

                var Parser_Chaps = new HtmlParser();
                Parser_Chaps = new HtmlParser();
                var Doc_Pages = Parser_Chaps.ParseDocument(Page_Chaps);
                //Создание парсер-документа HTML Страницы

                string URL_Chaps = URL;

                var Last_Chap = Doc_Pages.QuerySelector("span.tx");
                int Last_Chap_Num = Convert.ToInt32(Last_Chap.InnerHtml.Substring(1));

                int Num_Chap = 0, Num_Page = 1;
                Links_chapter = new string[Last_Chap_Num];
                List<string>[] Names_Chap = new List<string>[Last_Chap_Num];
                while (Num_Chap < Last_Chap_Num)
                {

                    Data_Chaps = Wb_Chaps.DownloadData(URL_Chaps);
                    Page_Chaps = Encoding.Default.GetString(Data_Chaps);

                    Parser_Chaps = new HtmlParser();
                    Doc_Pages = Parser_Chaps.ParseDocument(Page_Chaps);
                    //Парсинг страницы с главами
                    var ChapNames = Doc_Pages.QuerySelectorAll("span.subj > span");
                    var Link_Ep = Doc_Pages.QuerySelectorAll("li[data-episode-no] > a");
                    //Получение названий Глав и ссылок на Главы
                    for (int i = 0; i < ChapNames.Length; i++)
                    {
                        Console.WriteLine(" [" + Num_Chap + "]  " + ChapNames[ChapNames.Length - 1 - i].Text());
                        Links_chapter[Num_Chap] = Link_Ep[ChapNames.Length - 1 - i].GetAttribute("href");
                        if (Num_Chap < Last_Chap_Num)
                            Num_Chap++;
                    }
                    Num_Page++;

                    URL_Chaps = URL;
                    URL_Chaps = URL + "&page=" + Num_Page;
                }
            }
        }
        static void Img_Down(string[] Links, int Beg, int End, string Path)
        {
            using (WebClient wc = new WebClient())
            {
                while (Beg < End) 
                {
                    var Page = wc.DownloadData(Links[Beg]);
                    var Page_Img = Encoding.Default.GetString(Page);

                    var Parser_Img = new HtmlParser();
                    var Doc_Img = Parser_Img.ParseDocument(Page_Img);
                    var Get_Chap_Name = Doc_Img.QuerySelector("h1.subj_episode");
                    var Get_Data_Url = Doc_Img.QuerySelectorAll("img._images[data-url]");

                    Console.WriteLine("\n Загружается " + Get_Chap_Name.Text());

                    wc.UseDefaultCredentials = true;
                    wc.Headers.Add("Referer", Links[Beg]);

                    #region Download
                    for (int i = 0; i < Get_Data_Url.Length; i++)
                    {
                        //https:/webtoon-phinf.pstatic.net/20200516_241/1589574502517Ku9ym_JPEG/9043590d-b92f-41ab-812a-6af735a0d76c.jpg?type=q90
                        if (!Path.EndsWith(@"\"))
                        {
                            if (!Directory.Exists($"{Path}{Get_Chap_Name.Text()}"))
                                Directory.CreateDirectory($"{Path}{Get_Chap_Name.Text()}");
                        }
                        else
                        {
                            if (!Directory.Exists($"{Path}\\{Get_Chap_Name.Text()}"))
                                Directory.CreateDirectory($"{Path}\\{Get_Chap_Name.Text()}");
                        }
                        wc.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.129 Safari/537.36 OPR/68.0.3618.63");
                        wc.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                        wc.Headers.Add("Referer", Links[Beg]);
                        // Path + "0" + j + ".png"
                        //if (j < 10) wc.DownloadFile(link.Substring(link.Length - 10),);
                        //else wc.DownloadFile(link.Substring(link.Length - 10));
                        if (i < 10)
                        {
                            if (!Path.EndsWith(@"\")) wc.DownloadFile(new Uri(Get_Data_Url[i].GetAttribute("data-url")), $"{Path}\\{Get_Chap_Name.Text()}\\0{i}.jpg");
                            else wc.DownloadFile(new Uri(Get_Data_Url[i].GetAttribute("data-url")), $"{Path}{Get_Chap_Name.Text()}\\0{i}.jpg");
                            Console.WriteLine($" Изображение [{i}] загрузилось.....!");
                        }
                        else
                        {
                            if (!Path.EndsWith(@"\")) wc.DownloadFile(new Uri(Get_Data_Url[i].GetAttribute("data-url")), $"{Path}\\{Get_Chap_Name.Text()}\\{i}.jpg");
                            else wc.DownloadFile(new Uri(Get_Data_Url[i].GetAttribute("data-url")), $@"{Path}{Get_Chap_Name.Text()}\\{i}.jpg");
                            Console.WriteLine($" Изображение [{i}] загрузилось.....!");
                        }

                        /*
                        if (bm != null)
                        {
                            if (j < 10) bm.Save(Path + "0" + j + ".png", ImageFormat.Jpeg);
                            else bm.Save(Path + j + ".png", ImageFormat.Jpeg);
                        }
                        */
                        wc.Headers.Clear();
                    }
                    #endregion

                    //Bitmap Bmap = new Bitmap($"{Path}\\{Get_Chap_Name.Text()}\\00.jpg");
                    List<string> files = new List<string>();
                    string[] filesfrom = Directory.GetFiles($"{Path}\\{Get_Chap_Name.Text()}");
                    foreach (string filename in filesfrom)
                    {
                        int pos = filename.LastIndexOf('.');

                        string extension = filename.Substring(pos).ToLower();
                        if ((extension == ".bmp") || (extension == ".jpg") ||
                            (extension == ".jpeg") || (extension == ".png") ||
                            (extension == ".tiff") || (extension == ".gif"))
                            files.Add(filename);
                    }
                    int num_images = files.Count;
                    if (num_images == 0) Console.WriteLine("В папке 0 файлов.");
                    Bitmap[] images = new Bitmap[files.Count];
                    for (int i = 0; i < num_images; i++) images[i] = new Bitmap(files[i]);

                    // Find the largest width and height.
                    int max_wid = 0;
                    int max_hgt = 0;
                    for (int i = 0; i < num_images; i++)
                    {
                        if (max_wid < images[i].Width) max_wid = images[i].Width;
                        if (max_hgt < images[i].Height) max_hgt = images[i].Height;
                    }

                    // Make the result bitmap. C:\Users\smile\Desktop\
                    int wid = max_wid;
                    int hgt = 0;
                    for (int i = 0; i < num_images; i++)
                        hgt += images[i].Height;

                    Bitmap bm = new Bitmap(wid, hgt);

                    // Place the images on it.
                    using (Graphics gr = Graphics.FromImage(bm))
                    {
                        int x = 0;
                        int y = 0;
                        for (int i = 0; i < num_images; i++)
                        {
                            gr.DrawImage(images[i], x, y);
                            x += images[i].Width;
                            if (x >= wid)
                            {
                                //MessageBox.Show("Pic #" + i);
                                y += images[i].Height;
                                x = 0;
                            }
                        }
                    }

                    bm.Save($"{Path}\\{Get_Chap_Name.Text()}\\999.png", ImageFormat.Png);
                    
                    //for (int i = 1; i < 
                    Beg++;
                }

                //Bitmap bmap = new Bitmap();
                //$"{Path}\\{Get_Chap_Name.Text()}"

            }
            //Bitmap Bm = new Bitmap();
        }
    }
}
/*
                         for (int j = 0; j < ChapNames.Length; j++)
                {
                    URL_Eps = Links_chapter[j];
                    Data_Eps = Wb_Chaps.DownloadData(URL_Eps);
                    Page_Eps = Encoding.Default.GetString(Data_Eps);

                    Parser_Eps = new HtmlParser();
                    var Doc_Eps = Parser_Eps.ParseDocument(Page_Eps);
                    var Image_URL = Doc_Eps.QuerySelectorAll("#_imageList > img._images");

                    Links_images = new string[Image_URL.Count()];

                    for (int i = 0; i < Image_URL.Count(); i++)
                        Links_images[i] = Image_URL[i].GetAttribute("data-url");
                    //Console.WriteLine(Image_URL[i].GetAttribute("data-url"));
                    //Console.WriteLine();
                }

         */
/*
            WebClient Wb_Chaps = new WebClient();
            // Создание WebClient'а     
            
            byte[] Data_Chaps, Data_Eps;
            string Page_Chaps = String.Empty;
            string Page_Eps = String.Empty;
            var Parser_Eps = new HtmlParser(); 
            var Parser_Chaps = new HtmlParser();

            Data_Chaps = Wb_Chaps.DownloadData(URL);
            Page_Chaps = Encoding.Default.GetString(Data_Chaps);
            // Создание строки содержащую код страницы

            Parser_Chaps = new HtmlParser();
            var Doc_Pages = Parser_Chaps.ParseDocument(Page_Chaps);
            //Создание парсер-документа HTML Страницы

            var Last_Chap = Doc_Pages.QuerySelector("span.tx");
            int Last_Chap_Num = Convert.ToInt32(Last_Chap.InnerHtml.Substring(1));
            //Получение конечного количества глав через QuerySelector


            int Num_Chap = 0, Num_Page = 1;
            string[] Links_chapter = new string[Last_Chap_Num];
            string[] Links_images;
            // Создание массива содержащего все ссылки на главы, размером в конечное количество глав
            // Создание переменной Номера главы и страницы с главами

            URL_Chaps = URL;
            */
/*
            #region Get_Chap_Names&URL  
            while (Num_Chap < Last_Chap_Num)
            {

                Data_Chaps = Wb_Chaps.DownloadData(URL_Chaps);
                Page_Chaps = Encoding.Default.GetString(Data_Chaps);

                Parser_Chaps = new HtmlParser();
                Doc_Pages = Parser_Chaps.ParseDocument(Page_Chaps);
                //Парсинг страницы с главами

                var ChapNames = Doc_Pages.QuerySelectorAll("span.subj");
                var Link_Ep = Doc_Pages.QuerySelectorAll("li[data-episode-no] > a");
                //Получение названий Глав и ссылок на Главы

                for (int i = 0; i < ChapNames.Length; i++)
                {
                    //Console.WriteLine(" [" + Num_Chap + "]  " + ChapNames[i].Text());
                    Links_chapter[Num_Chap] = Link_Ep[i].GetAttribute("href");
                    if (Num_Chap < Last_Chap_Num)
                        Num_Chap++;
                }

                Num_Page++;
                
                URL_Chaps = URL;
                URL_Chaps = URL + "&page=" + Num_Page;
            }
            #endregion
            */
/*
            var theImageUrl = Doc
                .DocumentElement.Descendents()
                .Where(x => x.NodeType == NodeType.Element)
                .OfType<IHtmlImageElement>()
                .Where(x => x.Attributes["class"]?.Value == "_images")
                .Select(x => x.Attributes["src"]?.Value)
                .FirstOrDefault();
            */
/*
            foreach (var nod in CssSelector)
            {
                Console.Write("[+] ");
                Console.WriteLine(nod);
            }*/
//foreach (var Url in theImageUrl)                Console.Write(Url);
            //Console.WriteLine("Hello World!");
/*
            int Page_Num = 1;
            var smth = Doc.QuerySelectorAll("div>a>span");
            if (Doc.QuerySelector("div>a>em").Equals("Next Page"))
                Page_Num += 10;
            foreach (var smt in smth)
            {

                Console.WriteLine(smt.Text());

            }
            */
// Программа считает количество ссылок с тэгом <a>
            // А если среди ссылок присутствует тэг <em> со значением "Next Page"
            // Тогда URL добавляется на &Page=11 и он с каждым последующим <em> будет увеличиваться на 10
            //
            //
            //
