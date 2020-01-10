using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Wijkagent2.Classes
{
    class Uploader
    {
        private static readonly HttpClient _Client = new HttpClient();


        public Uploader()
        {
        }

        //Async method that send files to the VPS server
        public async Task<string> SendFileAsync(string file_data, string original_name, string folder, string filename_upload)
        {

            using (var client = new HttpClient())
            {
                //Setting POST values
                var values = new Dictionary<string, string>
                {
                     { "fileName", original_name },
                     { "fileData", file_data },
                     { "UploadDir", folder },
                     { "FileNameUpload", filename_upload }
                };

                int limit = 2000;
                //Building the post String with the file
                StringContent content = new StringContent(values.Aggregate(new StringBuilder(), (sb, nxt) => {
                    StringBuilder sbInternal = new StringBuilder();
                    if (sb.Length > 0)
                    {
                        sb.Append("&");
                    }

                    int loops = nxt.Value.Length / limit;

                    for (int i = 0; i <= loops; i++)
                    {
                        if (i < loops)
                        {
                            sbInternal.Append(Uri.EscapeDataString(nxt.Value.Substring(limit * i, limit)));
                        }
                        else
                        {
                            sbInternal.Append(Uri.EscapeDataString(nxt.Value.Substring(limit * i)));
                        }
                    }

                    return sb.Append(nxt.Key + "=" + sbInternal.ToString());
                }).ToString(), Encoding.UTF8, "application/x-www-form-urlencoded");


                //Post the data to the server
                var response = await client.PostAsync("http://141.138.137.63/uploader.php", content);
                
                var responseString = await response.Content.ReadAsStringAsync();
                //Return the file path of the added file to the server
                return "http://141.138.137.63/" + folder + filename_upload;
            }
        }
    }
}
