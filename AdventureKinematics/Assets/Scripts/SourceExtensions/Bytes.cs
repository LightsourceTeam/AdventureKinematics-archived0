using System.Text;
using System;
using System.Linq;

namespace SourceExtensions
{
    public class Bytes
    {
        //--------------------------------------------------
        #region FROM BYTES 



        public static int ToInt(byte[] data, int startIndex = 0)
        {
            if (data == null || data.Length == 0) throw new ArgumentException("Got an empty byte array!");
            return BitConverter.ToInt32(data, startIndex);
        }

        public static short ToShort(byte[] data, int startIndex = 0)
        {
            if (data == null || data.Length == 0) throw new ArgumentException("Got an empty byte array!");

            return BitConverter.ToInt16(data, startIndex);
        }

        public static long ToLong(byte[] data, int startIndex = 0)
        {
            if (data == null || data.Length == 0) throw new ArgumentException("Got an empty byte array!");

            return BitConverter.ToInt64(data, startIndex);
        }

        public static float ToFloat(byte[] data, int startIndex = 0)
        {
            if (data == null || data.Length == 0) throw new ArgumentException("Got an empty byte array!");

            return BitConverter.ToSingle(data, startIndex);
        }

        public static double ToDouble(byte[] data, int startIndex = 0)
        {
            if (data == null || data.Length == 0) throw new ArgumentException("Got an empty byte array!");

            return BitConverter.ToDouble(data, startIndex);
        }

        public static string ToString(byte[] data, out int stringSize, int startIndex = 0)
        {
            if (data == null || data.Length == 0) throw new ArgumentException("Got an empty byte array!");

            stringSize = ToInt(data);

            return Encoding.Unicode.GetString(data, startIndex + 4, stringSize);
        }

        public static string ToString(byte[] data, int startIndex = 0)
        {
            if (data == null || data.Length == 0) throw new ArgumentException("Got an empty byte array!");

            return Encoding.Unicode.GetString(data, startIndex + 4, ToInt(data));
        }

        public static bool ToBool(byte[] data, int startIndex = 0)
        {
            if (data == null || data.Length == 0) throw new ArgumentException("Got an empty byte array!");

            return BitConverter.ToBoolean(data, startIndex);
        }



        #endregion
        //--------------------------------------------------
        #region TO BYTES



        public static byte[] ToBytes(int data)
        {
            byte[] bytes = BitConverter.GetBytes(data);
            return bytes;
        }

        public static byte[] ToBytes(short data)
        {
            byte[] bytes = BitConverter.GetBytes(data);
            return bytes;
        }

        public static byte[] ToBytes(long data)
        {
            byte[] bytes = BitConverter.GetBytes(data);
            return bytes;
        }

        public static byte[] ToBytes(float data)
        {
            byte[] bytes = BitConverter.GetBytes(data);
            return bytes;
        }

        public static byte[] ToBytes(double data)
        {
            byte[] bytes = BitConverter.GetBytes(data);
            return bytes;
        }

        public static byte[] ToBytes(bool data)
        {
            byte[] bytes = BitConverter.GetBytes(data);
            return bytes;
        }

        public static byte[] ToBytes(string data)
        {
            if (data == null || data.Length == 0) throw new ArgumentException("Got an empty string!");

            byte[] dataBytes = Encoding.Unicode.GetBytes(data);
            dataBytes = ToBytes(dataBytes.Length).Concat(dataBytes).ToArray();

            return dataBytes;
        }

        public static byte[] ToBytes(byte data) { return new byte[] { data }; }



        #endregion
        //--------------------------------------------------
        #region UTILITIES



        public static byte[] Combine(params byte[][] arrays)
        {

            if (arrays.Length == 0) return null;
            if (arrays.Length == 1) return arrays[0];


            byte[] combined = new byte[0];

            foreach (byte[] array in arrays) combined = combined.Concat(array).ToArray();

            return combined;
        }

        public static void TestFunction()
        {
            var a = ToBytes(16);
            Logging.Log("Int: " + ToInt(a));

            var b = ToBytes((short)16);
            Logging.Log("Short: " + ToShort(b));

            var c = ToBytes((long)16);
            Logging.Log("Long: " + ToLong(c));

            var d = ToBytes("Hello World");
            Logging.Log("String: " + ToString(d));

            var e = ToBytes(16.456f);
            Logging.Log("Float: " + ToFloat(e));

            var f = ToBytes(16.456);
            Logging.Log("Double: " + ToDouble(f));

            var g = ToBytes(true);
            Logging.Log("Bool: " + ToBool(g));

            var h = ToBytes((byte)200);
            Logging.Log("Byte: " + h[0]);
        }

        public class Couple
        {
            //--------------------------------------------------
            #region VARIABLES/METHODS



            public int offset { get; private set; } = 0;
            public byte[] data { get; private set; }

            public Couple(byte[] data, int index = 0) { this.data = data; this.offset = index; }

            public Couple() { data = new byte[0]; }

            public static implicit operator Couple(byte[] data) => new Couple(data);
            public static implicit operator byte[](Couple data) => data.data.Skip(data.offset).ToArray();



            #endregion
            //--------------------------------------------------
            #region GET BYTES 



            public int GetInt()
            {
                if (data == null || data.Length == 0) throw new ArgumentException("Got an empty byte array!");

                var result = ToInt(data, offset);
                offset += 4;

                return result;
            }

            public short GetShort()
            {
                if (data == null || data.Length == 0) throw new ArgumentException("Got an empty byte array!");

                var result = ToShort(data, offset);
                offset += 2;

                return result;
            }

            public long GetLong()
            {
                if (data == null || data.Length == 0) throw new ArgumentException("Got an empty byte array!");
                
                var result = ToLong(data, offset);
                offset += 8;

                return result;
            }

            public float GetFloat()
            {
                if (data == null || data.Length == 0) throw new ArgumentException("Got an empty byte array!");

                var result = ToFloat(data, offset); ;
                offset += 4;

                return result;
            }

            public double GetDouble()
            {
                if (data == null || data.Length == 0) throw new ArgumentException("Got an empty byte array!");

                var result = ToDouble(data, offset);
                offset += 8;

                return result;
            }

            public string GetString(out int stringSize)
            {
                if (data == null || data.Length == 0) throw new ArgumentException("Got an empty byte array!");
               
                var result = Bytes.ToString(data, out stringSize, offset);
                offset += stringSize + 4;

                return result;
            }

            public bool GetBool()
            {
                if (data == null || data.Length == 0) throw new ArgumentException("Got an empty byte array!");

                var result = ToBool(data, offset);
                offset += 1;

                return result;
            }

            public byte GetByte()
            {
                if (data == null || data.Length == 0) throw new ArgumentException("Got an empty byte array!");

                var result = data[offset];
                offset += 1;

                return result;
            }



            #endregion
            //--------------------------------------------------
            #region ADD BYTES



            public void AddInt(int val) { data = data.Concat(ToBytes(val)).ToArray(); }

            public void AddShort(short val) { data = data.Concat(ToBytes(val)).ToArray(); }

            public void AddLong(long val) { data = data.Concat(ToBytes(val)).ToArray(); }

            public void AddFloat(float val) { data = data.Concat(ToBytes(val)).ToArray(); }

            public void AddDouble(double val) { data = data.Concat(ToBytes(val)).ToArray(); }

            public void AddString(string val) { data = data.Concat(ToBytes(val)).ToArray(); }

            public void AddBool(bool val) { data = data.Concat(ToBytes(val)).ToArray(); }

            public void AddByte(byte val) { data = data.Concat(ToBytes(val)).ToArray(); }



            #endregion
            //--------------------------------------------------
        }



        #endregion
        //--------------------------------------------------
    }
}