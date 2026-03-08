namespace CieReader.Utils
{
    public static class CieTags
    {
        public static readonly byte[] KEY_FULL_NAME = new byte[] { 0x5F, 0x0E };
        public static readonly byte[] KEY_FIRST_NAME = new byte[] { 0x5F, 0x2B };
        public static readonly byte[] KEY_LAST_NAME = new byte[] { 0x5F, 0x2A };
        public static readonly byte[] KEY_BIRTH_DATE = new byte[] { 0x5F, 0x57 };
        public static readonly byte[] KEY_SEX = new byte[] { 0x5F, 0x35 };
        public static readonly byte[] KEY_NATIONALITY = new byte[] { 0x5F, 0x2C };
        public static readonly byte[] KEY_BIRTH_ADDRESS = new byte[] { 0x5F, 0x11 };
        public static readonly byte[] KEY_ADDRESS = new byte[] { 0x5F, 0x42 };
        public static readonly byte[] KEY_CF = new byte[] { 0x5F, 0x10 };
        public static readonly byte[] KEY_DOCUMENT_NUMBER = new byte[] { 0x5F, 0x03 };
        public static readonly byte[] KEY_DATE_ISSUE = new byte[] { 0x5F, 0x26 };
        public static readonly byte[] KEY_DATE_EXPIRE = new byte[] { 0x5F, 0x24 };
        
        public static readonly byte[] KEY_AUTHORITY = new byte[] { 0x5F, 0x28 };
        public static readonly byte[] KEY_AUTHORITY_ALTERNATIVE = new byte[] { 0x5F, 0x19 };

        public static readonly byte[] KEY_MRZ = new byte[] { 0x5F, 0x1F };
    }
}
