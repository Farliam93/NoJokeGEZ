using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
namespace NoJokeGEZ.GEZ
{
    public class GEZSession
    {
        #region GetFirstFormular
        private const string PageNewSesssion = "https://online-services.rundfunkbeitrag.de/Obsidian/anmeldung/anmeldungperson.xhtml";
        private const string SecondPage = "https://online-services.rundfunkbeitrag.de/Obsidian/anmeldung/anmeldungwohnung.xhtml";
        private const string ThirdPage = "https://online-services.rundfunkbeitrag.de/Obsidian/anmeldung/anmeldungprivatzahlung.xhtml";
        private const string CaptchaLoad = "https://online-services.rundfunkbeitrag.de/Obsidian/captchaPicture/captchaPicture.xhtml";
        private const string LastPage = "https://online-services.rundfunkbeitrag.de/Obsidian/anmeldung/anmeldungprivatresumee.xhtml";
        private const string Succes = "https://online-services.rundfunkbeitrag.de/Obsidian/anmeldung/anmeldungprivatbestaetigung.xhtml";
        public static SessionHolder BaseSessionToken()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(PageNewSesssion);
            request.CookieContainer = new CookieContainer();
            Dictionary<string, string> sessionTokens = new Dictionary<string, string>();
            string javaxViewState;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                   foreach(var cks in response.Cookies)
                    {
                        Cookie ck = (Cookie)cks;
                        sessionTokens.Add(ck.Name, ck.Value);
                    }
                    using(StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        javaxViewState = FilterViewState(reader.ReadToEnd());
                        reader.Close();reader.Dispose();
                    }
                    response.Close();
                    response.Dispose();
                }
            }catch(Exception ex)
            {
                throw ex;
            }
            return new SessionHolder(sessionTokens, javaxViewState) ;
        }


        #endregion

        #region PostFirstFormular
        public static bool PostFirstFormular(SessionHolder session, string content)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(PageNewSesssion);
            string message;
            request.CookieContainer = new CookieContainer();
            foreach (var cc in session.GetSessionValue)
            {
             request.CookieContainer.Add(new Cookie(cc.Key,cc.Value,"/", "online-services.rundfunkbeitrag.de"));
            }
            request.Host = "online-services.rundfunkbeitrag.de";
            request.Referer = "https://online-services.rundfunkbeitrag.de/Obsidian/anmeldung/anmeldungperson.xhtml?gemeldet=n&bereichId=0&atoptedin=Y";
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=";
            request.AllowAutoRedirect = true;
            request.Headers.Add("Origin", "https://online-services.rundfunkbeitrag.de");
            request.Headers.Add("sec-ch-ua", "\"Chromium\";v=\"106\", \"Google Chrome\";v=\"106\", \"Not;A=Brand\";v=\"99\"");
            request.Headers.Add("sec-ch-ua-mobile", "?0");
            request.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
            request.Headers.Add("Sec-Fetch-Dest", "iframe");
            request.Headers.Add("Sec-Fetch-Mode", "navigate");
            request.Headers.Add("Sec-Fetch-Site", "same-origin");
            request.Headers.Add("Sec-Fetch-User", "?1");
            request.Headers.Add("Upgrade-Insecure-Requests", "1");
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36";
            byte[] data = ASCIIEncoding.UTF8.GetBytes(content);
            request.ContentLength = data.Length;

            using(Stream stream = request.GetRequestStream())
            {
                stream.Write(data,0,data.Length);
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        message = reader.ReadToEnd();
                        reader.Close(); reader.Dispose();
                    }
                }
            }catch(WebException ex)
            {
                throw ex;
            }
            return true;
        }
        #endregion

        #region Hilfsstrukturen/Funktionen
        private static string FilterViewState(string input)
        {
            Regex regex = new Regex("</fieldset><input type=\"hidden\" name=\"javax\\.faces\\.ViewState\" id=\"j_id1:javax\\.faces\\.ViewState:0\" value=\"(?:[^\"]|\"\")*\" autocomplete=\"off\" />", RegexOptions.IgnoreCase);
            if (regex.IsMatch(input))
            {
                string m = regex.Match(input).ToString();
                m = m.Remove(0, m.IndexOf("value=\"") + 7);
                return m.Substring(0, m.IndexOf("\""));
            }
            else {
                Regex reg = new Regex("<input type=\"hidden\" name=\"javax\\.faces\\.ViewState\" id=\"j_id1:javax\\.faces\\.ViewState:0\" value=\"[^\"]*\" autocomplete=\"off\" />", RegexOptions.IgnoreCase);
                if (reg.IsMatch(input))
                {
                    string m = reg.Match(input).ToString();
                    m = m.Remove(0, m.IndexOf("value=\"") + 7);
                    return m.Substring(0, m.IndexOf("\""));
                }
                return "";
            }
        }
        public struct SessionHolder
        {

            private Dictionary<string, string> SessionValues;
            private string javaxViewState;

            public SessionHolder(Dictionary<string, string> sessionValues, string javaxViewState)
            {
                SessionValues = sessionValues;
                this.javaxViewState = javaxViewState;
            }

            public Dictionary<string, string> GetSessionValue { get => SessionValues; }
            public string GetJavaxViewState { get => javaxViewState; set => javaxViewState = value; }
        }
        #endregion

        #region GetSecondPage
        public static SessionHolder GetSecondPage(ref SessionHolder last)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(SecondPage);
            request.CookieContainer = new CookieContainer();
            foreach (var cc in last.GetSessionValue)
            {
                request.CookieContainer.Add(new Cookie(cc.Key, cc.Value, "/", "online-services.rundfunkbeitrag.de"));
            }
            string javaxViewState;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {   
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        javaxViewState = FilterViewState(reader.ReadToEnd());
                        reader.Close(); reader.Dispose();
                    }
                    response.Close();
                    response.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            last.GetJavaxViewState = javaxViewState;
            return last;
        }
        #endregion

        #region Post SecondPage

        public static bool PostSecondFormular(SessionHolder session, string content)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(SecondPage);
            string message;
            request.CookieContainer = new CookieContainer();
            foreach (var cc in session.GetSessionValue)
            {
                request.CookieContainer.Add(new Cookie(cc.Key, cc.Value, "/", "online-services.rundfunkbeitrag.de"));
            }
            request.Host = "online-services.rundfunkbeitrag.de";
            request.Referer = "https://online-services.rundfunkbeitrag.de/Obsidian/anmeldung/anmeldungwohnung.xhtml";
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=";
            request.AllowAutoRedirect = true;
            request.Headers.Add("Origin", "https://online-services.rundfunkbeitrag.de");
            request.Headers.Add("sec-ch-ua", "\"Chromium\";v=\"106\", \"Google Chrome\";v=\"106\", \"Not;A=Brand\";v=\"99\"");
            request.Headers.Add("sec-ch-ua-mobile", "?0");
            request.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
            request.Headers.Add("Sec-Fetch-Dest", "iframe");
            request.Headers.Add("Sec-Fetch-Mode", "navigate");
            request.Headers.Add("Sec-Fetch-Site", "same-origin");
            request.Headers.Add("Sec-Fetch-User", "?1");
            request.Headers.Add("Upgrade-Insecure-Requests", "1");
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36";
            byte[] data = ASCIIEncoding.UTF8.GetBytes(content);
            request.ContentLength = data.Length;

            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        message = reader.ReadToEnd();
                        reader.Close(); reader.Dispose();
                    }
                }
            }
            catch (WebException ex)
            {
                throw ex;
            }
            return true;
        }

        #endregion

        #region GetThirdPage
        public static SessionHolder GetThirdPage(ref SessionHolder last)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ThirdPage);
            request.CookieContainer = new CookieContainer();
            foreach (var cc in last.GetSessionValue)
            {
                request.CookieContainer.Add(new Cookie(cc.Key, cc.Value, "/", "online-services.rundfunkbeitrag.de"));
            }
            string javaxViewState;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        javaxViewState = FilterViewState(reader.ReadToEnd());
                        reader.Close(); reader.Dispose();
                    }
                    response.Close();
                    response.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            last.GetJavaxViewState = javaxViewState;
            return last;
        }
        #endregion

        #region PostThirdPage
        public static bool PostThirdFormular(ref SessionHolder session, string content)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ThirdPage);
            string message;
            request.CookieContainer = new CookieContainer();
            foreach (var cc in session.GetSessionValue)
            {
                request.CookieContainer.Add(new Cookie(cc.Key, cc.Value, "/", "online-services.rundfunkbeitrag.de"));
            }
            request.Host = "online-services.rundfunkbeitrag.de";
            request.Referer = "https://online-services.rundfunkbeitrag.de/Obsidian/anmeldung/anmeldungprivatzahlung.xhtml";
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=";
            request.AllowAutoRedirect = true;
            request.Headers.Add("Origin", "https://online-services.rundfunkbeitrag.de");
            request.Headers.Add("sec-ch-ua", "\"Chromium\";v=\"106\", \"Google Chrome\";v=\"106\", \"Not;A=Brand\";v=\"99\"");
            request.Headers.Add("sec-ch-ua-mobile", "?0");
            request.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
            request.Headers.Add("Sec-Fetch-Dest", "iframe");
            request.Headers.Add("Sec-Fetch-Mode", "navigate");
            request.Headers.Add("Sec-Fetch-Site", "same-origin");
            request.Headers.Add("Sec-Fetch-User", "?1");
            request.Headers.Add("Upgrade-Insecure-Requests", "1");
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36";
            byte[] data = ASCIIEncoding.UTF8.GetBytes(content);
            request.ContentLength = data.Length;
            string url = "";
            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        message = reader.ReadToEnd();
                        session.GetJavaxViewState = FilterViewState(message);
                        url = message.Remove(0, message.IndexOf("vorlesen</a><a href=\"") + 21);
                        url = url.Substring(0, url.IndexOf("\""));
                        reader.Close(); reader.Dispose();
                    }
                }
            }
            catch (WebException ex)
            {
                throw ex;
            }
            return true;
        }
        #endregion

        #region DownloadCaptcha
        public static Bitmap  DownloadCaptcha(SessionHolder session)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(CaptchaLoad);
            request.CookieContainer = new CookieContainer();
            foreach (var cc in session.GetSessionValue)
            {
                request.CookieContainer.Add(new Cookie(cc.Key, cc.Value, "/", "online-services.rundfunkbeitrag.de"));
            }
            string message;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        message = reader.ReadToEnd();
                        reader.Close(); reader.Dispose();
                    }
                    response.Close();
                    response.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            string nuri = message.Remove(0, message.IndexOf("<img src=\"") + 10);
            nuri = "https://online-services.rundfunkbeitrag.de" +  nuri.Substring(0, nuri.IndexOf("\""));
            nuri = nuri.Replace("amp;", "");
            System.Drawing.Image image = null;
            try
            {
                System.Net.HttpWebRequest webRequest = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(nuri);
                webRequest.AllowWriteStreamBuffering = true;
                webRequest.Timeout = 30000;
                webRequest.ServicePoint.ConnectionLeaseTimeout = 5000;
                webRequest.ServicePoint.MaxIdleTime = 5000;
                webRequest.CookieContainer = new CookieContainer();
                foreach (var cc in session.GetSessionValue)
                {
                    webRequest.CookieContainer.Add(new Cookie(cc.Key, cc.Value, "/", "online-services.rundfunkbeitrag.de"));
                }
                using (System.Net.WebResponse webResponse = webRequest.GetResponse())
                {

                    using (System.IO.Stream stream = webResponse.GetResponseStream())
                    {
                        image = System.Drawing.Image.FromStream(stream);
                    }
                }

                webRequest.ServicePoint.CloseConnectionGroup(webRequest.ConnectionGroupName);
                webRequest = null;

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);

            }
            //string fpath = Directory.GetCurrentDirectory() + "\\" + DateTime.Now.ToString("hhmmss") + ".jpg";
            //image.Save(fpath, System.Drawing.Imaging.ImageFormat.Jpeg);
            //System.Diagnostics.Process.Start(fpath);
            return (Bitmap)image;
        }
        #endregion

        #region GetLastPage
        public static SessionHolder GetLastPage(ref SessionHolder last)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(LastPage);
            request.CookieContainer = new CookieContainer();
            foreach (var cc in last.GetSessionValue)
            {
                request.CookieContainer.Add(new Cookie(cc.Key, cc.Value, "/", "online-services.rundfunkbeitrag.de"));
            }
            string javaxViewState;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string message = reader.ReadToEnd();
                        javaxViewState = FilterViewState(message);
                        reader.Close(); reader.Dispose();
                    }
                    response.Close();
                    response.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            last.GetJavaxViewState = javaxViewState;
            return last;
        }
        #endregion

        #region PostLastPage
        public static bool PostLastPage(SessionHolder session, string content)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(LastPage);
            string message;
            request.CookieContainer = new CookieContainer();
            foreach (var cc in session.GetSessionValue)
            {
                request.CookieContainer.Add(new Cookie(cc.Key, cc.Value, "/", "online-services.rundfunkbeitrag.de"));
            }
            request.Host = "online-services.rundfunkbeitrag.de";
            request.Referer = "https://online-services.rundfunkbeitrag.de/Obsidian/anmeldung/anmeldungprivatresumee.xhtml";
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=";
            request.AllowAutoRedirect = true;
            request.Headers.Add("Origin", "https://online-services.rundfunkbeitrag.de");
            request.Headers.Add("sec-ch-ua", "\"Chromium\";v=\"106\", \"Google Chrome\";v=\"106\", \"Not;A=Brand\";v=\"99\"");
            request.Headers.Add("sec-ch-ua-mobile", "?0");
            request.Headers.Add("sec-ch-ua-platform", "\"Windows\"");
            request.Headers.Add("Sec-Fetch-Dest", "iframe");
            request.Headers.Add("Sec-Fetch-Mode", "navigate");
            request.Headers.Add("Sec-Fetch-Site", "same-origin");
            request.Headers.Add("Sec-Fetch-User", "?1");
            request.Headers.Add("Upgrade-Insecure-Requests", "1");
            request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36";
            byte[] data = ASCIIEncoding.UTF8.GetBytes(content);
            request.ContentLength = data.Length;
            bool answer = false;
            using (Stream stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        message = reader.ReadToEnd();
                        answer = message.Contains("Die von Ihnen angegebenen Daten haben wir an die zuständige Stelle im Haus weitergeleitet.");
                        reader.Close(); reader.Dispose();
                    }
                }
            }
            catch (WebException ex)
            {
                throw ex;
            }
            return answer;
        }
        #endregion

        #region Success
        public static bool IsWorking(ref SessionHolder last)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Succes);
            request.CookieContainer = new CookieContainer();
            foreach (var cc in last.GetSessionValue)
            {
                request.CookieContainer.Add(new Cookie(cc.Key, cc.Value, "/", "online-services.rundfunkbeitrag.de"));
            }
            string message;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        message = reader.ReadToEnd();
                        reader.Close();reader.Dispose();
                    }
                    response.Close();
                    response.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return true;
        }
        #endregion

    }
}
