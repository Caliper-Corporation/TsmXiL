using ProtoBuf;

namespace TsmXiL
{
    [ProtoContract]
    public class Request
    {
        [ProtoMember(1)]
        public double Acceleration { get; set; }
    }

    public class Response
    {
        [ProtoMember(1)]
        public double Acceleration { get; set; }
        [ProtoMember(2)]
        public double Speed { get; set; }
    }
}
