using System.IO;

namespace TsmXiL
{
    public class Request
    {
        public double Acceleration { get; set; }

        public byte[] GetByteArray()
        {
            using (var m = new MemoryStream())
            {
                using (var writer = new BinaryWriter(m))
                {
                    writer.Write(Acceleration);
                }

                return m.ToArray();
            }
        }
    }
}