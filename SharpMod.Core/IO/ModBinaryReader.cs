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

        public virtual short ReadUByte()
        {
            return (short)ReadByte();
        }

        public virtual bool ReadUBytes(short[] buffer, int number)
        {
            int pos = 0;
            while (number > 0)
            {
                buffer[pos++] = ReadUByte(); number--;
            }
            return !IsEOF();
        }

        public virtual int ReadMotorolaUWord()
        {
            var result = ((int)ReadUByte()) << 8;
            result = (short)result | ReadUByte();
            return result;
        }

        public virtual int ReadIntelUWord()/* _mm_read_I_UWORD*/
        {
            var result = (int)ReadUByte();
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
            }
            return !IsEOF();
        }



        public virtual short ReadIntelSWord()
        {
            var result = ReadUByte();
            result |= (short)(ReadUByte() << 8);
            return result;
        }

        public virtual int ReadMotorolaULong()
        {
            var result = (ReadMotorolaUWord()) << 16;
            result |= ReadMotorolaUWord();
            return result;
        }

        public virtual int ReadIntelULong()
        {
            var result = ReadIntelUWord();
            result |= ReadIntelUWord() << 16;
            return result;
        }

        public virtual int ReadMotorolaSLong()
        {
            return ReadMotorolaULong();
        }

        public virtual int ReadIntelSLong()
        {
            return ReadIntelULong();
        }

        public string ReadString(int length)
        {
            byte[] tmpBuffer = new byte[length];
            Read(tmpBuffer, 0, length);

            return System.Text.UTF8Encoding.UTF8.GetString(tmpBuffer, 0, length).Trim('\0');
        }

        public virtual bool ReadSBytes(sbyte[] buffer, int number)
        {
            int pos = 0; while (number > 0)
            {
                buffer[pos++] = ReadSByte(); number--;
            }

            return !IsEOF();
        }

        public virtual bool ReadMotorolaSWords(short[] buffer, int number)
        {
            int pos = 0; while (number > 0)
            {
                buffer[pos++] = ReadMotorolaSWord(); number--;
            }
            return !IsEOF();
        }

        public virtual bool ReadIntelSWords(short[] buffer, int number)
        {
            int pos = 0; while (number > 0)
            {
                buffer[pos++] = ReadIntelSWord(); number--;
            }
            return !IsEOF();
        }

        // isEOF is basically a utility function to catch all the
        // IOExceptions from the dependandt functions.
        // It's also make the code look more like the original
        // C source because it corresponds to feof.
        public virtual bool IsEOF()
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
