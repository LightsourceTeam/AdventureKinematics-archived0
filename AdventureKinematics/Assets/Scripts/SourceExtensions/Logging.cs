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
            Console.WriteLine($"  [i]  : {DateTime.UtcNow} : {message}");
            Console.ForegroundColor = color;
#else

            Debug.Log($"<color=lightblue> {message} </color>");
#endif
        }

        public static void LogAccept(object message)
        {
#if UNITY_SERVER
            ConsoleColor color = System.Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"  [+]  : {DateTime.UtcNow} : {message}");
            Console.ForegroundColor = color;
#else
            Debug.Log($"<color=green> {message} </color>");
#endif
        }

        public static void LogDeny(object message)
        {
#if UNITY_SERVER
            ConsoleColor color = System.Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  [-]  : {DateTime.UtcNow} : {message}");
            Console.ForegroundColor = color;
#else
            Debug.Log($"<color=red> {message} </color>");
#endif
        }

        public static void LogWarning(object message)
        {
#if UNITY_SERVER
            ConsoleColor color = System.Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"  [!]  : {DateTime.UtcNow} : {message}");
            Console.ForegroundColor = color;
#else
            Debug.LogWarning(message);
#endif
        }

        public static void LogError(object message)
        {
#if UNITY_SERVER
            ConsoleColor color = System.Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"  [x]  : {DateTime.UtcNow} : {message}");
            Console.ForegroundColor = color;
#else
            Debug.LogError(message);
#endif
        }

        public static void LogCritical(object message)
        {
#if UNITY_SERVER
            ConsoleColor color = System.Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine($"  [X]  : {DateTime.UtcNow} : {message}");
            Console.ForegroundColor = color;
#else
            //Debug.Log(" CRITICAL ERROR: " + message + );
            Debug.LogAssertion($"<color=#7c0a02ff> CRITICAL: </color> {message}");

#endif
        }
    }
}