using System;
using System.Runtime.Serialization;
using UnityEngine;

namespace PartY
{
    //https://docs.microsoft.com/en-us/dotnet/api/system.runtime.serialization.iserializationsurrogate?view=netframework-4.8
    public class FloatSurrogate : ISerializationSurrogate
    {
        void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            float @float = (float)obj;
            info.AddValue("x", @float);
        }

        object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            float x = info.GetSingle("x");
            return x;
        }
    }
}