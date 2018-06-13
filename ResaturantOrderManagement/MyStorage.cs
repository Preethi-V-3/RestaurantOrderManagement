using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace ResaturantOrderManagement
{
    internal class MyStorage
    {
        internal static void WriteXML<T>(string file, T data)
        {
            FileStream stream;
            XmlSerializer xmlSer = new XmlSerializer(typeof(T));
            stream = new FileStream(file, FileMode.Create);
            xmlSer.Serialize(stream, data);
            stream.Close();
        }

        internal static T ReadXML<T>(string file)
        {
            try
            {
                //set the resources free
                using (StreamReader sr = new StreamReader(file))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                    return (T)xmlSerializer.Deserialize(sr);
                }
            }
            catch (Exception e)
            {
                return default(T);
            }
        }

        internal static T GetEmbeddedXML<T>(string file)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var nameSpace = assembly.GetName().Name;

            try
            {
                //set the resources free
                using (Stream stream = assembly.GetManifestResourceStream(nameSpace + "." + file))
                {
                    using (var reader = new StreamReader(stream))
                    {
                        XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
                        return (T)xmlSerializer.Deserialize(stream);
                    }
                }
            }
            catch (Exception e)
            {

                return default(T);
            }
        }
    }
}