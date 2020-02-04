using System;
using System.Collections.Generic;

namespace efsample.Models
{
    public partial class Yuniqldbversion
    {
        public short Id { get; set; }
        public string Version { get; set; }
        public DateTime Dateinsertedutc { get; set; }
        public DateTime Lastupdatedutc { get; set; }
        public string Lastuserid { get; set; }
        public byte[] Artifact { get; set; }
    }
}
