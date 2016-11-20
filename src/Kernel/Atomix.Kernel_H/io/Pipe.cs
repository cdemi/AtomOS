﻿/*
* PROJECT:          Atomix Development
* LICENSE:          Copyright (C) Atomix Development, Inc - All Rights Reserved
*                   Unauthorized copying of this file, via any medium is
*                   strictly prohibited Proprietary and confidential.
* PURPOSE:          Pipe Class
* PROGRAMMERS:      Aman Priyadarshi (aman.eureka@gmail.com)
*/

using Atomix.Kernel_H.Core;
using Atomix.Kernel_H.Arch.x86;

namespace Atomix.Kernel_H.IO
{
    internal unsafe class Pipe
    {
        uint Buffer;
        uint BufferSize;
        bool[] BufferStatus;

        uint ReadingPointer;
        uint WritingPointer;

        internal readonly uint PacketSize;
        internal readonly uint PacketsCount;

        internal Pipe(uint aPacketSize, uint aPacketsCount)
        {
            PacketsCount = aPacketsCount;
            PacketSize = aPacketSize;
            BufferSize = PacketsCount * PacketSize;
            Buffer = Heap.kmalloc(BufferSize);
            BufferStatus = new bool[PacketsCount];

            ReadingPointer = WritingPointer = 0;
        }

        internal bool Write(byte[] aData, bool Hangup = true)
        {
            if (aData.Length != PacketSize)
                return false;

            while (Hangup && BufferStatus[WritingPointer]) ;

            if (BufferStatus[WritingPointer])
                return false;

            Memory.FastCopy(Buffer + WritingPointer * PacketSize, Native.GetContentAddress(aData), PacketSize);
            BufferStatus[WritingPointer] = true;

            WritingPointer = (WritingPointer + 1) % PacketsCount;
                WritingPointer = 0;
            return true;
        }

        internal bool Read(byte[] aData)
        {
            if (aData.Length != PacketSize)
                return false;

            while (!BufferStatus[ReadingPointer]) ;

            Memory.FastCopy(Native.GetContentAddress(aData), Buffer + ReadingPointer * PacketSize, PacketSize);
            BufferStatus[ReadingPointer] = false;

            ReadingPointer = (ReadingPointer + 1) % PacketsCount;
            return true;
        }

        internal void Close()
        {
            Heap.Free(Buffer, BufferSize);
            Heap.Free(BufferStatus);
            Heap.Free(this);
        }
    }
}