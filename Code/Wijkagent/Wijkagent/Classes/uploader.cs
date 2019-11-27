using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Wijkagent.Classes
{
    class Uploader
    {
        private static readonly HttpClient _Client = new HttpClient();


        public Uploader()
        {
        }

        public async Task<string> SendFileAsync(string file_data, string original_name, string folder, string filename_upload)
        {

            System.Diagnostics.Debug.WriteLine("Starting");

            using (var client = new HttpClient())
            {
                var values = new Dictionary<string, string>
                {
                     { "fileName", original_name },
                     { "fileData", file_data },
                     { "UploadDir", folder },
                     { "FileNameUpload", filename_upload }
                };

                System.Diagnostics.Debug.WriteLine("Values set");

                int limit = 2000;

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


                System.Diagnostics.Debug.WriteLine("Executing");
                var response = await client.PostAsync("http://141.138.137.63/uploader.php", content);
                
                System.Diagnostics.Debug.WriteLine(response);
                var responseString = await response.Content.ReadAsStringAsync();
                return responseString;
            }
        }
    }
}
