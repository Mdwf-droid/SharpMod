using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace SharpMod.IO
{
    public class ModBinaryReader : BinaryReader
    {
        public ModBinaryReader(Stream baseStream)
            : base(baseStream)
        {

        }

        public void Seek(int offset, SeekOrigin origin)
        {
            BaseStream.Seek(offset, origin);
        }

        public virtual int Tell()
        {
            try
            {
                return (int)(BaseStream.Position);
            }
            catch (System.IO.IOException)
            {
                return -1;
            }
        }

        /*public int Read(byte[] buffer, int offset, int count)
        {
            return BaseStream.Read(buffer, offset, count);
        }*/

        /*public virtual sbyte ReadSByte()
        {
            try
            {
                return (sbyte)this.ReadByte();
            }
            catch (System.IO.IOException ioe1)
            {
                return -1;
            }
        }*/

        public virtual short ReadUByte()
        {
            try
            {
                return (short)this.ReadByte();
            }
            catch (System.IO.IOException ioe1)
            {
                throw ioe1;
            }
        }

        public virtual bool ReadUBytes(short[] buffer, int number)
        {
            int pos = 0; 
            while (number > 0)
            {
                buffer[pos++] = ReadUByte(); number--;
            } 
            return !isEOF();
        }

        public virtual int ReadMotorolaUWord()
        {
            int result = ((int)ReadUByte()) << 8;
            result = (int)((short)result | (short)ReadUByte());
            return result;
        }

        public virtual int ReadIntelUWord()/* _mm_read_I_UWORD*/
        {
            int result = ReadUByte();
            result |= ((int)ReadUByte()) << 8;
            return result;
        }


        public virtual short ReadMotorolaSWord()
        {
            short result = (short)(ReadUByte() << 8);
            result |= ReadUByte();
            return result;
        }

        public virtual bool ReadIntelUWords(int[] buffer, int number)
        {
            int pos = 0; while (number > 0)
            {
                buffer[pos++] = ReadIntelUWord(); number--;
            } return !isEOF();
        }

        

        public virtual short ReadIntelSWord()
        {
            short result = ReadUByte();
            result |= (short)(ReadUByte() << 8);
            return result;
        }

        public virtual int ReadMotorolaULong()
        {
            int result = (ReadMotorolaUWord()) << 16;
            result |= ReadMotorolaUWord();
            return result;
        }

        public virtual int ReadIntelULong()
        {
            int result = ReadIntelUWord();
            result |= ((int)ReadIntelUWord()) << 16;
            return result;
        }

        public virtual int ReadMotorolaSLong()
        {
            return ((int)ReadMotorolaULong());
        }

        public virtual int ReadIntelSLong()
        {
            return ((int)ReadIntelULong());
        }

        public string ReadString(int length)
        {
            try
            {
                byte[] tmpBuffer = new byte[length];
                this.Read(tmpBuffer, 0, length);

                return System.Text.UTF8Encoding.UTF8.GetString(tmpBuffer, 0, length).Trim(new char[] {'\0'});
            }
            catch (System.IO.IOException ioe1)
            {
                throw ioe1;
            }
        }

        public virtual bool ReadSBytes(sbyte[] buffer, int number)
        {
            int pos = 0; while (number > 0)
            {
                buffer[pos++] = ReadSByte(); number--;
            }

            return !isEOF();
        }

        public virtual bool readMotorolaSWords(short[] buffer, int number)
        {
            int pos = 0; while (number > 0)
            {
                buffer[pos++] = ReadMotorolaSWord(); number--;
            } return !isEOF();
        }

        public virtual bool readIntelSWords(short[] buffer, int number)
        {
            int pos = 0; while (number > 0)
            {
                buffer[pos++] = ReadIntelSWord(); number--;
            } return !isEOF();
        }

        // isEOF is basically a utility function to catch all the
        // IOExceptions from the dependandt functions.
        // It's also make the code look more like the original
        // C source because it corresponds to feof.
        public virtual bool isEOF()
        {
            try
            {
                return (BaseStream.Position >= BaseStream.Length);
            }
            catch (System.IO.IOException)
            {
                return true;
            }
        }

        public void Rewind()
        {
            Seek(0, SeekOrigin.Begin);
        }

    }
}
