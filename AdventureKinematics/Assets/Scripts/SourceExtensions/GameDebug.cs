using System;
using System.Collections.Generic;
using UnityEngine;


class Gdebug
{

#if UNITY_EDITOR
    
    public static void Log(string text)
    {
        Debug.Log(text);    
    }

    
#else
    
    public static void Log(string text)
    {
        Console.WriteLine(text);    
    }

#endif

    public static void LogWarning(string text)
    {
        Debug.LogWarning(text);
    }


    public static void LogError(string text)
    {
        Debug.LogError(text);
    }

}