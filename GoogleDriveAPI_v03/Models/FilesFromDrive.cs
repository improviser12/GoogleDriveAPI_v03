using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GoogleDriveAPI_v03.Models
{
    public class FilesFromDrive
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public long? Size { get; set; }
        public long? Version { get; set; }
        public DateTime? CreatedTime { get; set; }
    }
}