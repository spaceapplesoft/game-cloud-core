﻿using System;
using System.Collections.Generic;
using System.Text;

namespace GameCloud.Core
{
    public class NetReader
    {
        NetworkBuffer m_buf;

        const int k_MaxStringLength = 1024 * 32;
        const int k_InitialStringBufferSize = 1024;
        static byte[] s_StringReaderBuffer;
        static Encoding s_Encoding;

        public NetReader()
        {
            m_buf = new NetworkBuffer();
            Initialize();
        }

        public NetReader(byte[] buffer)
        {
            m_buf = new NetworkBuffer(buffer);
            Initialize();
        }

        static void Initialize()
        {
            if (s_Encoding == null)
            {
                s_StringReaderBuffer = new byte[k_InitialStringBufferSize];
                s_Encoding = new UTF8Encoding();
            }
        }

        public uint Position { get { return m_buf.Position; } }
        public int Length { get { return m_buf.Length; } }

        public void SeekZero()
        {
            m_buf.SeekZero();
        }

        internal void Replace(byte[] buffer)
        {
            m_buf.Replace(buffer);
        }

        // http://sqlite.org/src4/doc/trunk/www/varint.wiki
        // NOTE: big endian.

        public UInt32 ReadPackedUInt32()
        {
            byte a0 = ReadByte();
            if (a0 < 241)
            {
                return a0;
            }
            byte a1 = ReadByte();
            if (a0 >= 241 && a0 <= 248)
            {
                return (UInt32)(240 + 256 * (a0 - 241) + a1);
            }
            byte a2 = ReadByte();
            if (a0 == 249)
            {
                return (UInt32)(2288 + 256 * a1 + a2);
            }
            byte a3 = ReadByte();
            if (a0 == 250)
            {
                return a1 + (((UInt32)a2) << 8) + (((UInt32)a3) << 16);
            }
            byte a4 = ReadByte();
            if (a0 >= 251)
            {
                return a1 + (((UInt32)a2) << 8) + (((UInt32)a3) << 16) + (((UInt32)a4) << 24);
            }
            throw new IndexOutOfRangeException("ReadPackedUInt32() failure: " + a0);
        }

        public UInt64 ReadPackedUInt64()
        {
            byte a0 = ReadByte();
            if (a0 < 241)
            {
                return a0;
            }
            byte a1 = ReadByte();
            if (a0 >= 241 && a0 <= 248)
            {
                return 240 + 256 * (a0 - ((UInt64)241)) + a1;
            }
            byte a2 = ReadByte();
            if (a0 == 249)
            {
                return 2288 + (((UInt64)256) * a1) + a2;
            }
            byte a3 = ReadByte();
            if (a0 == 250)
            {
                return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16);
            }
            byte a4 = ReadByte();
            if (a0 == 251)
            {
                return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24);
            }


            byte a5 = ReadByte();
            if (a0 == 252)
            {
                return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24) + (((UInt64)a5) << 32);
            }


            byte a6 = ReadByte();
            if (a0 == 253)
            {
                return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24) + (((UInt64)a5) << 32) + (((UInt64)a6) << 40);
            }


            byte a7 = ReadByte();
            if (a0 == 254)
            {
                return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24) + (((UInt64)a5) << 32) + (((UInt64)a6) << 40) + (((UInt64)a7) << 48);
            }


            byte a8 = ReadByte();
            if (a0 == 255)
            {
                return a1 + (((UInt64)a2) << 8) + (((UInt64)a3) << 16) + (((UInt64)a4) << 24) + (((UInt64)a5) << 32) + (((UInt64)a6) << 40) + (((UInt64)a7) << 48) + (((UInt64)a8) << 56);
            }
            throw new IndexOutOfRangeException("ReadPackedUInt64() failure: " + a0);
        }

        public byte ReadByte()
        {
            return m_buf.ReadByte();
        }

        public sbyte ReadSByte()
        {
            return (sbyte)m_buf.ReadByte();
        }

        public short ReadInt16()
        {
            ushort value = 0;
            value |= m_buf.ReadByte();
            value |= (ushort)(m_buf.ReadByte() << 8);
            return (short)value;
        }

        public ushort ReadUInt16()
        {
            ushort value = 0;
            value |= m_buf.ReadByte();
            value |= (ushort)(m_buf.ReadByte() << 8);
            return value;
        }

        public int ReadInt32()
        {
            uint value = 0;
            value |= m_buf.ReadByte();
            value |= (uint)(m_buf.ReadByte() << 8);
            value |= (uint)(m_buf.ReadByte() << 16);
            value |= (uint)(m_buf.ReadByte() << 24);
            return (int)value;
        }

        public uint ReadUInt32()
        {
            uint value = 0;
            value |= m_buf.ReadByte();
            value |= (uint)(m_buf.ReadByte() << 8);
            value |= (uint)(m_buf.ReadByte() << 16);
            value |= (uint)(m_buf.ReadByte() << 24);
            return value;
        }

        public long ReadInt64()
        {
            ulong value = 0;

            ulong other = m_buf.ReadByte();
            value |= other;

            other = ((ulong)m_buf.ReadByte()) << 8;
            value |= other;

            other = ((ulong)m_buf.ReadByte()) << 16;
            value |= other;

            other = ((ulong)m_buf.ReadByte()) << 24;
            value |= other;

            other = ((ulong)m_buf.ReadByte()) << 32;
            value |= other;

            other = ((ulong)m_buf.ReadByte()) << 40;
            value |= other;

            other = ((ulong)m_buf.ReadByte()) << 48;
            value |= other;

            other = ((ulong)m_buf.ReadByte()) << 56;
            value |= other;

            return (long)value;
        }

        public ulong ReadUInt64()
        {
            ulong value = 0;
            ulong other = m_buf.ReadByte();
            value |= other;

            other = ((ulong)m_buf.ReadByte()) << 8;
            value |= other;

            other = ((ulong)m_buf.ReadByte()) << 16;
            value |= other;

            other = ((ulong)m_buf.ReadByte()) << 24;
            value |= other;

            other = ((ulong)m_buf.ReadByte()) << 32;
            value |= other;

            other = ((ulong)m_buf.ReadByte()) << 40;
            value |= other;

            other = ((ulong)m_buf.ReadByte()) << 48;
            value |= other;

            other = ((ulong)m_buf.ReadByte()) << 56;
            value |= other;
            return value;
        }

        public decimal ReadDecimal()
        {
            Int32[] bits = new Int32[4];

            bits[0] = ReadInt32();
            bits[1] = ReadInt32();
            bits[2] = ReadInt32();
            bits[3] = ReadInt32();

            return new decimal(bits);
        }

        public float ReadSingle()
        {
            uint value = ReadUInt32();
            return FloatConversion.ToSingle(value);
        }

        public double ReadDouble()
        {
            ulong value = ReadUInt64();
            return FloatConversion.ToDouble(value);
        }

        public string ReadString()
        {
            UInt16 numBytes = ReadUInt16();
            if (numBytes == 0)
                return "";

            if (numBytes >= k_MaxStringLength)
            {
                throw new IndexOutOfRangeException("ReadString() too long: " + numBytes);
            }

            while (numBytes > s_StringReaderBuffer.Length)
            {
                s_StringReaderBuffer = new byte[s_StringReaderBuffer.Length * 2];
            }

            m_buf.ReadBytes(s_StringReaderBuffer, numBytes);

            char[] chars = s_Encoding.GetChars(s_StringReaderBuffer, 0, numBytes);
            return new string(chars);
        }

        public char ReadChar()
        {
            return (char)m_buf.ReadByte();
        }

        public Dictionary<string, string> ReadDictionary()
        {
            var count = ReadInt32();

            var dictionary = new Dictionary<string, string>(count);

            for (var i = 0; i < count; i++)
            {
                dictionary.Add(ReadString(), ReadString());
            }

            return dictionary;
        }

        public bool ReadBoolean()
        {
            int value = m_buf.ReadByte();
            return value == 1;
        }

        public byte[] ReadBytes(int count)
        {
            if (count < 0)
            {
                throw new IndexOutOfRangeException("NetReader ReadBytes " + count);
            }
            byte[] value = new byte[count];
            m_buf.ReadBytes(value, (uint)count);
            return value;
        }

        public byte[] ReadBytesAndSize()
        {
            ushort sz = ReadUInt16();
            if (sz == 0)
                return null;

            return ReadBytes(sz);
        }

        //public Vector2 ReadVector2()
        //{
        //    return new Vector2(ReadSingle(), ReadSingle());
        //}

        //public Vector3 ReadVector3()
        //{
        //    return new Vector3(ReadSingle(), ReadSingle(), ReadSingle());
        //}

        //public Vector4 ReadVector4()
        //{
        //    return new Vector4(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
        //}

        //public Color ReadColor()
        //{
        //    return new Color(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
        //}

        //public Color32 ReadColor32()
        //{
        //    return new Color32(ReadByte(), ReadByte(), ReadByte(), ReadByte());
        //}

        //public Quaternion ReadQuaternion()
        //{
        //    return new Quaternion(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
        //}

        //public Rect ReadRect()
        //{
        //    return new Rect(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
        //}

        //public Plane ReadPlane()
        //{
        //    return new Plane(ReadVector3(), ReadSingle());
        //}

        //public Ray ReadRay()
        //{
        //    return new Ray(ReadVector3(), ReadVector3());
        //}

        //public Matrix4x4 ReadMatrix4x4()
        //{
        //    Matrix4x4 m = new Matrix4x4();
        //    m.m00 = ReadSingle();
        //    m.m01 = ReadSingle();
        //    m.m02 = ReadSingle();
        //    m.m03 = ReadSingle();
        //    m.m10 = ReadSingle();
        //    m.m11 = ReadSingle();
        //    m.m12 = ReadSingle();
        //    m.m13 = ReadSingle();
        //    m.m20 = ReadSingle();
        //    m.m21 = ReadSingle();
        //    m.m22 = ReadSingle();
        //    m.m23 = ReadSingle();
        //    m.m30 = ReadSingle();
        //    m.m31 = ReadSingle();
        //    m.m32 = ReadSingle();
        //    m.m33 = ReadSingle();
        //    return m;
        //}

        public override string ToString()
        {
            return m_buf.ToString();
        }

        public void SetPosition(uint u)
        {
            m_buf.SetPosition(u);
        }

        public T Read<T>() where T : INetSerializable, new()
        {
            return Deserialize<T>();
        }

        public T Deserialize<T>() where T : INetSerializable, new()
        {
            if (!ReadBoolean())
            {
                return default(T);
            }

            var obj = new T();

            obj.Deserialize(this);
            return obj;
        }
    }
}