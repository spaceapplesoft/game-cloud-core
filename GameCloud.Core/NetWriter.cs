using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace GameCloud.Core
{
    /*
    // Binary stream Writer. Supports simple types, buffers, arrays, structs, and nested types
    */
    public class NetWriter
    {
        const int k_MaxStringLength = 1024 * 32;
        NetworkBuffer m_Buffer;
        static Encoding s_Encoding;
        static byte[] s_StringWriteBuffer;

        public NetWriter()
        {
            m_Buffer = new NetworkBuffer();
            if (s_Encoding == null)
            {
                s_Encoding = new UTF8Encoding();
                s_StringWriteBuffer = new byte[k_MaxStringLength];
            }
        }

        public NetWriter(byte[] buffer)
        {
            m_Buffer = new NetworkBuffer(buffer);
            if (s_Encoding == null)
            {
                s_Encoding = new UTF8Encoding();
                s_StringWriteBuffer = new byte[k_MaxStringLength];
            }
        }

        public short Position { get { return (short)m_Buffer.Position; } }

        public byte[] ToArray()
        {
            var newArray = new byte[m_Buffer.AsArraySegment().Count];
            Array.Copy(m_Buffer.AsArraySegment().Array, newArray, m_Buffer.AsArraySegment().Count);
            return newArray;
        }

        public byte[] AsArray()
        {
            return AsArraySegment().Array;
        }

        internal ArraySegment<byte> AsArraySegment()
        {
            return m_Buffer.AsArraySegment();
        }

        // http://sqlite.org/src4/doc/trunk/www/varint.wiki

        public NetWriter WritePackedUInt32(UInt32 value)
        {
            if (value <= 240)
            {
                Write((byte)value);
                return this;
            }
            if (value <= 2287)
            {
                Write((byte)((value - 240) / 256 + 241));
                Write((byte)((value - 240) % 256));
                return this;
            }
            if (value <= 67823)
            {
                Write((byte)249);
                Write((byte)((value - 2288) / 256));
                Write((byte)((value - 2288) % 256));
                return this;
            }
            if (value <= 16777215)
            {
                Write((byte)250);
                Write((byte)(value & 0xFF));
                Write((byte)((value >> 8) & 0xFF));
                Write((byte)((value >> 16) & 0xFF));
                return this;
            }

            // all other values of uint
            Write((byte)251);
            Write((byte)(value & 0xFF));
            Write((byte)((value >> 8) & 0xFF));
            Write((byte)((value >> 16) & 0xFF));
            Write((byte)((value >> 24) & 0xFF));
            return this;
        }

        public NetWriter WritePackedUInt64(UInt64 value)
        {
            if (value <= 240)
            {
                Write((byte)value);
                return this;
            }
            if (value <= 2287)
            {
                Write((byte)((value - 240) / 256 + 241));
                Write((byte)((value - 240) % 256));
                return this;
            }
            if (value <= 67823)
            {
                Write((byte)249);
                Write((byte)((value - 2288) / 256));
                Write((byte)((value - 2288) % 256));
                return this;
            }
            if (value <= 16777215)
            {
                Write((byte)250);
                Write((byte)(value & 0xFF));
                Write((byte)((value >> 8) & 0xFF));
                Write((byte)((value >> 16) & 0xFF));
                return this;
            }
            if (value <= 4294967295)
            {
                Write((byte)251);
                Write((byte)(value & 0xFF));
                Write((byte)((value >> 8) & 0xFF));
                Write((byte)((value >> 16) & 0xFF));
                Write((byte)((value >> 24) & 0xFF));
                return this;
            }
            if (value <= 1099511627775)
            {
                Write((byte)252);
                Write((byte)(value & 0xFF));
                Write((byte)((value >> 8) & 0xFF));
                Write((byte)((value >> 16) & 0xFF));
                Write((byte)((value >> 24) & 0xFF));
                Write((byte)((value >> 32) & 0xFF));
                return this;
            }
            if (value <= 281474976710655)
            {
                Write((byte)253);
                Write((byte)(value & 0xFF));
                Write((byte)((value >> 8) & 0xFF));
                Write((byte)((value >> 16) & 0xFF));
                Write((byte)((value >> 24) & 0xFF));
                Write((byte)((value >> 32) & 0xFF));
                Write((byte)((value >> 40) & 0xFF));
                return this;
            }
            if (value <= 72057594037927935)
            {
                Write((byte)254);
                Write((byte)(value & 0xFF));
                Write((byte)((value >> 8) & 0xFF));
                Write((byte)((value >> 16) & 0xFF));
                Write((byte)((value >> 24) & 0xFF));
                Write((byte)((value >> 32) & 0xFF));
                Write((byte)((value >> 40) & 0xFF));
                Write((byte)((value >> 48) & 0xFF));
                return this;
            }

            // all others
            {
                Write((byte)255);
                Write((byte)(value & 0xFF));
                Write((byte)((value >> 8) & 0xFF));
                Write((byte)((value >> 16) & 0xFF));
                Write((byte)((value >> 24) & 0xFF));
                Write((byte)((value >> 32) & 0xFF));
                Write((byte)((value >> 40) & 0xFF));
                Write((byte)((value >> 48) & 0xFF));
                Write((byte)((value >> 56) & 0xFF));
            }
            return this;
        }

        public NetWriter Write(char value)
        {
            m_Buffer.WriteByte((byte)value);
            return this;
        }

        public NetWriter Write(byte value)
        {
            m_Buffer.WriteByte(value);
            return this;
        }

        public NetWriter Write(sbyte value)
        {
            m_Buffer.WriteByte((byte)value);
            return this;
        }

        public NetWriter Write(short value)
        {
            m_Buffer.WriteByte2((byte)(value & 0xff), (byte)((value >> 8) & 0xff));
            return this;
        }

        public NetWriter Write(ushort value)
        {
            m_Buffer.WriteByte2((byte)(value & 0xff), (byte)((value >> 8) & 0xff));
            return this;
        }

        public NetWriter Write(int value)
        {
            // little endian...
            m_Buffer.WriteByte4(
                (byte)(value & 0xff),
                (byte)((value >> 8) & 0xff),
                (byte)((value >> 16) & 0xff),
                (byte)((value >> 24) & 0xff));
            return this;
        }

        public NetWriter Write(uint value)
        {
            m_Buffer.WriteByte4(
                (byte)(value & 0xff),
                (byte)((value >> 8) & 0xff),
                (byte)((value >> 16) & 0xff),
                (byte)((value >> 24) & 0xff));
            return this;
        }

        public NetWriter Write(long value)
        {
            m_Buffer.WriteByte8(
                (byte)(value & 0xff),
                (byte)((value >> 8) & 0xff),
                (byte)((value >> 16) & 0xff),
                (byte)((value >> 24) & 0xff),
                (byte)((value >> 32) & 0xff),
                (byte)((value >> 40) & 0xff),
                (byte)((value >> 48) & 0xff),
                (byte)((value >> 56) & 0xff));
            return this;
        }

        public NetWriter Write(ulong value)
        {
            m_Buffer.WriteByte8(
                (byte)(value & 0xff),
                (byte)((value >> 8) & 0xff),
                (byte)((value >> 16) & 0xff),
                (byte)((value >> 24) & 0xff),
                (byte)((value >> 32) & 0xff),
                (byte)((value >> 40) & 0xff),
                (byte)((value >> 48) & 0xff),
                (byte)((value >> 56) & 0xff));
            return this;
        }

        static UIntFloat s_FloatConverter;

        public NetWriter Write(float value)
        {
            s_FloatConverter.floatValue = value;
            Write(s_FloatConverter.intValue);
            return this;
        }

        public NetWriter Write(double value)
        {
            s_FloatConverter.doubleValue = value;
            Write(s_FloatConverter.longValue);
            return this;
        }

        public NetWriter Write(decimal value)
        {
            Int32[] bits = decimal.GetBits(value);
            Write(bits[0]);
            Write(bits[1]);
            Write(bits[2]);
            Write(bits[3]);
            return this;
        }

        public NetWriter Write(string value)
        {
            if (value == null)
            {
                m_Buffer.WriteByte2(0, 0);
                return this;
            }

            int len = s_Encoding.GetByteCount(value);

            if (len >= k_MaxStringLength)
            {
                throw new IndexOutOfRangeException("Serialize(string) too long: " + value.Length);
            }

            Write((ushort)(len));
            int numBytes = s_Encoding.GetBytes(value, 0, value.Length, s_StringWriteBuffer, 0);
            m_Buffer.WriteBytes(s_StringWriteBuffer, (ushort)numBytes);
            return this;
        }

        public NetWriter Write(INetSerializable serializable)
        {
            if (serializable == null)
            {
                Write(false);
                return this;
            }

            Write(true);
            serializable.Serialize(this);
            return this;
        }

        public NetWriter Write(bool value)
        {
            if (value)
                m_Buffer.WriteByte(1);
            else
                m_Buffer.WriteByte(0);
            return this;
        }

        public NetWriter Write(Dictionary<string, string> value)
        {
            if (value == null)
            {
                Write(0);
                return this;
            }

            Write(value.Count);

            foreach (var pair in value)
            {
                Write(pair.Key);
                Write(pair.Value);
            }

            return this;
        }

        public NetWriter Write(byte[] buffer, int count)
        {
            if (count > UInt16.MaxValue)
            {
//                Logs.General.Error("NetWriter Write: buffer is too large (" + count + ") bytes. The maximum buffer size is 64K bytes.");
                return this;
            }
            m_Buffer.WriteBytes(buffer, (UInt16)count);
            return this;
        }

        public NetWriter Write(byte[] buffer, int offset, int count)
        {
            if (count > UInt16.MaxValue)
            {
                Logs.General.LogError("NetWriter Write: buffer is too large (" + count + ") bytes. The maximum buffer size is 64K bytes.");
                return this;
            }
            m_Buffer.WriteBytesAtOffset(buffer, (ushort)offset, (ushort)count);
            return this;
        }

        public NetWriter WriteBytesAndSize(byte[] buffer, int count)
        {
            if (buffer == null || count == 0)
            {
                Write((UInt16)0);
                return this;
            }

            if (count > UInt16.MaxValue)
            {
                Logs.General.LogError("NetWriter WriteBytesAndSize: buffer is too large (" + count + ") bytes. The maximum buffer size is 64K bytes.");
                return this;
            }

            Write((UInt16)count);
            m_Buffer.WriteBytes(buffer, (UInt16)count);
            return this;
        }

        //NOTE: this will write the entire buffer.. including trailing empty space!
        public NetWriter WriteBytesFull(byte[] buffer)
        {
            if (buffer == null)
            {
                Write((UInt16)0);
                return this;
            }
            if (buffer.Length > UInt16.MaxValue)
            {
                Logs.General.LogError("NetWriter WriteBytes: buffer is too large (" + buffer.Length + ") bytes. The maximum buffer size is 64K bytes.");
                return this;
            }
            Write((UInt16)buffer.Length);
            m_Buffer.WriteBytes(buffer, (UInt16)buffer.Length);
            return this;
        }

        public NetWriter SeekZero()
        {
            m_Buffer.SeekZero();
            return this;
        }

        public void SetPosition(uint position)
        {
            m_Buffer.SetPosition(position);
        }

        public NetWriter StartMessage(short msgType)
        {
            SeekZero();

            // two bytes for size, will be filled out in FinishMessage
            m_Buffer.WriteByte2(0, 0);

            // two bytes for message type
            Write(msgType);
            return this;
        }

        public NetWriter FinishMessage()
        {
            // writes correct size into space at start of buffer
            m_Buffer.FinishMessage();
            return this;
        }
    }
}