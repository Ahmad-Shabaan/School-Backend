using Application.Interfaces;
using Microsoft.Extensions.Options;


namespace Infrastructure.Options
{
    public sealed class SettingsService : ISettings
    {
        private readonly Settings _settings;

        public SettingsService(IOptions<Settings> options)
        {
            _settings = options.Value;
        }
        public string DefaultPassword => _settings.DefaultPassword;

    }
}
