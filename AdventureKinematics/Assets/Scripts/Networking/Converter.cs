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
    public static int ToInt(byte[] data)
    {
        int converted = BitConverter.ToInt32(data, 0);
        return converted;
    }

    public static short ToShort(byte[] data)
    {
        short converted = BitConverter.ToInt16(data, 0);
        return converted;
    }

    public static long ToLong(byte[] data)
    {
        long converted = BitConverter.ToInt64(data, 0);
        return converted;
    }

    public static float ToFloat(byte[] data)
    {
        float converted = BitConverter.ToSingle(data, 0);
        return converted;
    }

    public static string ToString(byte[] data)
    {
        string converted = Encoding.UTF8.GetString(data);
        return converted;
    }

    public static bool ToBool(byte[] data)
    {
        bool converted = BitConverter.ToBoolean(data, 0);
        return converted;
    }

    // To Byte

    public static byte[] ToBytes(int data)
    {
        byte[] converted = BitConverter.GetBytes(data);
        return converted;
    }

    public static byte[] ToBytes(short data)
    {
        byte[] converted = BitConverter.GetBytes(data);
        return converted;
    }

    public static byte[] ToBytes(long data)
    {
        byte[] converted = BitConverter.GetBytes(data);
        return converted;
    }

    public static byte[] ToBytes(float data)
    {
        byte[] converted = BitConverter.GetBytes(data);
        return converted;
    }

    public static byte[] ToBytes(string data)
    {
        byte[] converted = Encoding.UTF8.GetBytes(data);
        return converted;
    }

    public static byte[] ToBytes(bool data)
    {
        byte[] converted = BitConverter.GetBytes(data);
        return converted;
    }

}
