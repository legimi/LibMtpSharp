using System;
using System.Runtime.InteropServices;
using System.Text;
using Optional;

namespace LibMtpSharpFrameworkMacOS.Utils
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
    }
}