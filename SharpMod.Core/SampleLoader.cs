using System;
using SharpMod.IO;

namespace SharpMod
{
    ///<summary>
    ///</summary>
    public class SampleLoader
    {        
        private ModBinaryReader _reader;
        private SampleFormatFlags _inputFormat;
        private SampleFormatFlags _outputFormat;
        private short _old;
        private short[] _buffer;

        ///<summary>
        ///</summary>
        public SampleLoader()
        {
            _buffer = new short[1024];
        }

        ///<summary>
        ///</summary>
        ///<param name="reader"></param>
        ///<param name="inputFormat"></param>
        ///<param name="outputFormat"></param>
        public virtual void Init(ModBinaryReader reader, SampleFormatFlags inputFormat, SampleFormatFlags outputFormat)
        {
            _old = 0;
            _reader = reader;
            _inputFormat = inputFormat;
            _outputFormat = outputFormat;
        }

        ///<summary>
        ///</summary>
        ///<param name="buffer"></param>
        ///<param name="offset"></param>
        ///<param name="length"></param>
        public virtual void Load(byte[] buffer, int offset, int length)
        {
            var out_index = offset;

            // compute number of samples to load 
            if ((_outputFormat & SampleFormatFlags.SF_16BITS) != 0)
                length >>= 1;

            while (length != 0)
            {

                var stodo = (short)((length < 1024) ? length : 1024);

                int t;
                if ((_inputFormat & SampleFormatFlags.SF_16BITS) != 0)
                {
                    if ((_inputFormat & SampleFormatFlags.SF_BIG_ENDIAN) != 0)                        
                        _reader.readMotorolaSWords(_buffer, stodo);
                    else                        
                        _reader.readIntelSWords(_buffer, stodo);
                }
                else
                {

                    var byte_buffer = new sbyte[stodo];
                    
                    try
                    {                        
                        _reader.Read((byte[])(Array)byte_buffer, 0, stodo);
                    }
                    catch (System.IO.IOException)
                    {
                    }
                    for (t = 0; t < stodo; t++)
                    {
                        _buffer[t] = (short)((byte_buffer[t] << 8));
                    }
                    byte_buffer = null;
                }

                if ((_inputFormat & SampleFormatFlags.SF_DELTA) != 0)
                {
                    for (t = 0; t < stodo; t++)
                    {
                        _buffer[t] = (short)(_buffer[t] + _old);
                        _old = _buffer[t];
                    }
                }

                if (((_inputFormat ^ _outputFormat) & SampleFormatFlags.SF_SIGNED) != 0)
                {
                    for (t = 0; t < stodo; t++)
                    {
                        _buffer[t] = (short)(_buffer[t] ^ 0x8000);
                    }
                }

                if ((_outputFormat & SampleFormatFlags.SF_16BITS) != 0)
                {
                    for (t = 0; t < stodo; t++)
                    {
                        buffer[out_index++] = (byte)(_buffer[t] & 0xFF);
                        buffer[out_index++] = (byte)((_buffer[t] >> 8) & 0xFF);
                    }
                }
                else
                {
                    for (t = 0; t < stodo; t++)
                        buffer[out_index++] = (byte)(_buffer[t] >> 8);
                }

                length -= stodo;
            }
        }
    }
}
