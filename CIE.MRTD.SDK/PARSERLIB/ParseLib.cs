using CIE.MRTD.SDK.EAC;
using CieReader.Utils;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace CIE.MRTD.SDK.PARSERLIB
{        
    /*Classe che astrae le informazioni di una CIE, ha i metodi per la decodifica dell'icao e gli attributi delle
     informazioni presenti sulla carta, sono riportate le sole informazioni presenti sulla card di prova*/
    public class C_CIE
    {
        private CIE.MRTD.SDK.EAC.EAC Eac;
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string BirthCity { get; set; }
        public string BirthProv { get; set; }
        public string BirthDate { get; set; }
        public string Address { get; set; }
        public string Prov { get; set; }
        public string Cf { get; set; }
        public string Mrz { get; set; }
        public string DateIssue { get; set; }
        public string DateExpire { get; set; }
        public string Sex { get; set; }
        public string City { get; set; }
        public string Nationality { get; set; }
        public string DocumentNumber { get; set; }
        public string Authority { get; set; }
        public string Photo { get; set; }

        /* questo pattern è l'inizio del file jpeg2000 presente nella card, si cerca questo pattern per poi
         * scartare i primi bytes*/
        private static byte[] Jpg2kMagicNumber = { 0x00, 0x00, 0x00, 0x0C, 0x6A, 0x50,
            0x20, 0x20, 0x0D, 0x0A, 0x87, 0x0A, 0x00, 0x00, 0x00, 0x14, 0x66, 0x74,
            0x79, 0x70, 0x6A, 0x70, 0x32, 0x20, 0x00, 0x00, 0x00, 0x00, 0x6A, 0x70,
            0x32, 0x20, 0x00, 0x00, 0x00, 0x2D, 0x6A, 0x70, 0x32, 0x68, 0x00, 0x00,
            0x00, 0x16, 0x69, 0x68, 0x64, 0x72 };       

        /*Costruttore vuoto*/
        public C_CIE() { }

        /* Costruttore, riempie i campi dell'oggetto interrogando l'oggetto EAC */
        public C_CIE(CIE.MRTD.SDK.EAC.EAC eac)
        {
            Eac = eac;

            ReadMrz();
            ReadPhoto();
            ReadPersonalData();
            ReadDocumentData();
            ReadSod();
        }
        private void ReadMrz() 
        {
            var dg1 = Eac.ReadDG(DG.DG1);           // MRZ
            Mrz = ICAOGetValueFromKey(CieTags.KEY_MRZ, dg1);
        }
        private void ReadPhoto()
        {
            var dg2 = Eac.ReadDG(DG.DG2);           // Foto
            byte[] cie_jpg2k_image = ImageRetrieve(dg2);
            Photo = Convert.ToBase64String(cie_jpg2k_image);
        }
        private void ReadPersonalData()
        {
            var dg11 = Eac.ReadDG(DG.DG11);         // Dati personali come luogo di nascita, indirizzo, cf, nome e cognome, ecc..
            FirstName = ICAOGetValueFromKey(CieTags.KEY_FIRST_NAME, dg11);
            LastName = ICAOGetValueFromKey(CieTags.KEY_LAST_NAME, dg11);
            BirthDate = ICAOGetValueFromKey(CieTags.KEY_BIRTH_DATE, dg11);
            Sex = ICAOGetValueFromKey(CieTags.KEY_SEX, dg11);
            Nationality = ICAOGetValueFromKey(CieTags.KEY_NATIONALITY, dg11);
            Address = ICAOGetValueFromKey(CieTags.KEY_ADDRESS, dg11);
            Cf = ICAOGetValueFromKey(CieTags.KEY_CF, dg11);

            if (FirstName == null || LastName == null)
            {
                String[] s1 = ParseFullName(ICAOGetValueFromKey(CieTags.KEY_FULL_NAME, dg11));
                LastName = s1[0];
                FirstName = s1[1];
            }
            if (BirthCity == null)
            {
                String[] s2 = ParseAddress(ICAOGetValueFromKey(CieTags.KEY_BIRTH_ADDRESS, dg11));
                BirthCity = s2[0];
                BirthProv = s2[1];
            }
            if (Address == null || City == null || Prov == null)
            {
                String[] s3 = ParseAddress(ICAOGetValueFromKey(CieTags.KEY_ADDRESS, dg11));
                Address = s3[0];
                City = s3[1];
                Prov = s3[2];
            }
            if (BirthDate == null)
            {
                string[] lines = Mrz.Split('\n');
                string rawDate = lines[0].Substring(30, 6);
                string yearPrefix = rawDate.Substring(0, 2).CompareTo("50") >= 0 ? "19" : "20";
                BirthDate = rawDate.Substring(4, 2) + "/" + rawDate.Substring(2, 2) + "/" + yearPrefix + rawDate.Substring(0, 2);
            }
            else
            {
                BirthDate = GetParsedData(BirthDate);
            }
            if (Sex == null)
            {
                string[] lines = Mrz.Split('\n');
                Sex = lines[0].Substring(37, 1);
            }
            if (Nationality == null)
            {
                string[] lines = Mrz.Split('\n');
                Nationality = lines[0].Substring(2, 3);
            }            
        }
        private void ReadSod()
        {
            _ = Eac.ReadDG(DG.SOD);           // Firma digitale dei dati, non è necessario per il parsing ma è utile per verificare l'integrità dei dati letti
        }
        private void ReadDocumentData()
        {
            var dg12 = Eac.ReadDG(DG.DG12);         // Dati di emissione e scadenza della carta
            DocumentNumber = ICAOGetValueFromKey(CieTags.KEY_DOCUMENT_NUMBER, dg12);
            DateIssue = ICAOGetValueFromKey(CieTags.KEY_DATE_ISSUE, dg12);
            DateExpire = ICAOGetValueFromKey(CieTags.KEY_DATE_EXPIRE, dg12);
            Authority = ICAOGetValueFromKey(CieTags.KEY_AUTHORITY, dg12);

            if (DateIssue == null)
            {
                string rawDateIssue = ICAOGetValueFromKey(CieTags.KEY_DATE_ISSUE, dg12);
                DateIssue = GetParsedData(rawDateIssue);
            }
            else
            {
                DateIssue = GetParsedData(DateIssue);
            }
            if (DateExpire == null)
            {
                string[] lines = Mrz.Split('\n');
                string expiryRaw = lines[0].Substring(38, 6); // YYMMDD
                string yearPrefix = expiryRaw.Substring(0, 2).CompareTo("50") >= 0 ? "19" : "20";
                DateExpire = expiryRaw.Substring(4, 2) + "/" + expiryRaw.Substring(2, 2) + "/" + yearPrefix + expiryRaw.Substring(0, 2);
            }
            else
            {
                DateExpire = GetParsedData(DateExpire);
            }
            if (DocumentNumber == null)
            {
                string[] lines = Mrz.Split('\n');
                DocumentNumber = lines[0].Substring(5, 9);
            }
            if (Authority == null)
            {
                Authority = ICAOGetValueFromKey(CieTags.KEY_AUTHORITY_ALTERNATIVE, dg12);
            }
        }
        /*Estrae i byte dell'immagine jpeg2000 da un array di bytes usando il suo magic_number*/
        private Byte[] ImageRetrieve(Byte[] blob)
        {
            int[] locs = ArrayUtils.Locate(blob, Jpg2kMagicNumber);
            if (locs.Length == 0)
            {
                return [];
            }
            return blob.SubArray(locs[0], blob.Length - locs[0]);
        }

        /* estrae i dati da un dg attraverso il tag */
        private String ICAOGetValueFromKey(byte[] key, byte[] dg)
        {

            int[] index = ArrayUtils.Locate(dg, key);

            if (index == ArrayUtils.Empty)
                return null;

            int i = index.Length == 1 ? index[0] : index[1]; //la prima occorrenza è quella della lista dei tag (eccezione per dg1)

            int sizeOfData = Convert.ToInt32(dg[i + key.Length]);

            return System.Text.Encoding.UTF8.GetString(dg.SubArray(i + key.Length + 1, sizeOfData));
        }

        /* Fa il parsing del FullName dividendolo in nome e cognome */
        private String[] ParseFullName(String s)
        {
            return s.Split(new[] { "<<" }, StringSplitOptions.None); ;
        }

        /* Parsing di un indirizzo dividendolo in Via (eventuale), Città e provincia */
        private String[] ParseAddress(String s)
        {

            return s.Split(new[] { "<" }, StringSplitOptions.None); ;
        }                                        
        /* Parserizza la stringa contenente una data in fomrato aaaammgg in una con formato gg/mm/aaaa*/
        private String GetParsedData(String d)
        {
            if (d == null)
            {
                return "";
            }
            return d.Substring(6, 2) + "/" + d.Substring(4, 2) + "/" + d.Substring(0, 4);
        }
        public override string ToString()
        {
            return $"First name: {FirstName}\n" +
                   $"Last name: {LastName}\n" +
                   $"Birth city: {BirthCity}\n" +
                   $"Birth prov: {BirthProv}\n" +
                   $"Birth date: {BirthDate}\n" +
                   $"Address: {Address}\n" +
                   $"Prov: {Prov}\n" +
                   $"CF: {Cf}\n" +
                   $"MRZ: {Mrz}\n" +
                   $"Date issue: {DateIssue}\n" +
                   $"Date expire: {DateExpire}\n" +
                   $"City: {City}\n" +
                   $"Photo: {Photo}";
        }

        public string ToJsonString()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            return System.Text.Json.JsonSerializer.Serialize(this, options);
        }
    }
}
