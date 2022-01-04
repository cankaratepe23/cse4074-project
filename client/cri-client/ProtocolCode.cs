using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cri_client
{
    class ProtocolCode
    {
        public string Value { get; private set; }
        private ProtocolCode(string value)
        {
            Value = value;
        }

        public static ProtocolCode Register { get { return new ProtocolCode("00");  } }
        public static ProtocolCode Login { get { return new ProtocolCode("01"); } }
        public static ProtocolCode Logout { get { return new ProtocolCode("02"); } }
        public static ProtocolCode Hello { get { return new ProtocolCode("03"); } }
        public static ProtocolCode Search { get { return new ProtocolCode("04"); } }
        public static ProtocolCode Chat { get { return new ProtocolCode("05"); } }
        public static ProtocolCode Text { get { return new ProtocolCode("06"); } }
        public static ProtocolCode GroupCreate { get { return new ProtocolCode("07"); } }
        public static ProtocolCode GroupSearch { get { return new ProtocolCode("08"); } }
        public static ProtocolCode GroupText { get { return new ProtocolCode("09"); } }

        public override string ToString()
        {
            return Value;
        }
    }
}
