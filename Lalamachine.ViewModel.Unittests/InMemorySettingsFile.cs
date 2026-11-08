using System.Collections.Generic;
using Settings;

namespace Lalamachine.ViewModel.Unittests
{
    internal sealed class InMemorySettingsFile : SettingsFile
    {
        private Dictionary<string, string> _settings = [];

        public InMemorySettingsFile() : base("Test", "Test", "/nowhere") { }

        public override Dictionary<string, string> Load() => new(_settings);

        public override void Save(Dictionary<string, string> settings) => _settings = new(settings);
    }
}
