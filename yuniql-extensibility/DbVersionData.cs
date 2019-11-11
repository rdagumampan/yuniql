namespace Yuniql.Extensibility
{
    public class DbVersionData
    {
        public int Id { get; set; }
        public string Version { get; set; }
        public byte[] Artifact { get; set; }
    }
}
