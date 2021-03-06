﻿using ProtoBuf;
using System;
using System.IO;

namespace Dotnet.Serializing
{
    public class ProtocolBufSerializer : IBinarySerializer
    {
        public byte[] Serialize(object obj)
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, obj);
                return stream.ToArray();
            }
        }

        public T Deserialize<T>(byte[] data) where T : class
        {
            using (var stream = new MemoryStream(data))
            {
                return Serializer.Deserialize<T>(stream);
            }
        }

        public object Deserialize(byte[] data, Type type)
        {
            using (var stream = new MemoryStream(data))
            {
                return Serializer.NonGeneric.Deserialize(type, stream);
            }
        }
    }
}
