using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace SourceExtensions
{
    class Logging
    {
        public static void Log(object message)
        {
#if UNITY_SERVER
            Console.WriteLine(message);
#else
            Debug.Log(message);
#endif
        }

        public static void LogInfo(object message)
        {
#if UNITY_SERVER
            ConsoleColor color = System.Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine("  [i]  " + message);
            Console.ForegroundColor = color;
#else
            Debug.Log("<color=lightblue>" + message + "</color>");
#endif
        }

        public static void LogError(object message)
        {
#if UNITY_SERVER
            ConsoleColor color = System.Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  [x]  " + message);
            Console.ForegroundColor = color;
#else
            Debug.LogError(message);
#endif
        }

        public static void LogAccept(object message)
        {
#if UNITY_SERVER
            ConsoleColor color = System.Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("  [v]  " + message);
            Console.ForegroundColor = color;
#else
            Debug.Log("<color=green>" + message + "</color>");
#endif
        }

        public static void LogDeny(object message)
        {
#if UNITY_SERVER
            ConsoleColor color = System.Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  [x]  " + message);
            Console.ForegroundColor = color;
#else
            Debug.Log("<color=red>" + message + "</color>");
#endif
        }

        public static void LogWarning(object message)
        {
#if UNITY_SERVER
            ConsoleColor color = System.Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("  [!]  " + message);
            Console.ForegroundColor = color;
#else
            Debug.LogWarning(message);
#endif
        }
    }
}