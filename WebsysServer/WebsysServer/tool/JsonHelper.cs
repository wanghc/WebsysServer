using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace WebsysServer.tool
{
    internal class JsonHelper
    {
        public static string escape(string text)
        {
            if (text == null) return null;
            if (text.Equals("")) return text;
            string rtn = "";
            char[] arr = text.ToCharArray();
            foreach (char letter in arr)
            {
                int a = Convert.ToInt32(letter);
                if (a>32 && a!=34 && a != 92)
                {
                    rtn +=  (char)a;
                }
                else
                {
                    //

                    rtn += @"\u00" + String.Format("{0:X2}", a); //String.Format("{0:00}", a.ToString("X"));
                }

            }
            return rtn;
        }
        //JSON字符串转对象
        public static T JsonToT<T>(string json)
        {
            //
            //using System.Runtime.Serialization.Json;

            /*var ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(json))
            {
                Position = 0
            };
            return (T)ser.ReadObject(stream);*/
            return default(T);
        }

        //对象转化为JSON字符串
        public static string TtoJson<T>(T obj)
        {
            /*var ser = new DataContractJsonSerializer(typeof(T));
            var stream = new MemoryStream();
            ser.WriteObject(stream, obj);
            var db = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(db, 0, (int)stream.Length);
            var dataString = Encoding.UTF8.GetString(db);
            return dataString;*/
            return null;
        }
        public static T JsonFileToT<T>(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return default(T);
            }
            StreamReader streamReader = File.OpenText(filePath);
            string s  = streamReader.ReadToEnd();
            s = s.Replace(@"\n","").Replace("\t","").Replace("\r","");
            streamReader.Close();
            streamReader.Dispose();
            return JsonToT<T>(s);
        }

    }
}
