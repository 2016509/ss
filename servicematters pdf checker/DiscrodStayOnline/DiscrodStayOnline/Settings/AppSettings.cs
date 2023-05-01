using Microsoft.Extensions.Configuration;

namespace DiscrodStayOnline.Settings
{
    /// <summary>
    /// Application settings model
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Proxy Settings
        /// </summary>
        public Proxy Proxy { get; set; }

        private static AppSettings _appSettings;

        /// <summary>
        /// Init app settings
        /// </summary>
        public AppSettings()
        {
            _appSettings = this;
        }

        /// <summary>
        /// Currens application settings
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
