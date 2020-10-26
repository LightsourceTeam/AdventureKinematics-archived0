using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System;
using System.Linq;


public class Bytes : MonoBehaviour
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

    public static string ToString(byte[] data, int startIndex, int count)
    {
        if (data == null || data.Length == 0) throw new ArgumentException("Got an empty byte array!");
        if (count == -1) count = data.Length - startIndex;
        
        return Encoding.Unicode.GetString(data, startIndex, count); ;
    }

    public static string ToString(byte[] data, out int returnedStringCount, int startIndex = 0)
    {
        if (data == null || data.Length == 0) throw new ArgumentException("Got an empty byte array!");


        returnedStringCount = ToInt(data);

        return Encoding.Unicode.GetString(data, startIndex + 4, returnedStringCount);
    }

    public static string ToString(byte[] data, int startIndex = 0)
    {
        if (data == null || data.Length == 0) throw new ArgumentException("Got an empty byte array!");


        int returnedStringCount = ToInt(data);

        return Encoding.Unicode.GetString(data, startIndex + 4, returnedStringCount);

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

    public static byte[] ToBytes(string data, bool autosizedString = true)
    {
        if (data == null || data.Length == 0) throw new ArgumentException("Got an empty string!");

        byte[] dataBytes = Encoding.Unicode.GetBytes(data);
        
        if (autosizedString) dataBytes = ToBytes(dataBytes.Length).Concat(dataBytes).ToArray();

        return dataBytes;
    }

    public static byte[] ToBytes(bool data)
    {
        return BitConverter.GetBytes(data);
    }

    public static byte[] ToBytes(byte data) { return new byte[] { data }; }

    public static byte[] Combine(params byte[][] arrays)
    {

        if (arrays.Length == 0) return null;
        if (arrays.Length == 1) return arrays[0];


        byte[] combined = new byte[0];


        foreach (byte[] array in arrays) combined = combined.Concat(array).ToArray();

        return combined;
    }
}