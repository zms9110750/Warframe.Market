using System.Buffers;
using System.IO;
using WarframeMarketQuery.Extension;

namespace WarframeMarketQueryWPF.Tool;

public class ProgressStream(Stream innerStream, IObserver<long>? readObser = null, IObserver<long>? writeObser = null) : Stream
{
    private readonly Stream innerStream = innerStream ?? throw new ArgumentNullException(nameof(innerStream));
    private long totalBytesRead = 0;
    private long totalBytesWritten = 0;

    public override bool CanRead => innerStream.CanRead;
    public override bool CanSeek => innerStream.CanSeek;
    public override bool CanWrite => innerStream.CanWrite;
    public override bool CanTimeout => innerStream.CanTimeout;

    public override long Length => innerStream.Length;

    public override long Position
    {
        get => innerStream.Position;
        set => innerStream.Position = value;
    }

    public override int ReadTimeout
    {
        get => innerStream.ReadTimeout;
        set => innerStream.ReadTimeout = value;
    }

    public override int WriteTimeout
    {
        get => innerStream.WriteTimeout;
        set => innerStream.WriteTimeout = value;
    }

    #region 读取相关方法
    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        var bytesRead = await innerStream.ReadAsync(buffer.AsMemory(offset, count), cancellationToken);
        ReportReadProgress(bytesRead);
        return bytesRead;
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        var bytesRead = await innerStream.ReadAsync(buffer, cancellationToken);
        ReportReadProgress(bytesRead);
        return bytesRead;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        var bytesRead = innerStream.Read(buffer, offset, count);
        ReportReadProgress(bytesRead);
        return bytesRead;
    }

    public override int Read(Span<byte> buffer)
    {
        var bytesRead = innerStream.Read(buffer);
        ReportReadProgress(bytesRead);
        return bytesRead;
    }

    public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
    {
        // 重写 CopyToAsync 以确保进度报告
        var buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        try
        {
            int bytesRead;
            while ((bytesRead = await innerStream.ReadAsync(buffer, 0, bufferSize, cancellationToken)) > 0)
            {
                await destination.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                ReportReadProgress(bytesRead);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
    #endregion

    #region 写入相关方法
    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        await innerStream.WriteAsync(buffer, offset, count, cancellationToken);
        ReportWriteProgress(count);
    }

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
    {
        await innerStream.WriteAsync(buffer, cancellationToken);
        ReportWriteProgress(buffer.Length);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        innerStream.Write(buffer, offset, count);
        ReportWriteProgress(count);
    }

    public override void Write(ReadOnlySpan<byte> buffer)
    {
        innerStream.Write(buffer);
        ReportWriteProgress(buffer.Length);
    }
    #endregion

    #region 报告进度的方法
    private void ReportReadProgress(int bytesRead)
    {
        if (bytesRead <= 0 || readObser == null)
            return;

        // 原子增加读取字节数
        var newTotal = Interlocked.Add(ref totalBytesRead, bytesRead);

        // 使用内存屏障确保写入对其他线程可见
        Interlocked.MemoryBarrier();

        // 报告进度
        readObser.OnNext(newTotal);
    }

    private void ReportWriteProgress(int bytesWritten)
    {
        if (bytesWritten <= 0 || writeObser == null)
            return;

        // 原子增加写入字节数
        var newTotal = Interlocked.Add(ref totalBytesWritten, bytesWritten);

        // 使用内存屏障确保写入对其他线程可见
        Interlocked.MemoryBarrier();

        // 报告进度
        writeObser.OnNext(newTotal);
    }
    #endregion

    #region 其他Stream方法
    public override void Flush() => innerStream.Flush();

    public override Task FlushAsync(CancellationToken cancellationToken) =>
        innerStream.FlushAsync(cancellationToken);

    public override long Seek(long offset, SeekOrigin origin) => innerStream.Seek(offset, origin);

    public override void SetLength(long value) => innerStream.SetLength(value);

    public override void Close() => innerStream.Close();

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            innerStream.Dispose();
        }
        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        await innerStream.DisposeAsync();
        await base.DisposeAsync();
    }
    #endregion

    #region 进度相关属性
    /// <summary>
    /// 获取已读取的总字节数（使用原子读操作）
    /// </summary>
    public long TotalBytesRead
    {
        get
        {
            // 使用内存屏障确保读取最新的值
            Interlocked.MemoryBarrier();
            return Interlocked.Read(ref totalBytesRead);
        }
    }

    /// <summary>
    /// 获取已写入的总字节数（使用原子读操作）
    /// </summary>
    public long TotalBytesWritten
    {
        get
        {
            // 使用内存屏障确保读取最新的值
            Interlocked.MemoryBarrier();
            return Interlocked.Read(ref totalBytesWritten);
        }
    }

    /// <summary>
    /// 重置读取进度计数器
    /// </summary>
    public void ResetReadProgress()
    {
        var oldValue = Interlocked.Exchange(ref totalBytesRead, 0);
        if (oldValue != 0 && readObser != null)
        {
            readObser.OnNext(0);
        }
    }

    /// <summary>
    /// 重置写入进度计数器
    /// </summary>
    public void ResetWriteProgress()
    {
        var oldValue = Interlocked.Exchange(ref totalBytesWritten, 0);
        if (oldValue != 0 && writeObser != null)
        {
            writeObser.OnNext(0);
        }
    }

    /// <summary>
    /// 重置所有进度计数器
    /// </summary>
    public void ResetAllProgress()
    {
        ResetReadProgress();
        ResetWriteProgress();
    }
    #endregion
}