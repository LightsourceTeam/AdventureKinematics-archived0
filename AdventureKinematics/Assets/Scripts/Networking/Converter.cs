using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System;


public class Converter : MonoBehaviour
{
    // To Type
    public static int ToInt(byte[] data, int startIndex = 0)
    {
        return BitConverter.ToInt32(data, startIndex);
    }

    public static short ToShort(byte[] data, int startIndex = 0)
    {
        return BitConverter.ToInt16(data, startIndex);
    }

    public static long ToLong(byte[] data, int startIndex = 0)
    {
        return BitConverter.ToInt64(data, startIndex);
    }

    public static float ToFloat(byte[] data, int startIndex = 0)
    {
        return BitConverter.ToSingle(data, startIndex);
    }

    public static string ToString(byte[] data, int startIndex = 0)
    {
        return BitConverter.ToString(data, startIndex);
    }

    public static bool ToBool(byte[] data, int startIndex = 0)
    {
        return BitConverter.ToBoolean(data, startIndex);
    }

    // To Byte

    public static byte[] ToBytes(int data)
    {
        return BitConverter.GetBytes(data);
    }

    public static byte[] ToBytes(short data)
    {
        return BitConverter.GetBytes(data);
    }

    public static byte[] ToBytes(long data)
    {
        return BitConverter.GetBytes(data);
    }

    public static byte[] ToBytes(float data)
    {
        return BitConverter.GetBytes(data);
    }

    public static byte[] ToBytes(string data)
    {
        return Encoding.UTF8.GetBytes(data);
    }

    public static byte[] ToBytes(bool data)
    {
        return BitConverter.GetBytes(data);
    }

    public static byte[] ToBytes(byte data) { return new byte[] { data }; }

    public static byte[] Combine(params byte[][] arrays)
    {
        byte[] combined = new byte[0];

        foreach (byte[] array in arrays) Buffer.BlockCopy(array, 0, combined, combined.Length, array.Length);

        return combined;
    }
}