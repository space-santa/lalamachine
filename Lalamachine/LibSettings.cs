namespace Lalamachine
{
    public class LibSettings : AbstractBaseSettings
    {
        public bool isActive { get; set; }
        public int libraryTopShelveHeight
        {
            get => GetIntValue("libraryTopShelveHeight");
            set => SetSettingsValue("libraryTopShelveHeight", value);
        }

        protected override void Init()
        {
            isActive = false;
            InitSettingsValue("libraryTopShelveHeight", 0);
        }

        protected override string targetPath()
        {
            return LibLala.Constants.LIB_SETTINGS_PATH;
        }
    }
}
