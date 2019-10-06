namespace ArdiLabs.Yuniql
{
    public class DbVersionArtifact
    {
        public int Id { get; set; }
        public string Version { get; set; }
        public byte[] Artifact { get; set; }
    }
}
