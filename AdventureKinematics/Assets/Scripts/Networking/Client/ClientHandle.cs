using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System;


public class ClientHandle : MonoBehaviour
{
    public void Welcome(byte[] data)
    {
        string msg = Converter.ToString(data);

        Debug.Log(msg);
    }

}
