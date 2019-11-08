using System;
using System.Runtime.Serialization;
using UnityEngine;

namespace PartY
{
    //https://docs.microsoft.com/en-us/dotnet/api/system.runtime.serialization.iserializationsurrogate?view=netframework-4.8
    public class LongSurrogate : ISerializationSurrogate
    {
        void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            long @long = (long)obj;
            info.AddValue("x", @long);
        }

        object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            long x = info.GetInt64("x");
            return x;
        }
    }
}