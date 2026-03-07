using CIE.MRTD.SDK.EAC;
using CSJ2K;
using System.Xml.Serialization;
using Nancy.Json;
using CieReader.Utils;
using System.Diagnostics;

namespace CIE.MRTD.SDK.PARSERLIB
{        
    /*Classe che astrae le informazioni di una CIE, ha i metodi per la decodifica dell'icao e gli attributi delle
     informazioni presenti sulla carta, sono riportate le sole informazioni presenti sulla card di prova*/
    public class C_CIE
    {
        private CIE.MRTD.SDK.EAC.EAC eac;
        public String firstName;
        public String lastName;
        public String birthCity;       
        public String birthProv;
        public String birthDate;
        public String address;
        public String prov;
        public String cf;
        public String mrz;
        public String dateIssue;
        public String dateExpire;
        public String sex;
        public String city;
        public String nationality;
        public String documentNumber;
        public String authority;
        public byte[] cie_jpg2k_image;

        /* questo pattern è l'inizio del file jpeg2000 presente nella card, si cerca questo pattern per poi
         * scartare i primi bytes*/
        public static byte[] jpg2k_magic_number = { 0x00, 0x00, 0x00, 0x0C, 0x6A, 0x50,
            0x20, 0x20, 0x0D, 0x0A, 0x87, 0x0A, 0x00, 0x00, 0x00, 0x14, 0x66, 0x74,
            0x79, 0x70, 0x6A, 0x70, 0x32, 0x20, 0x00, 0x00, 0x00, 0x00, 0x6A, 0x70,
            0x32, 0x20, 0x00, 0x00, 0x00, 0x2D, 0x6A, 0x70, 0x32, 0x68, 0x00, 0x00,
            0x00, 0x16, 0x69, 0x68, 0x64, 0x72 };       

        /*Costruttore vuoto*/
        public C_CIE() { }

        /* Costruttore, riempie i campi dell'oggetto interrogando l'oggetto EAC */
        public C_CIE(CIE.MRTD.SDK.EAC.EAC eac)
        {
            var dg1 = eac.ReadDG(DG.DG1);           // MRZ
            var dg2 = eac.ReadDG(DG.DG2);           // Foto
            var dg11 = eac.ReadDG(DG.DG11);         // Dati personali come luogo di nascita, indirizzo, cf, nome e cognome, ecc..
            var dg12 = eac.ReadDG(DG.DG12);         // Dati di emissione e scadenza della carta            
            var sod = eac.ReadDG(DG.SOD);           // Firma digitale dei dati, non è necessario per il parsing ma è utile per verificare l'integrità dei dati letti

            firstName = ICAOGetValueFromKey(CieTags.KEY_FIRST_NAME, dg11);
            lastName = ICAOGetValueFromKey(CieTags.KEY_LAST_NAME, dg11);
            birthDate = ICAOGetValueFromKey(CieTags.KEY_BIRTH_DATE, dg11);
            sex = ICAOGetValueFromKey(CieTags.KEY_SEX, dg11);
            nationality = ICAOGetValueFromKey(CieTags.KEY_NATIONALITY, dg11);            
            address = ICAOGetValueFromKey(CieTags.KEY_ADDRESS, dg11);
            cf = ICAOGetValueFromKey(CieTags.KEY_CF, dg11);
            documentNumber = ICAOGetValueFromKey(CieTags.KEY_DOCUMENT_NUMBER, dg12);
            dateIssue = ICAOGetValueFromKey(CieTags.KEY_DATE_ISSUE, dg12);
            dateExpire = ICAOGetValueFromKey(CieTags.KEY_DATE_EXPIRE, dg12);
            authority = ICAOGetValueFromKey(CieTags.KEY_AUTHORITY, dg12);
            mrz = ICAOGetValueFromKey(CieTags.KEY_MRZ, dg1);

            if (firstName == null || lastName == null)
            {
                String[] s1 = parseFullName(ICAOGetValueFromKey(CieTags.KEY_FULL_NAME, dg11));
                lastName = s1[0];
                firstName = s1[1];                
            }
            if (birthDate == null)
            {
                String[] s2 = parseAddress(ICAOGetValueFromKey(CieTags.KEY_BIRTH_ADDRESS, dg11));
                birthCity = s2[0];
                birthProv = s2[1];
            }
            if (address == null || city == null || prov == null)
            {
                String[] s3 = parseAddress(ICAOGetValueFromKey(CieTags.KEY_ADDRESS, dg11));
                address = s3[0];
                city = s3[1];
                prov = s3[2];
            }
            if (dateIssue == null)
            {
                string rawDateIssue = ICAOGetValueFromKey(CieTags.KEY_DATE_ISSUE, dg12);
                dateIssue = getParsedData(rawDateIssue);
            }
            else 
            {
                dateIssue = getParsedData(dateIssue);
            }
            if (birthDate == null)
            {
                string[] lines = mrz.Split('\n');
                string rawDate = lines[0].Substring(30, 6);
                string yearPrefix = rawDate.Substring(0, 2).CompareTo("50") >= 0 ? "19" : "20";
                birthDate = rawDate.Substring(4, 2) + "/" + rawDate.Substring(2, 2) + "/" + yearPrefix + rawDate.Substring(0, 2);
            }
            else
            {
                birthDate = getParsedData(birthDate);
            }
            if (dateExpire == null)
            {
                string rawDateExpire = ICAOGetValueFromKey(CieTags.KEY_DATE_EXPIRE, dg12);
                string[] lines = mrz.Split('\n');
                //if (lines.Length > 0 && lines[0].Length >= 43)
                //{
                string expiryRaw = lines[0].Substring(38, 6); // YYMMDD
                string yearPrefix = expiryRaw.Substring(0, 2).CompareTo("50") >= 0 ? "19" : "20";                    
                dateExpire = expiryRaw.Substring(4,2) + "/" + expiryRaw.Substring(2, 2) + "/" + yearPrefix + expiryRaw.Substring(0,2); 
                //}
            }
            else
            { 
                dateExpire = getParsedData(dateExpire);
            }
            if (sex == null)
            {
                string[] lines = mrz.Split('\n');
                sex = lines[0].Substring(37, 1);
            }
            if (nationality == null)
            {
                string[] lines = mrz.Split('\n');
                nationality = lines[0].Substring(2, 3);
            }
            if (documentNumber == null)
            {
                string[] lines = mrz.Split('\n');
                documentNumber = lines[0].Substring(5, 9);
            }

            cie_jpg2k_image = Image_retrive(dg2);          
            
            Debug.WriteLine("Parsing completato, dati estratti: %s", this.ToJsonString());

        }        
        /*Estrae i byte dell'immagine jpeg2000 da un array di bytes usando il suo magic_number*/
        public static Byte[] Image_retrive(Byte[] blob)
        {
            int loc = ArrayUtils.Locate(blob, jpg2k_magic_number)[0];
            return blob.SubArray(loc, blob.Length - loc);

        }

        /* estrae i dati da un dg attraverso il tag */
        static String ICAOGetValueFromKey(byte[] key, byte[] dg)
        {

            int[] index = ArrayUtils.Locate(dg, key);

            if (index == ArrayUtils.Empty)
                return null;

            int i = index.Length == 1 ? index[0] : index[1]; //la prima occorrenza è quella della lista dei tag (eccezione per dg1)

            int sizeOfData = Convert.ToInt32(dg[i + key.Length]);

            return System.Text.Encoding.UTF8.GetString(dg.SubArray(i + key.Length + 1, sizeOfData));
        }

        /* Fa il parsing del FullName dividendolo in nome e cognome */
        static String[] parseFullName(String s)
        {
            return s.Split(new[] { "<<" }, StringSplitOptions.None); ;
        }

        /* Parsing di un indirizzo dividendolo in Via (eventuale), Città e provincia */
        static String[] parseAddress(String s)
        {

            return s.Split(new[] { "<" }, StringSplitOptions.None); ;
        }       
        /*Converte l'array di byte codificato in jpeg2000 in una bitmap*/
        public Bitmap ret_cie_bitmap()
        {
            //BitmapImageCreator.Register();
            var por = J2kImage.FromBytes(cie_jpg2k_image);
            return por.As<Bitmap>();
        }                           
        /* Parserizza la stringa contenente una data in fomrato aaaammgg in una con formato gg/mm/aaaa*/
        static public String getParsedData(String d)
        {
            if (d == null)
            {
                return "";
            }
            return d.Substring(6, 2) + "/" + d.Substring(4, 2) + "/" + d.Substring(0, 4);
        }
        public override string ToString()
        {
            return $"First name: {firstName}\n" +
                   $"Last name: {lastName}\n" +
                   $"Birth city: {birthCity}\n" +
                   $"Birth prov: {birthProv}\n" +
                   $"Birth date: {birthDate}\n" +
                   $"Address: {address}\n" +
                   $"Prov: {prov}\n" +
                   $"CF: {cf}\n" +
                   $"MRZ: {mrz}\n" +
                   $"Date issue: {dateIssue}\n" +
                   $"Date expire: {dateExpire}\n" +
                   $"City: {city}\n" +
                   $"Photo: omessa";
        }

        public string ToJsonString()
        {
            return new JavaScriptSerializer().Serialize(this);
        }
    }
}
