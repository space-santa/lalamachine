namespace Lalamachine
{
    public class SysInfo
    {
        public string appVersion()
        {
            return "4.0.2";
        }
        public string appName()
        {
            return "lalamachine";
        }
        public string lalaDir()
        {
            return LibLala.Constants.LALADIR;
        }
        public string license()
        {
            return "GPL v3";
        }
        public string copyright()
        {
            return "2015-2019 Claus Zirkel";
        }
    }
}
