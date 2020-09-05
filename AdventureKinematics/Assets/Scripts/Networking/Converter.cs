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
    public int ToInt(byte[] data)
    {
        int converted = BitConverter.ToInt32(data, 0);
        return converted;
    }

    public short ToShort(byte[] data)
    {
        short converted = BitConverter.ToInt16(data, 0);
        return converted;
    }

    public long ToLong(byte[] data)
    {
        long converted = BitConverter.ToInt64(data, 0);
        return converted;
    }

    public float ToFloat(byte[] data)
    {
        float converted = BitConverter.ToSingle(data, 0);
        return converted;
    }

    public string ToString(byte[] data)
    {
        string converted = BitConverter.ToString(data, 0);
        return converted;
    }

    public bool ToBool(byte[] data)
    {
        bool converted = BitConverter.ToBoolean(data, 0);
        return converted;
    }

    // To Byte

    public byte[] ToBytes(int data)
    {
        byte[] converted = BitConverter.GetBytes(data);
        return converted;
    }

    public byte[] ToBytes(short data)
    {
        byte[] converted = BitConverter.GetBytes(data);
        return converted;
    }

    public byte[] ToBytes(long data)
    {
        byte[] converted = BitConverter.GetBytes(data);
        return converted;
    }

    public byte[] ToBytes(float data)
    {
        byte[] converted = BitConverter.GetBytes(data);
        return converted;
    }

    public byte[] ToBytes(string data)
    {
        byte[] converted = Encoding.ASCII.GetBytes(data);
        return converted;
    }

    public byte[] ToBytes(bool data)
    {
        byte[] converted = BitConverter.GetBytes(data);
        return converted;
    }

}
