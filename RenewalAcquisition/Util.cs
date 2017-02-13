using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace RenewalAcquisition
{
    public class Util
    {

        public static string CopyFile(HttpPostedFileBase hpf, string vendor)
        {
            System.Configuration.AppSettingsReader appReader = new System.Configuration.AppSettingsReader();
            string filePath = (string)appReader.GetValue("TempPath", typeof(string));
            filePath += vendor + "." +  hpf.FileName.Split('.')[1];
            Stream source = hpf.InputStream; //your source file
            Stream destination = File.OpenWrite(filePath); //your destination

            Copy(source, destination);

            source.Close();
            destination.Close();

            return filePath;
        }

        public static long Copy(Stream from, Stream to)
        {
            long copiedByteCount = 0;

            byte[] buffer = new byte[2 << 16];
            for (int len; (len = from.Read(buffer, 0, buffer.Length)) > 0;)
            {
                to.Write(buffer, 0, len);
                copiedByteCount += len;
            }
            to.Flush();

            return copiedByteCount;
        }

        public static bool IsFileInUse(string filePath)
        {
            FileStream stream = null;

            try
            {
                var file = new System.IO.FileInfo(filePath);
                stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
            }
            catch (IOException)
            {
                return true;
            }
            finally
            {
                if (stream != null)
                    stream.Close();
            }

            //file is not locked
            return false;
        }
    }
}