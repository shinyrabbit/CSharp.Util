namespace MultiLanguage
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;

    [StructLayout(LayoutKind.Sequential, Pack=8)]
    internal struct _ULARGE_INTEGER
    {
        public ulong QuadPart;
    }
}
