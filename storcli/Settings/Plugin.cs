using Newtonsoft.Json;
using System.ComponentModel;

namespace storcli.Settings
{
    class Plugin
    {
        [DefaultValue(@"storcli64.exe")]
        [JsonProperty(PropertyName = "storcli_file_path", DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
        public string StorcliFilePath { get; set; }
    }
}
