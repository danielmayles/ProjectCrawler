using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Serializer
{
    public static byte[] GetBytes(Vector3 vector3)
    {
        List<byte> data = new List<byte>();
        data.AddRange(BitConverter.GetBytes(vector3.x));
        data.AddRange(BitConverter.GetBytes(vector3.y));
        data.AddRange(BitConverter.GetBytes(vector3.z));

        return data.ToArray();
    }

    public static Vector3 DeserializeToVector3(byte[] data, int StartIndex)
    {
        Vector3 vector3 = Vector3.zero;
        vector3.x = BitConverter.ToSingle(data, StartIndex);
        vector3.y = BitConverter.ToSingle(data, StartIndex + 4);
        vector3.z = BitConverter.ToSingle(data, StartIndex + 8);

        return vector3;
    }
}
