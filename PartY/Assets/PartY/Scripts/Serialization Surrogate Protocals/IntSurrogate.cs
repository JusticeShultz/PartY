using System;
using System.Runtime.Serialization;
using UnityEngine;

namespace PartY
{
    //https://docs.microsoft.com/en-us/dotnet/api/system.runtime.serialization.iserializationsurrogate?view=netframework-4.8
    public class IntSurrogate : ISerializationSurrogate
    {
        void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            int @int = (int)obj;
            info.AddValue("x", @int);
        }

        object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            int x = info.GetInt32("x");
            return x;
        }
    }
}