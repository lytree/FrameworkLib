﻿using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace System.Buffers;

/// <summary>
/// 表示基于ArrayPool的BufferWriter
/// </summary>
[DebuggerDisplay("WrittenCount = {WrittenCount}")]
public sealed class ArrayPoolBufferWriter<T> : IWrittenBufferWriter<T>, IDisposable
{
    private int index = 0;
    private T[] buffer;
    private bool disposed = false;
    private const int defaultSizeHint = 256;

    /// <summary>
    /// 获取已数入的数据长度
    /// </summary>
    public int WrittenCount => this.index;

    /// <summary>
    /// 获取已数入的数据
    /// </summary>
    public ReadOnlySpan<T> WrittenSpan => this.buffer.AsSpan(0, this.index);

    /// <summary>
    /// 获取已数入的数据
    /// </summary>
    public ReadOnlyMemory<T> WrittenMemory => this.buffer.AsMemory(0, this.index);

    /// <summary>
    /// 获取已数入的数据
    /// </summary>
    /// <returns></returns>
    public ArraySegment<T> WrittenSegment => new(this.buffer, 0, this.index);

    /// <summary>
    /// 获取容量
    /// </summary>
    public int Capacity => this.buffer.Length;

    /// <summary>
    /// 获取剩余容量
    /// </summary>
    public int FreeCapacity => this.buffer.Length - this.index;


    /// <summary>
    /// 基于ArrayPool的BufferWriter
    /// </summary>
    /// <param name="initialCapacity">初始容量</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public ArrayPoolBufferWriter(int initialCapacity)
    {
        if (initialCapacity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(initialCapacity));
        }
        this.buffer = ArrayPool<T>.Shared.Rent(initialCapacity);
    }

    /// <summary>
    /// 清除数据
    /// </summary>
    public void Clear()
    {
        this.buffer.AsSpan(0, this.index).Clear();
        this.index = 0;
    }

    /// <summary>
    /// 设置向前推进
    /// </summary>
    /// <param name="count"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void Advance(int count)
    {
        if (count < 0 || this.index > this.buffer.Length - count)
        {
            throw new ArgumentOutOfRangeException(nameof(count));
        }

        this.index += count;
    }

    /// <summary>
    /// 返回用于写入数据的Memory
    /// </summary>
    /// <param name="sizeHint">意图大小</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <returns></returns>
    public Memory<T> GetMemory(int sizeHint = 0)
    {
        this.CheckAndResizeBuffer(sizeHint);
        return this.buffer.AsMemory(this.index);
    }

    /// <summary>
    /// 返回用于写入数据的Span
    /// </summary>
    /// <param name="sizeHint">意图大小</param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <returns></returns>
    public Span<T> GetSpan(int sizeHint = 0)
    {
        this.CheckAndResizeBuffer(sizeHint);
        return buffer.AsSpan(this.index);
    }

    /// <summary>
    /// 写入数据
    /// </summary>
    /// <param name="value"></param>
    public void Write(T value)
    {
        this.GetSpan(1)[0] = value;
        this.index += 1;
    }

    /// <summary>
    /// 写入数据
    /// </summary>
    /// <param name="value">值</param> 
    public void Write(ReadOnlySpan<T> value)
    {
        if (value.IsEmpty == false)
        {
            value.CopyTo(this.GetSpan(value.Length));
            this.index += value.Length;
        }
    }

    /// <summary>
    /// 检测和扩容
    /// </summary>
    /// <param name="sizeHint"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void CheckAndResizeBuffer(int sizeHint)
    {
        if (sizeHint < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(sizeHint));
        }

        if (sizeHint == 0)
        {
            sizeHint = defaultSizeHint;
        }

        if (sizeHint > this.FreeCapacity)
        {
            int currentLength = this.buffer.Length;
            var growBy = Math.Max(sizeHint, currentLength);
            var newSize = checked(currentLength + growBy);

            var newBuffer = ArrayPool<T>.Shared.Rent(newSize);
            Array.Copy(this.buffer, newBuffer, this.index);

            ArrayPool<T>.Shared.Return(this.buffer);
            this.buffer = newBuffer;
        }
    }

    /// <summary>
    /// 将对象进行回收
    /// </summary>
    public void Dispose()
    {
        if (this.disposed == false)
        {
            ArrayPool<T>.Shared.Return(this.buffer);
            GC.SuppressFinalize(this);
        }
        this.disposed = true;
    }

    /// <summary>
    /// 析构函数
    /// </summary>
    ~ArrayPoolBufferWriter()
    {
        this.Dispose();
    }
}