

namespace servicematters_pdf_checker.Settings
{
    public sealed class ProxySettings
    {
        /// <summary>
        /// Type of proxy [HTTP/SOCKS5]
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// IP Proxy
        /// </summary>
        public string Ip { get; set; }

        /// <summary>
        /// Port Proxy
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Login Proxy
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Password Proxy
        /// </summary>
        public string Password { get; set; }

    }
}
