using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NoJokeGEZ.GEZ
{
    public class GEZSession
    {
        #region CreateNewSession
        private const string PageNewSesssion = "https://online-services.rundfunkbeitrag.de/Obsidian/anmeldung/anmeldungperson.xhtml";
        public static Dictionary<string,string> BaseSessionToken()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(PageNewSesssion);
            request.CookieContainer = new CookieContainer();
            Dictionary<string, string> sessionTokens = new Dictionary<string, string>();
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
                        string x = FilterViewState(reader.ReadToEnd());
                        reader.Close();reader.Dispose();
                    }
                    response.Close();
                    response.Dispose();
                }
            }catch(Exception ex)
            {
                throw ex;
            }
            return sessionTokens;
        }

        private static string FilterViewState(string input){
            Regex regex = new Regex("</fieldset><input type=\"hidden\" name=\"javax\\.faces\\.ViewState\" id=\"j_id1:javax\\.faces\\.ViewState:0\" value=\"(?:[^\"]|\"\")*\" autocomplete=\"off\" />", RegexOptions.IgnoreCase);
            if (regex.IsMatch(input))
            {
                string m = regex.Match(input).ToString();
                m = m.Remove(0, m.IndexOf("value=\"") + 7);
                return m.Substring(0,m.IndexOf("\""));
            }
            else { return ""; }
        }
        #endregion

        #region RequestFirstInput
        private const string secondSite = "";


        #endregion

        public struct SessionHolder {

            private Dictionary<string, string> SessionValues;
            private string javaxViewState;

            public SessionHolder(Dictionary<string, string> sessionValues, string javaxViewState)
            {
                SessionValues = sessionValues;
                this.javaxViewState = javaxViewState;
            }

            public Dictionary<string, string> GetSessionValue { get => SessionValues};
            public string GetJavaxViewState { get => javaxViewState};
        }
    }
}
