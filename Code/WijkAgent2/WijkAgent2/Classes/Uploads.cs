using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wijkagent2.Classes
{
    class Uploads
    {
        public int UploadId { get; set; }
        public string Path { get; set; }
        public DateTime AddedDate { get; set; }

        public Uploads(int uploadId, string path)
        {
            UploadId = uploadId;
            Path = path;
        }

        public Uploads()
        {

        }
    }
}
