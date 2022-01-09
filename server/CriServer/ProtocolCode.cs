﻿using System;

namespace CriServer
{
    class ProtocolCode
    {
        private string Value { get; set; }

        public ProtocolCode(string value)
        {
            Value = value;
        }

        public static ProtocolCode Register => new("00");
        public static ProtocolCode Login => new("01");
        public static ProtocolCode Logout => new("02");
        public static ProtocolCode Hello => new("03");
        public static ProtocolCode Search => new("04");
        public static ProtocolCode Chat => new("05");
        public static ProtocolCode Text => new("06");
        public static ProtocolCode GroupCreate => new("07");
        public static ProtocolCode GroupSearch => new("08");
        public static ProtocolCode GroupText => new("09");

        public override string ToString()
        {
            return Value;
        }

        public override bool Equals(Object obj)
        {
            if (obj is not ProtocolCode protocolCode)
                return false;

            return Value == protocolCode.Value;
        }
    }
}