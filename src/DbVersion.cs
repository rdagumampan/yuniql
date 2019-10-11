using System;

namespace ArdiLabs.Yuniql
{
    public class DbVersion
    {
        public int Id { get; set; }

        public string Version { get; set; }

        public DateTime DateInsertedUtc { get; set; }

        public string LastUserId { get; set; }

        public string Comment { get; set; }
    }
}
