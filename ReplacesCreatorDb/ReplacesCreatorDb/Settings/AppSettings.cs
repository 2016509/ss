using Microsoft.Extensions.Configuration;

namespace ReplacesCreatorDb.Settings
{
    /// <summary>
    /// Application settings model
    /// </summary>
    public class AppSettings
    {
        
        /// <summary>
        /// DB settings data
        /// </summary>
        public DatabaseSettings Database { get; set; }

        public OtherSettings Other { get; set; }

        private static AppSettings _appSettings;

        /// <summary>
        /// Init app settings
        /// </summary>
        public AppSettings()
        {
            _appSettings = this;
        }

        /// <summary>
        /// Current application settings
        /// </summary>
        public static AppSettings Current
        {
            get
            {
                if (_appSettings == null)
                {
                    _appSettings = GetCurrentSettings();
                }

                return _appSettings;
            }
        }

        /// <summary>
        /// Get current settings
        /// </summary>
        /// <returns><see cref="AppSettings"/></returns>
        private static AppSettings GetCurrentSettings()
        {
            var builder = new ConfigurationBuilder()
                            .AddIniFile($"settings.ini", optional: false);

            IConfigurationRoot configuration = builder.Build();

            var settings = new AppSettings();
            configuration.Bind(settings);
            return settings;
        }



    }
}
