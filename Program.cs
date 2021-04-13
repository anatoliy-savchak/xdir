using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace xdir
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                string path, mask;
                if (args.Length > 1)
                    mask = args[1];
                else mask = "*.*";

                if (args.Length > 0)
                    path = args[0];
                else path = Directory.GetCurrentDirectory();

                XmlDocument doc = DoDir(path, mask);
                {
                    StringBuilder builder = new StringBuilder();
                    /*
                    {
                        using (TextWriter writer = new Utf8StringWriter(builder)) // Error Line
                        {
                            doc.Save(writer);
                        }
                    }
                    */
                    {
                        XmlWriterSettings settings = new XmlWriterSettings
                        {
                            Indent = true,
                            OmitXmlDeclaration = true, 
                            IndentChars = "  ",
                            NewLineChars = "\n",
                            NewLineHandling = NewLineHandling.Replace,
                            Encoding = Encoding.UTF8
                        };
                        using (XmlWriter writer = XmlWriter.Create(builder, settings))
                        {
                            doc.Save(writer);
                        }
                    }
                    string result = builder.ToString();
                    Console.WriteLine(result);
                }

                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR!!! {e.Message}");
                return GetErrorCode(e);
            }
        }

        static XmlDocument DoDir(string path, string mask)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement dirElement = doc.CreateElement("dir");
            doc.AppendChild(dirElement);
            XmlElement filesElement = doc.CreateElement("files");
            dirElement.AppendChild(filesElement);
            bool dirSet = false;
            dirElement.SetAttribute("path", path);
            dirElement.SetAttribute("mask", mask);
            foreach (string s in Directory.GetFiles(path, mask))
            {
                XmlElement row = doc.CreateElement("file");
                FileInfo fileInfo = new FileInfo(s);
                row.SetAttribute("name", fileInfo.Name);
                if (!dirSet)
                {
                    filesElement.SetAttribute("dir", fileInfo.DirectoryName);
                    dirSet = true;
                }
                row.SetAttribute("size", fileInfo.Length.ToString());
                row.SetAttribute("created", fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss"));
                row.SetAttribute("ext", fileInfo.Extension);
                filesElement.AppendChild(row);
            }
            return doc;
        }

        public static int GetErrorCode(Exception e)
        {
            Win32Exception we = e as Win32Exception;
            if (we != null)
            {
                return we.ErrorCode;
            }
            else if (e.InnerException != null)
            {
                return GetErrorCode(e.InnerException);
            }
            else
            {
                return (e.HResult & 0xFFFF);
            }

        }

        public class Utf8StringWriter : StringWriter
        {
            public Utf8StringWriter(StringBuilder stringBuilder)
                : base(stringBuilder)
            {
            }

            public override Encoding Encoding { get { return Encoding.UTF8; } }
        }
    }
}
