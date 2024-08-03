﻿using Framework.Buffers.Internal;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Buffers;

sealed partial class EmptyByteBuffer : IByteBuffer
{
    public void AdvanceReader(int count) { }


    const int IndexNotFound = -1;
    public const int EmptyByteBufferHashCode = 1;
    static readonly ArraySegment<byte> EmptyBuffer = new ArraySegment<byte>(ArrayExtensions.ZeroBytes);
    static readonly ArraySegment<byte>[] EmptyBuffers = { EmptyBuffer };

    public EmptyByteBuffer(IByteBufferAllocator allocator)
    {
        if (allocator is null) { ThrowHelper.ThrowArgumentNullException(ExceptionArgument.allocator); }

        Allocator = allocator;
    }

    public int Capacity => 0;

    public IByteBuffer AdjustCapacity(int newCapacity) => throw ThrowHelper.GetNotSupportedException();

    public int MaxCapacity => 0;

    public IByteBufferAllocator Allocator { get; }

    public IByteBuffer Unwrap() => null;

    public bool IsDirect => true;

    public bool IsAccessible => true;

    public bool IsReadOnly => false;

    public IByteBuffer AsReadOnly() => Unpooled.UnmodifiableBuffer(this);

    public int ReaderIndex => 0;

    public IByteBuffer SetReaderIndex(int readerIndex) => CheckIndex(readerIndex);

    public int WriterIndex => 0;

    public IByteBuffer SetWriterIndex(int writerIndex) => CheckIndex(writerIndex);

    public IByteBuffer SetIndex(int readerIndex, int writerIndex)
    {
        _ = CheckIndex(readerIndex);
        _ = CheckIndex(writerIndex);
        return this;
    }

    public int ReadableBytes => 0;

    public int WritableBytes => 0;

    public int MaxWritableBytes => 0;

    public int MaxFastWritableBytes => 0;

    public bool IsWritable() => false;

    public bool IsWritable(int size) => false;

    public IByteBuffer Clear() => this;

    public IByteBuffer MarkReaderIndex() => this;

    public IByteBuffer ResetReaderIndex() => this;

    public IByteBuffer MarkWriterIndex() => this;

    public IByteBuffer ResetWriterIndex() => this;

    public IByteBuffer DiscardReadBytes() => this;

    public IByteBuffer DiscardSomeReadBytes() => this;

    public IByteBuffer EnsureWritable(int minWritableBytes)
    {
        if ((uint)minWritableBytes > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentException_PositiveOrZero(minWritableBytes, ExceptionArgument.minWritableBytes); }

        if (minWritableBytes != 0)
        {
            ThrowHelper.ThrowIndexOutOfRangeException();
        }
        return this;
    }

    public int EnsureWritable(int minWritableBytes, bool force)
    {
        if ((uint)minWritableBytes > SharedConstants.TooBigOrNegative) { ThrowHelper.ThrowArgumentException_PositiveOrZero(minWritableBytes, ExceptionArgument.minWritableBytes); }

        return 0u >= (uint)minWritableBytes ? 0 : 1;
    }

    public bool GetBoolean(int index) => throw ThrowHelper.GetIndexOutOfRangeException();

    public byte GetByte(int index) => throw ThrowHelper.GetIndexOutOfRangeException();

    public short GetShort(int index) => throw ThrowHelper.GetIndexOutOfRangeException();

    public short GetShortLE(int index) => throw ThrowHelper.GetIndexOutOfRangeException();

    public int GetUnsignedMedium(int index) => throw ThrowHelper.GetIndexOutOfRangeException();

    public int GetUnsignedMediumLE(int index) => throw ThrowHelper.GetIndexOutOfRangeException();

    public int GetInt(int index) => throw ThrowHelper.GetIndexOutOfRangeException();

    public int GetIntLE(int index) => throw ThrowHelper.GetIndexOutOfRangeException();

    public long GetLong(int index) => throw ThrowHelper.GetIndexOutOfRangeException();

    public long GetLongLE(int index) => throw ThrowHelper.GetIndexOutOfRangeException();

    public IByteBuffer GetBytes(int index, IByteBuffer destination, int dstIndex, int length) => CheckIndex(index, length);

    public IByteBuffer GetBytes(int index, byte[] destination) => CheckIndex(index, destination.Length);

    public IByteBuffer GetBytes(int index, byte[] destination, int dstIndex, int length) => CheckIndex(index, length);

    public IByteBuffer GetBytes(int index, Stream destination, int length) => CheckIndex(index, length);

    public ICharSequence GetCharSequence(int index, int length, Encoding encoding)
    {
        _ = CheckIndex(index, length);
        return null;
    }

    public string GetString(int index, int length, Encoding encoding)
    {
        _ = CheckIndex(index, length);
        return null;
    }

    public IByteBuffer SetBoolean(int index, bool value) => throw ThrowHelper.GetIndexOutOfRangeException();

    public IByteBuffer SetByte(int index, int value) => throw ThrowHelper.GetIndexOutOfRangeException();

    public IByteBuffer SetShort(int index, int value) => throw ThrowHelper.GetIndexOutOfRangeException();

    public IByteBuffer SetShortLE(int index, int value) => throw ThrowHelper.GetIndexOutOfRangeException();

    public IByteBuffer SetMedium(int index, int value) => throw ThrowHelper.GetIndexOutOfRangeException();

    public IByteBuffer SetMediumLE(int index, int value) => throw ThrowHelper.GetIndexOutOfRangeException();

    public IByteBuffer SetInt(int index, int value) => throw ThrowHelper.GetIndexOutOfRangeException();

    public IByteBuffer SetIntLE(int index, int value) => throw ThrowHelper.GetIndexOutOfRangeException();

    public IByteBuffer SetLong(int index, long value) => throw ThrowHelper.GetIndexOutOfRangeException();

    public IByteBuffer SetLongLE(int index, long value) => throw ThrowHelper.GetIndexOutOfRangeException();

    public IByteBuffer SetBytes(int index, IByteBuffer src, int length) => CheckIndex(index, length);

    public IByteBuffer SetBytes(int index, IByteBuffer src, int srcIndex, int length) => CheckIndex(index, length);

    public IByteBuffer SetBytes(int index, byte[] src) => CheckIndex(index, src.Length);

    public IByteBuffer SetBytes(int index, byte[] src, int srcIndex, int length) => CheckIndex(index, length);

    public Task<int> SetBytesAsync(int index, Stream src, int length, CancellationToken cancellationToken)
    {
        _ = CheckIndex(index, length);
        return TaskUtil.Zero;
    }

    public IByteBuffer SetZero(int index, int length) => CheckIndex(index, length);

    public int SetCharSequence(int index, ICharSequence sequence, Encoding encoding) => throw ThrowHelper.GetIndexOutOfRangeException();

    public int SetString(int index, string value, Encoding encoding) => throw ThrowHelper.GetIndexOutOfRangeException();

    public bool ReadBoolean() => throw ThrowHelper.GetIndexOutOfRangeException();

    public byte ReadByte() => throw ThrowHelper.GetIndexOutOfRangeException();

    public short ReadShort() => throw ThrowHelper.GetIndexOutOfRangeException();

    public short ReadShortLE() => throw ThrowHelper.GetIndexOutOfRangeException();

    public int ReadMedium() => throw ThrowHelper.GetIndexOutOfRangeException();

    public int ReadMediumLE() => throw ThrowHelper.GetIndexOutOfRangeException();

    public int ReadUnsignedMedium() => throw ThrowHelper.GetIndexOutOfRangeException();

    public int ReadUnsignedMediumLE() => throw ThrowHelper.GetIndexOutOfRangeException();

    public int ReadInt() => throw ThrowHelper.GetIndexOutOfRangeException();

    public int ReadIntLE() => throw ThrowHelper.GetIndexOutOfRangeException();

    public long ReadLong() => throw ThrowHelper.GetIndexOutOfRangeException();

    public long ReadLongLE() => throw ThrowHelper.GetIndexOutOfRangeException();

    public IByteBuffer ReadBytes(int length) => CheckLength(length);

    public IByteBuffer ReadBytes(IByteBuffer destination, int length) => CheckLength(length);

    public IByteBuffer ReadBytes(IByteBuffer destination, int dstIndex, int length) => CheckLength(length);

    public IByteBuffer ReadBytes(byte[] destination) => CheckLength(destination.Length);

    public IByteBuffer ReadBytes(byte[] destination, int dstIndex, int length) => CheckLength(length);

    public IByteBuffer ReadBytes(Stream destination, int length) => CheckLength(length);

    public ICharSequence ReadCharSequence(int length, Encoding encoding)
    {
        _ = CheckLength(length);
        return StringCharSequence.Empty;
    }

    public string ReadString(int length, Encoding encoding)
    {
        _ = CheckLength(length);
        return string.Empty;
    }

    public IByteBuffer SkipBytes(int length) => CheckLength(length);

    public IByteBuffer WriteBoolean(bool value) => throw ThrowHelper.GetIndexOutOfRangeException();

    public IByteBuffer WriteByte(int value) => throw ThrowHelper.GetIndexOutOfRangeException();

    public IByteBuffer WriteShort(int value) => throw ThrowHelper.GetIndexOutOfRangeException();

    public IByteBuffer WriteShortLE(int value) => throw ThrowHelper.GetIndexOutOfRangeException();

    public IByteBuffer WriteMedium(int value) => throw ThrowHelper.GetIndexOutOfRangeException();

    public IByteBuffer WriteMediumLE(int value) => throw ThrowHelper.GetIndexOutOfRangeException();

    public IByteBuffer WriteUnsignedMedium(int value) => throw ThrowHelper.GetIndexOutOfRangeException();

    public IByteBuffer WriteUnsignedMediumLE(int value) => throw ThrowHelper.GetIndexOutOfRangeException();

    public IByteBuffer WriteInt(int value) => throw ThrowHelper.GetIndexOutOfRangeException();

    public IByteBuffer WriteIntLE(int value) => throw ThrowHelper.GetIndexOutOfRangeException();

    public IByteBuffer WriteLong(long value) => throw ThrowHelper.GetIndexOutOfRangeException();

    public IByteBuffer WriteLongLE(long value) => throw ThrowHelper.GetIndexOutOfRangeException();

    public IByteBuffer WriteBytes(IByteBuffer src, int length) => CheckLength(length);

    public IByteBuffer WriteBytes(IByteBuffer src, int srcIndex, int length) => CheckLength(length);

    public IByteBuffer WriteBytes(byte[] src) => CheckLength(src.Length);

    public IByteBuffer WriteBytes(byte[] src, int srcIndex, int length) => CheckLength(length);

    public IByteBuffer WriteZero(int length) => CheckLength(length);

    public int WriteCharSequence(ICharSequence sequence, Encoding encoding) => throw ThrowHelper.GetIndexOutOfRangeException();

    public int WriteString(string value, Encoding encoding) => throw ThrowHelper.GetIndexOutOfRangeException();

    public int ForEachByte(int index, int length, IByteProcessor processor)
    {
        _ = CheckIndex(index, length);
        return IndexNotFound;
    }

    public int ForEachByteDesc(int index, int length, IByteProcessor processor)
    {
        _ = CheckIndex(index, length);
        return IndexNotFound;
    }

    public IByteBuffer Copy(int index, int length)
    {
        _ = CheckIndex(index, length);
        return this;
    }

    public IByteBuffer Slice() => this;

    public IByteBuffer RetainedSlice() => this;

    public IByteBuffer Slice(int index, int length) => CheckIndex(index, length);

    public IByteBuffer RetainedSlice(int index, int length) => CheckIndex(index, length);

    public IByteBuffer Duplicate() => this;

    public bool IsSingleIoBuffer => true;

    public int IoBufferCount => 1;

    public ArraySegment<byte> GetIoBuffer(int index, int length)
    {
        _ = CheckIndex(index, length);
        return EmptyBuffer;
    }

    public ArraySegment<byte>[] GetIoBuffers(int index, int length)
    {
        _ = CheckIndex(index, length);
        return EmptyBuffers;
    }

    public bool HasArray => true;

    public byte[] Array => ArrayExtensions.ZeroBytes;

    public byte[] ToArray() => ArrayExtensions.ZeroBytes;

    public int ArrayOffset => 0;

    public bool HasMemoryAddress => false;

    public ref byte GetPinnableMemoryAddress() => throw ThrowHelper.GetNotSupportedException();

    public IntPtr AddressOfPinnedMemory() => IntPtr.Zero;

    public bool IsContiguous => true;

    public override int GetHashCode() => EmptyByteBufferHashCode;

    public bool Equals(IByteBuffer buffer) => buffer is object && !buffer.IsReadable();

    public override bool Equals(object obj)
    {
        var buffer = obj as IByteBuffer;
        return Equals(buffer);
    }

    public int CompareTo(IByteBuffer buffer) => buffer.IsReadable() ? -1 : 0;

    public override string ToString() => string.Empty;

    public bool IsReadable() => false;

    public bool IsReadable(int size) => false;

    public int ReferenceCount => 1;

    public IReferenceCounted Retain() => this;

    public IByteBuffer RetainedDuplicate() => this;

    public IReferenceCounted Retain(int increment) => this;

    public IReferenceCounted Touch() => this;

    public IReferenceCounted Touch(object hint) => this;

    public bool Release() => false;

    public bool Release(int decrement) => false;

    public IByteBuffer ReadSlice(int length) => CheckLength(length);

    public IByteBuffer ReadRetainedSlice(int length) => CheckLength(length);

    public Task WriteBytesAsync(Stream stream, int length, CancellationToken cancellationToken)
    {
        _ = CheckLength(length);
        return TaskUtil.Completed;
    }

    [MethodImpl(InlineMethod.AggressiveInlining)]
    IByteBuffer CheckIndex(int index)
    {
        if ((uint)index > 0u) // != 0
        {
            ThrowHelper.ThrowIndexOutOfRangeException();
        }
        return this;
    }

    [MethodImpl(InlineMethod.AggressiveInlining)]
    IByteBuffer CheckIndex(int index, int length)
    {
        if ((uint)index > 0u || (uint)length > 0u)
        {
            CheckIndex0(index, length);
        }

        return this;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    void CheckIndex0(int index, int length)
    {
        if ((uint)length > SharedConstants.TooBigOrNegative)
        {
            ThrowHelper.ThrowArgumentException_PositiveOrZero(length, ExceptionArgument.length);
        }
        if ((uint)index > 0u || (uint)length > 0u)
        {
            ThrowHelper.ThrowIndexOutOfRangeException();
        }
    }

    [MethodImpl(InlineMethod.AggressiveInlining)]
    IByteBuffer CheckLength(int length)
    {
        if ((uint)length > 0u) { CheckLength0(length); }
        return this;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    void CheckLength0(int length)
    {
        if ((uint)length > SharedConstants.TooBigOrNegative)
        {
            ThrowHelper.ThrowArgumentException_PositiveOrZero(length, ExceptionArgument.length);
        }
        if ((uint)length > 0u)
        {
            ThrowHelper.ThrowIndexOutOfRangeException();
        }
    }
    public ReadOnlyMemory<byte> UnreadMemory => ReadOnlyMemory<byte>.Empty;
    public ReadOnlyMemory<byte> GetReadableMemory(int index, int count)
    {
        _ = CheckIndex(index, count);
        return ReadOnlyMemory<byte>.Empty;
    }

    public ReadOnlySpan<byte> UnreadSpan => ReadOnlySpan<byte>.Empty;
    public ReadOnlySpan<byte> GetReadableSpan(int index, int count)
    {
        _ = CheckIndex(index, count);
        return ReadOnlySpan<byte>.Empty;
    }

    public ReadOnlySequence<byte> UnreadSequence => ReadOnlySequence<byte>.Empty;
    public ReadOnlySequence<byte> GetSequence(int index, int count)
    {
        _ = CheckIndex(index, count);
        return ReadOnlySequence<byte>.Empty;
    }

    public Memory<byte> FreeMemory => Memory<byte>.Empty;
    public Memory<byte> GetMemory(int sizeHintt = 0) => Memory<byte>.Empty;
    public Memory<byte> GetMemory(int index, int count)
    {
        _ = CheckIndex(index, count);
        return Memory<byte>.Empty;
    }

    public void Advance(int count)
    {
        _ = CheckLength(count);
    }

    public Span<byte> FreeSpan => Span<byte>.Empty;
    public Span<byte> GetSpan(int sizeHintt = 0) => Span<byte>.Empty;
    public Span<byte> GetSpan(int index, int count)
    {
        _ = CheckIndex(index, count);
        return Span<byte>.Empty;
    }

    public int GetBytes(int index, Span<byte> destination)
    {
        _ = CheckIndex(index);
        return 0;
    }
    public int GetBytes(int index, Memory<byte> destination)
    {
        _ = CheckIndex(index);
        return 0;
    }

    public int ReadBytes(Span<byte> destination) => 0;
    public int ReadBytes(Memory<byte> destination) => 0;

    public IByteBuffer SetBytes(int index, in ReadOnlySpan<byte> src) => CheckIndex(index, src.Length);
    public IByteBuffer SetBytes(int index, in ReadOnlyMemory<byte> src) => CheckIndex(index, src.Length);

    public IByteBuffer WriteBytes(in ReadOnlySpan<byte> src) => CheckLength(src.Length);
    public IByteBuffer WriteBytes(in ReadOnlyMemory<byte> src) => CheckLength(src.Length);

    public int FindIndex(int index, int count, Predicate<byte> match)
    {
        _ = CheckIndex(index, count);
        return IndexNotFound;
    }

    public int FindLastIndex(int index, int count, Predicate<byte> match)
    {
        _ = CheckIndex(index, count);
        return IndexNotFound;
    }

    public int IndexOf(int fromIndex, int toIndex, byte value)
    {
        _ = CheckIndex(fromIndex, toIndex - fromIndex);
        return IndexNotFound;
    }

    public int IndexOf(int fromIndex, int toIndex, in ReadOnlySpan<byte> values)
    {
        _ = CheckIndex(fromIndex, toIndex - fromIndex);
        return IndexNotFound;
    }

    public int IndexOfAny(int fromIndex, int toIndex, byte value0, byte value1)
    {
        _ = CheckIndex(fromIndex, toIndex - fromIndex);
        return IndexNotFound;
    }

    public int IndexOfAny(int fromIndex, int toIndex, byte value0, byte value1, byte value2)
    {
        _ = CheckIndex(fromIndex, toIndex - fromIndex);
        return IndexNotFound;
    }

    public int IndexOfAny(int fromIndex, int toIndex, in ReadOnlySpan<byte> values)
    {
        _ = CheckIndex(fromIndex, toIndex - fromIndex);
        return IndexNotFound;
    }
}
