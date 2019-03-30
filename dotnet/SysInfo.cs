namespace dotnet
{
    public class SysInfo
    {
        public string appVersion()
        {
            return "3.15.0";
        }
        public string appName()
        {
            return "lalamachine";
        }
        public string lalaDir()
        {
            return Config.LALADIR;
        }
    }
}