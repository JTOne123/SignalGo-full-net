﻿using SignalGo.Shared.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SignalGo.Server.IO
{
    internal class StreamGo : Stream
    {
        private PipeNetworkStream CurrentStream { get; set; }

        public StreamGo(PipeNetworkStream currentStream)
        {
            CurrentStream = currentStream;
        }


        private long _Length;

        private uint BoundarySize { get; set; }

        private long _Position;

        private bool IsReadFinishedBytes { get; set; } = false;

        public override long Length
        {
            get
            {
                return _Length;
            }
        }

        public override long Position
        {
            get
            {
                return _Position;
            }

            set
            {
                _Position = value;
            }
        }

        public override bool CanRead
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }

        public override bool CanSeek
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }

        public override bool CanWrite
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }


        /// <summary>
        /// set lenth of stream
        /// </summary>
        /// <param name="value"></param>
        public void SetOfStreamLength(long length, int _BoundarySize)
        {
            _Length = length - _BoundarySize;
            BoundarySize = (uint)_BoundarySize;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (IsReadFinishedBytes)
                return -1;
            if (count + Position > Length)
            {
                IsReadFinishedBytes = true;
                //Console.WriteLine("length:" + Length + "pos:" + Position);
                //Console.WriteLine("need take:" + (Length - Position + BoundarySize));
                byte[] endBuffer = SignalGoStreamBase.CurrentBase.ReadBlockSize(CurrentStream, (ulong)(Length - Position + BoundarySize));
                //Console.WriteLine("sizeTake:" + endBuffer.Length);
                if (endBuffer.Length == 0)
                    return 0;
                int needRead = (int)BoundarySize;
                //Console.WriteLine(endBuffer.Length + "&" + (endBuffer.Length - needRead) + " & " + needRead);
                string text = Encoding.UTF8.GetString(endBuffer.ToList().GetRange(endBuffer.Length - needRead, needRead).ToArray());
                int lineLen = 0;
                if (!text.StartsWith("\r\n"))
                {
                    lineLen = 2;
                    _Length -= 2;
                }
                //Console.WriteLine("ok&" + (endBuffer.Length - needRead - lineLen));
                List<byte> newBuffer = endBuffer.ToList().GetRange(0, endBuffer.Length - needRead - lineLen);
                if (newBuffer.Count == 0)
                    return -1;
                for (int i = 0; i < newBuffer.Count; i++)
                {
                    buffer[i] = newBuffer[i];
                }
                return newBuffer.Count;
            }
            if (count + Position > Length)
            {
                count = (int)(Length - Position);
                if (count <= 0)
                {
                    FinishRead();
                    return -1;
                }
            }
            byte[] readedBuffer = CurrentStream.Read(count, out int readCount);
            Array.Copy(readedBuffer, buffer, readCount);
            Position += readCount;
            if (Position == Length)
                FinishRead();
            return readCount;
        }

        private void FinishRead()
        {
            if (!IsReadFinishedBytes)
            {
                SignalGoStreamBase.CurrentBase.ReadBlockSize(CurrentStream, BoundarySize);
            }
            IsReadFinishedBytes = true;
        }

        public void Write(byte[] buffer)
        {
            CurrentStream.Write(buffer);
        }

        public override void Flush()
        {
            throw new System.NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new System.NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new System.NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            CurrentStream.Write(buffer);
        }
    }
}
