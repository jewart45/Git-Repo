using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SpeechNotifierWin10.Classes
{
    public class CustomXmlSerializer
    {
        private readonly Type type;

        public string FileName { get; private set; }

        private XmlSerializer InternalSerializer { get; set; }

        private FileStream fileStream { get; set; }

        public CustomXmlSerializer(string fileName, Type type)
        {
            FileName = fileName;

            InternalSerializer = new XmlSerializer(type);
        }

        public bool Serialize(object ob)
        {
            try
            {
                fileStream = File.Open(FileName, FileMode.Create);

                InternalSerializer.Serialize(fileStream, ob);
                fileStream.Close();
                return true;
            }
            catch (Exception ex)
            {
                string k = ex.ToString();
                fileStream.Close();
                return false;
            }
        }

        public object Deserialize()
        {
            try
            {
                fileStream = File.Open(FileName, FileMode.OpenOrCreate);
                object obj = InternalSerializer.Deserialize(fileStream);
                fileStream.Close();
                return obj;
            }
            catch (Exception ex)
            {
                string k = ex.ToString();
                fileStream.Close();
                return null;
            }
        }

        public void Close() => fileStream?.Close();
    }
}