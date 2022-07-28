using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TsmXiL
{
    [ProtoContract]
    internal class Request
    {
        [ProtoMember(1)]
        public double Acceleration { get; set; }
    }

    internal class Response
    {
        [ProtoMember(1)]
        public double Acceleration { get; set; }
        [ProtoMember(2)]
        public double Speed { get; set; }
    }
}
