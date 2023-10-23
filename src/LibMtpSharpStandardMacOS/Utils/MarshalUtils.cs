using System;
using System.Runtime.InteropServices;
using System.Text;
using Optional;

namespace LibMtpSharpStandardMacOS.Utils
{
    public static class MarshalUtils
    {
        public static Option<string> PtrToStringUTF8(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                return Option.None<string>();

            var len = 0;
            while (Marshal.ReadByte(ptr, len) != 0)
                len++;

            var array = new byte[len];
            Marshal.Copy(ptr, array, 0, len);

            return Encoding.UTF8.GetString(array).SomeNotNull();
        }

        public static IntPtr StringToPtrUTF8(string? str)
        {
            if (string.IsNullOrEmpty(str))
                return IntPtr.Zero;

            var strBytes = Encoding.UTF8.GetBytes(str);
            var ptr = Marshal.AllocHGlobal(strBytes.Length + 1);
            Marshal.Copy(strBytes, 0, ptr, strBytes.Length);
            Marshal.WriteByte(ptr + strBytes.Length, 0);
            return ptr;
        }
    }
}