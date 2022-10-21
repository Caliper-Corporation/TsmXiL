using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace TsmXiL
{
    public class Response
    {
        public double Acceleration { get; set; }
        public double Speed { get; set; }

        public static Response GetResponseFromByteArray(byte[] bytes)
        {
            var bf = new BinaryFormatter();
            using (var m = new MemoryStream(bytes))
            {
                var obj = bf.Deserialize(m);
                return (Response) obj;
            }
        }
    }
}
