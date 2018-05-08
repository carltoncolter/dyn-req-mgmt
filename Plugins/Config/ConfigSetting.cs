using System.Xml.Serialization;

namespace Plugins.Common
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A configuration setting.
    /// </summary>
    [XmlType(TypeName = "setting")]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class ConfigSetting
    {
        /// <summary>
        /// The name of the configuration setting.
        /// </summary>
        [XmlAttribute("name")]
        public string Name { get; set; }

        /// <summary>
        /// The value of the configuration setting.
        /// </summary>
        [XmlText]
        public string Value { get; set; }
    }
}