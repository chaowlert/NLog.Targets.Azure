using NLog.Config;
using NLog.Layouts;

namespace NLog.Targets
{
    [NLogConfigurationItem]
    public class AzureProperty
    {
        [RequiredParameter]
        public string Name { get; set; }

        [RequiredParameter]
        public Layout Value { get; set; }
    }
}
