using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wijkagent2.Classes
{
    class Uploads
    {
        //Upload ID of FILE with GET and SET
        public int UploadId { get; set; }
        //Path of the FILE with GET and SET
        public string Path { get; set; }
        //Added date of the FILE with GET and SET
        public DateTime AddedDate { get; set; }

        //Constructor for a upload
        public Uploads(int uploadId, string path)
        {
            UploadId = uploadId;
            Path = path;
        }

        //Empty Constructor
        public Uploads()
        {

        }
    }
}
