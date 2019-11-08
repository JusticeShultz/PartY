using System;
using System.Runtime.Serialization;
using UnityEngine;

namespace PartY
{
    //https://docs.microsoft.com/en-us/dotnet/api/system.runtime.serialization.iserializationsurrogate?view=netframework-4.8
    public class Vector2Surrogate : ISerializationSurrogate
    {
        void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
        {
            Vector2 vector = (Vector2)obj;
            info.AddValue("x", vector.x);
            info.AddValue("y", vector.y);
        }

        object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
        {
            float x = info.GetSingle("x");
            float y = info.GetSingle("y");
            return new Vector2(x, y);
        }
    }
}
