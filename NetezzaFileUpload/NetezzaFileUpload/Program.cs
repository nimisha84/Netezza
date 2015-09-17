using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevDefined.OAuth.Consumer;
using DevDefined.OAuth.Framework;
using System.IO;
using System.Net;
using System.Web;
using System.Globalization;
using System.Configuration;

namespace NetezzaFileUpload
{
    class Program
    {
        static void Main(string[] args)
        {
            string fileLocation= ConfigurationManager.AppSettings["DataFileLocation"];
            UploadFileFromNetezza(fileLocation);
        }

        private static string UploadFileFromNetezza (string docPath)
        {
            string sRet = "fail";

            try
            {
                string filename = Path.GetFileName(docPath); //docPath includes filename
                UTF8Encoding enc = new UTF8Encoding();
                string guid = Guid.NewGuid().ToString();
                string uri = string.Format("upload url");
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

                string boundary = guid;
                string boundaryStart = "--" + boundary; // << add two dashes at beginning
                string boundaryEnd = boundaryStart + "--";

                request.Method = "POST";
               

                request.ContentType = "multipart/form-data; boundary=" + boundary;
                request.Accept = "application/json";


                byte[] fileBytes = System.IO.File.ReadAllBytes(docPath);

                string jsonDisposition = "Content-Disposition: form-data; name=\"file_metadata_0\"";
                string jsonContentType = "Content-Type: application/json";
               
                string dataDispositionHeader = String.Format("Content-Disposition: form-data; name=\"file_content_0\"; filename=\"{0}\"", filename.ToLower());
                string dataTypeHeader = "Content-Type: " + MimeType(docPath);
                string dataContentXfer = "Content-Transfer-Encoding: " + GetEncoding(docPath);

                string oneCr = "\r\n";
                string twoCrs = "\r\n\r\n";

                request.ContentLength = boundaryStart.Count() +
                                            oneCr.Count() +
                                            jsonDisposition.Count() +
                                            oneCr.Count() +
                                            jsonContentType.Count() +
                                            twoCrs.Count() +
                                            boundaryStart.Count() +
                                            oneCr.Count() +
                                            dataDispositionHeader.Count() +
                                            oneCr.Count() +
                                            dataTypeHeader.Count() +
                                            oneCr.Count() +
                                            dataContentXfer.Count() +
                                            twoCrs.Count() +
                                            fileBytes.Length +
                                            oneCr.Count() +
                                            boundaryEnd.Count() +
                                            twoCrs.Count();

                using (System.IO.Stream requestStream = request.GetRequestStream())
                {
                    requestStream.Write(enc.GetBytes(boundaryStart), 0, boundaryStart.Count());
                    requestStream.Write(enc.GetBytes(oneCr), 0, oneCr.Count());

                    requestStream.Write(enc.GetBytes(jsonDisposition), 0, jsonDisposition.Count());
                    requestStream.Write(enc.GetBytes(oneCr), 0, oneCr.Count());

                    requestStream.Write(enc.GetBytes(jsonContentType), 0, jsonContentType.Count());
                    requestStream.Write(enc.GetBytes(twoCrs), 0, twoCrs.Count());

                    requestStream.Write(enc.GetBytes(boundaryStart), 0, boundaryStart.Count());
                    requestStream.Write(enc.GetBytes(oneCr), 0, oneCr.Count());

                    requestStream.Write(enc.GetBytes(dataDispositionHeader), 0, dataDispositionHeader.Count());
                    requestStream.Write(enc.GetBytes(oneCr), 0, oneCr.Count());

                    requestStream.Write(enc.GetBytes(dataTypeHeader), 0, dataTypeHeader.Count());
                    requestStream.Write(enc.GetBytes(oneCr), 0, oneCr.Count());

                    requestStream.Write(enc.GetBytes(dataContentXfer), 0, dataContentXfer.Count());
                    requestStream.Write(enc.GetBytes(twoCrs), 0, twoCrs.Count());

                    requestStream.Write(fileBytes, 0, fileBytes.Length);
                    requestStream.Write(enc.GetBytes(oneCr), 0, oneCr.Count());

                    requestStream.Write(enc.GetBytes(boundaryEnd), 0, boundaryEnd.Count());
                    requestStream.Write(enc.GetBytes(twoCrs), 0, twoCrs.Count());

                    requestStream.Close();
                }

                System.Net.HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                using (StreamReader streamReader = new StreamReader(response.GetResponseStream(), true))
                {
                    string result = streamReader.ReadToEnd();
                    sRet = result.Contains("FileAccessUri") ? "Success" : "Fail";
                }
            }
            catch (Exception e)
            {
                return "false";
            }

            return sRet;
        }



        private static string MimeType(string doc)
        {
            string sRet = "";

            string ext = Path.GetExtension(doc).ToLower();
            switch (ext)
            {
                case ".gif":
                    sRet = "image/gif";
                    break;
                case ".jpeg":
                case ".jpg":
                    sRet = "image/jpeg";
                    break;
                case ".png":
                    sRet = "image/png";
                    break;
                case ".pdf":
                    sRet = "application/pdf";
                    break;
                case ".txt":
                    sRet = "text/plain";
                    break;
            }

            return sRet;
        }



        private static string GetEncoding(string docPath)
        {
            string sRet = "";

            string ext = Path.GetExtension(docPath).ToLower();
            switch (ext)
            {
                case ".gif":
                case ".jpeg":
                case ".jpg":
                case ".png":
                    sRet = "binary";
                    break;
                case ".pdf":
                    sRet = "application/pdf";
                    break;
                case ".txt":
                    sRet = "text/plain";
                    break;
            }

            return sRet;
        }

            
    }
}
