using System;

namespace ArdiLabs.Yuniql
{
    public class DbVersion
    {
        public int Id { get; set; }
        public string Version { get; set; }
        public string Comment { get; set; }
        public DateTime Created { get; set; }
        public string CreatedBy { get; set; }
    }
}
