using System.Runtime.InteropServices;

namespace SocketDemo.Sniffer
{
    [StructLayout(LayoutKind.Explicit)]
    public struct IPHeader
    {
        /// <summary>
        /// 4位IP版本号+4位IIP包头长度
        /// </summary>
        [FieldOffset(0)]
        public byte VersionAndLength;

        /// <summary>
        /// 8位服务类型
        /// </summary>
        [FieldOffset(1)]
        public byte TypeOfService;

        /// <summary>
        /// 16位总长度
        /// </summary>
        [FieldOffset(2)]
        public ushort TotalLength;

        /// <summary>
        /// 16位标识
        /// </summary>
        [FieldOffset(4)]
        public ushort Identifier;

        /// <summary>
        /// 3位标志位+13位报片便宜
        /// </summary>
        [FieldOffset(6)]
        public ushort FlagAndOffset;

        /// <summary>
        /// 8位生存时间
        /// </summary>
        [FieldOffset(8)]
        public byte TimeToLive;

        /// <summary>
        /// 8位协议（TCP,UDP或其它）
        /// </summary>
        [FieldOffset(9)]
        public byte[] Protocol;

        /// <summary>
        /// 16位IP包头校验和
        /// </summary>
        [FieldOffset(10)]
        public ushort Checksum;

        /// <summary>
        /// 32位源IP地址
        /// </summary>
        [FieldOffset(12)]
        public uint SourceAddress;

        /// <summary>
        /// 32位目的IP地址
        /// </summary>
        [FieldOffset(16)]
        public uint DestinationAddress;
    }
}