using System;
using System.Collections.Generic;
using System.Text;

namespace Sheev.Common.Models
{
    /// <summary>
    /// These are the settings needed for the JarJar service
    /// </summary>
    public class JarJarSetting
    {
        public string JarJar_DisplayName { get; set; }
        public string JarJar_Email { get; set; }
        public string JarJar_Username { get; set; }
        public string JarJar_Password { get; set; }
        public bool UseSendGrid { get; set; }
        public string SendGridKey { get; set; }
        public string MailServerAddress { get; set; }
    }
}
