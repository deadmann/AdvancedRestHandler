using System;

namespace Arh
{
    [Flags]
    public enum OverrideDirection
    {
        Serialization = 1,
        Deserialization = 2,
        Both = Serialization | Deserialization
    }
}