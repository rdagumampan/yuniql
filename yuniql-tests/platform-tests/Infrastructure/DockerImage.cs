using System.Text;

namespace Yuniql.PlatformTests.Infrastructure
{
    public class DockerImage
    {
        public string Name { get; set; }

        public string Image { get; set; }

        public string Tag { get; set; }

        public string Architecture { get; set; }
    }
}
