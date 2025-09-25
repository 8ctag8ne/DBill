namespace CoreLib.Common
{
    public static class Constants
    {
        public const string DefaultDatabaseExtension = ".dbm";
        public const string DefaultJsonExtension = ".json";
        
        public static class FileTypes
        {
            public const string Text = "text/plain";
            public const string Json = "application/json";
            public const string Xml = "application/xml";
            public const string Csv = "text/csv";
            public const string Html = "text/html";
            public const string Markdown = "text/markdown";
        }

        public static class Validation
        {
            public const long DefaultMaxFileSize = 10 * 1024 * 1024; // 10MB
            public const int MaxIdentifierLength = 100;
        }

        public const string CurrentVersion = "1.0";
    }
}