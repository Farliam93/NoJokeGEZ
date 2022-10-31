using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;
using System.Web;

namespace NoJokeGEZ.GEZ
{
    public class ContentCreator
    {
        #region TopVariabel
        private string defaulPLZ = Directory.GetCurrentDirectory() + @"\DB\PLZ.csv";
        private string defaulStrasse = Directory.GetCurrentDirectory() + @"\DB\Nachnamen.csv";
        private string defaulVorname = Directory.GetCurrentDirectory() + @"\DB\Vornamen.csv";
        private string defaulNachname = Directory.GetCurrentDirectory() + @"\DB\Nachnamen.csv";

        private List<Tuple<string, string, string>> myPLZ;
        private List<string> myStrasse;
        private List<string> myVorname;
        private List<string> myNachname;

        private Random rnd;
        #endregion
        public ContentCreator()
        {
            if(File.Exists(defaulPLZ) && File.Exists(defaulStrasse) && File.Exists(defaulNachname) && File.Exists(defaulVorname))
            {
                rnd = new Random(Guid.NewGuid().GetHashCode());
                ReadData();
            }else { throw new Exception("Dateien nicht gefunden"); }
        }

        private void ReadData()
        {
            myPLZ = new List<Tuple<string, string, string>>();
            myStrasse = new List<string>();
            myVorname = new List<string>();
            myNachname = new List<string>();

            using (TextFieldParser csvNachname = new TextFieldParser(defaulNachname))
            {
                csvNachname.CommentTokens = new string[] { "#" };
                csvNachname.SetDelimiters(new string[] { "," });
                csvNachname.HasFieldsEnclosedInQuotes = true;
                
                // Skip the row with the column names
                csvNachname.ReadLine();

                while (!csvNachname.EndOfData)
                {
                    myNachname.Add(csvNachname.ReadLine());
                }
            }

            using (TextFieldParser csvVorname = new TextFieldParser(defaulVorname))
            {
                csvVorname.CommentTokens = new string[] { "#" };
                csvVorname.SetDelimiters(new string[] { "," });
                csvVorname.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvVorname.ReadLine();

                while (!csvVorname.EndOfData)
                {
                    myVorname.Add(csvVorname.ReadLine());
                }
            }

            using (TextFieldParser csvStrasse = new TextFieldParser(defaulStrasse))
            {
                csvStrasse.CommentTokens = new string[] { "#" };
                csvStrasse.SetDelimiters(new string[] { "," });
                csvStrasse.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvStrasse.ReadLine();

                while (!csvStrasse.EndOfData)
                {
                    myStrasse.Add(csvStrasse.ReadLine());
                }
            }

            using (TextFieldParser csvplz = new TextFieldParser(defaulPLZ))
            {
                csvplz.CommentTokens = new string[] { "#" };
                csvplz.SetDelimiters(new string[] { ";" });
                csvplz.HasFieldsEnclosedInQuotes = true;

                // Skip the row with the column names
                csvplz.ReadLine();

                while (!csvplz.EndOfData)
                {
                    string[] fields = csvplz.ReadFields();
                    string plz = fields[0];
                    string Stadt = fields[1];
                    string Ort = fields[2];
                    myPLZ.Add(new Tuple<string, string, string>(plz, Stadt, Ort));
                }
            }
        }

        public PersonData CreateRandom()
        {
            string vname = myVorname[rnd.Next(0, myVorname.Count - 1)];
            string nname = myNachname[rnd.Next(0, myNachname.Count - 1)];
            string mstrasse = myStrasse[rnd.Next(0, myStrasse.Count - 1)];
            var xp = myPLZ[rnd.Next(0, myPLZ.Count - 1)];
            return new PersonData(nname, vname, xp.Item1, xp.Item2, xp.Item3, rnd.Next(1, 28), rnd.Next(1, 12), rnd.Next(1980, 2001),rnd.Next(0,200), mstrasse);
        }

        public string CreatePostPersonData(GEZSession.SessionHolder session, PersonData randPerson)
        {
            string tmp = "anmeldeform=anmeldeform&anmeldeform:ctangemeldet:angemeldet=false&anmeldeform:ctbnr:bnr=&anmeldeform:ctanrede:anrede=3&anmeldeform:ctvorname:vorname=";
            tmp += randPerson.vorname + "&anmeldeform:ctnachname:nachname=" + randPerson.name + "&anmeldeform:ctgeburt:cttage:tage=" + randPerson.day.ToString();
            tmp += "&anmeldeform:ctgeburt:ctmonate:monate=" + randPerson.month + "&anmeldeform:ctgeburt:ctjahre:jahre=" + randPerson.year;
            tmp += "&anmeldeform:ctplz:plz=" + randPerson.plz + "&anmeldeform:ctort:ort=" + randPerson.ort + "&anmeldeform:ctstrasse:strasse=" + randPerson.strasse;
            tmp += "&anmeldeform:cthausNr:hausnr=" + randPerson.hausnummer + "&anmeldeform:ctadresszusatz:adresszusatz=&anmeldeform:ctvorwahl:vorwahl=&anmeldeform:cttel:tel=&anmeldeform:ctemail:email=&anmeldeform:j_idt285=Weiter&javax.faces.ViewState=";
            tmp += session.GetJavaxViewState;
            return tmp;
        }
        public string CreateWohnungData(GEZSession.SessionHolder session, PersonData data)
        {
            string tmp = "anmeldeform=anmeldeform&anmeldeform:ctabweichend:abweichend=false&anmeldeform:ctplz:plz=&anmeldeform:ctort:ort=&anmeldeform:ctstrasse:strasse=&anmeldeform:cthausNr:hausnr=&anmeldeform:ctanmeldedatum:monate=11&anmeldeform:ctanmeldedatum:jahre=2023&anmeldeform:weitere:ctplz:plz=&anmeldeform:weitere:ctort:ort=&anmeldeform:weitere:ctstrasse:strasse=&anmeldeform:weitere:cthausNr:hausnr=&anmeldeform:weitere:ctanmeldedatum:monate=Monat&anmeldeform:weitere:ctanmeldedatum:jahre=Jahr&anmeldeform:j_idt244=Weiter&javax.faces.ViewState=";
            return tmp + session.GetJavaxViewState;
        }

        public string CreateZahlungsData(GEZSession.SessionHolder session, PersonData data)
        {
            string tmp = "anmeldeform=anmeldeform&anmeldeform:zmod:ctrhythmus:rhythmus=1&anmeldeform:zmod:ctart:zahlungsart=3&anmeldeform:zmod:ctiban:iban=&anmeldeform:zmod:ctbic:bic=&anmeldeform:zmod:ctgeldinstitut:geldinstitut=&anmeldeform:zmod:ctNameKontoinhaber:nameKontoinhaber=&anmeldeform:j_idt162=Weiter&javax.faces.ViewState=";
            return tmp + session.GetJavaxViewState;
        }

        public string CreateCaptchaAndSubmit(GEZSession.SessionHolder session, string captcha)
        {
            string tmp = "anmeldeform=anmeldeform&anmeldeform:cb_datenschutz=on&anmeldeform:captcha:captcha_response=" + captcha;
            tmp += "&anmeldeform:anmelden=Anmelden&javax.faces.ViewState=" + session.GetJavaxViewState;
            return tmp;
        }
    }

    public struct PersonData
    {
        public string name;
        public string vorname;
        public string plz;
        public string ort;
        public string stadt;
        public int day;
        public int month;
        public int year;
        public int hausnummer;
        public string strasse;

        public PersonData(string name, string vorname, string plz, string ort, string stadt, int day, int month, int year, int hausnummer, string strasse)
        {
            this.name = Regex.Replace(name, "[^\\w]", "");
            this.vorname = Regex.Replace(vorname, "[^\\w]", "");
            this.plz = Regex.Replace(plz, "[^\\w]", "");
            this.ort = Regex.Replace(ort, "[^\\w]", "");
            this.stadt = Regex.Replace(stadt, "[^\\w]", "");
            this.day = day;
            this.month = month;
            this.year = year;
            if (ort == "") ort = stadt;
            if (stadt == "") stadt = ort;
            this.hausnummer = hausnummer;
            this.strasse = Regex.Replace(strasse, "[^\\w]", "");
        }
        public override string ToString()
        {
            return $"Name:{name},Vorname:{vorname},PLZ:{plz},Ort:{ort},Stadt:{stadt}Straße:{strasse},Hausnummer:{hausnummer},Geb:{day}.{month}.{year}";
        }
    }
}
