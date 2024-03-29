using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Ktx2Sharp;

public static partial class Ktx
{
    private const string LibraryName = "ktx";

    private static IntPtr _nativeLibraryHandle = IntPtr.Zero;

    private const string SrLibKtxNotInitialized =
        "Ktx not initialized. Call Ktx.Init somewhere in your application startup first";

    public static bool Init()
    {
        var executingAssembly = typeof(Ktx).Assembly;
        if (NativeLibrary.TryLoad(LibraryName, executingAssembly, DllImportSearchPath.AssemblyDirectory, out _nativeLibraryHandle))
        {
            return true;
        }

        Debug.WriteLine("Ktx: Unable to load native library");
        return false;
    }

    public static void Terminate()
    {
        if (_nativeLibraryHandle != IntPtr.Zero)
        {
            NativeLibrary.Free(_nativeLibraryHandle);
        }
    }

    public static unsafe KtxTexture* LoadFromMemory(ReadOnlyMemory<byte> data)
    {
        if (_nativeLibraryHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException(SrLibKtxNotInitialized);
        }
        KtxTexture* ktxTexture = null;
        var createFlagBits = KtxTextureCreateFlagBits.LoadImageDataBit;
        var result = _ktxTexture2CreateFromMemoryDelegate(data.Pin().Pointer, (IntPtr)data.Length, createFlagBits, &ktxTexture);
        return result != KtxErrorCode.KtxSuccess ? null : ktxTexture;
    }

    public static unsafe KtxTexture* LoadFromFile(string fileName)
    {
        if (_nativeLibraryHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException(SrLibKtxNotInitialized);
        }
        var fileNamePtr = Marshal.StringToHGlobalAnsi(fileName);
        KtxTexture* ktxTexture = null;
        var createFlagBits = KtxTextureCreateFlagBits.LoadImageDataBit;
        var result = _ktxTexture2CreateFromNamedFileDelegate(fileNamePtr, createFlagBits, &ktxTexture);
        Marshal.FreeHGlobal(fileNamePtr);
        return result != KtxErrorCode.KtxSuccess ? null : ktxTexture;
    }

    public static unsafe void Destroy(KtxTexture* texture)
    {
        if (_nativeLibraryHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException(SrLibKtxNotInitialized);
        }
        _ktxTexture2DestroyDelegate(texture);
    }

    public static unsafe bool NeedsTranscoding(KtxTexture* texture)
    {
        if (_nativeLibraryHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException(SrLibKtxNotInitialized);
        }
        return _ktxTexture2NeedsTranscodingDelegate(texture) == 1;
    }

    public static unsafe KtxErrorCode Transcode(KtxTexture* texture, TranscodeFormat transcodeFormat, TranscodeFlagBits transcodeFlagBits)
    {
        if (_nativeLibraryHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException(SrLibKtxNotInitialized);
        }
        return _ktxTexture2TranscodeBasisDelegate(texture, transcodeFormat, transcodeFlagBits);
    }

    public static unsafe uint GetNumComponents(KtxTexture* texture)
    {
        if (_nativeLibraryHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException(SrLibKtxNotInitialized);
        }
        return _ktxTexture2GetNumComponentsDelegate(texture);
    }

    public static unsafe uint GetImageOffset(KtxTexture* texture, uint mipLevel, uint layer, uint faceIndex)
    {
        if (_nativeLibraryHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException(SrLibKtxNotInitialized);
        }
        uint imageOffset = 0;
        var result = _ktxTexture2GetImageOffsetDelegate(texture, mipLevel, layer, faceIndex, &imageOffset);
        if (result == KtxErrorCode.KtxSuccess)
        {
            return imageOffset;
        }

        throw new InvalidOperationException("Handle this properly");
    }

    public static unsafe uint GetImageSize(KtxTexture* texture, uint mipLevel)
    {
        if (_nativeLibraryHandle == IntPtr.Zero)
        {
            throw new InvalidOperationException(SrLibKtxNotInitialized);
        }
        return _ktxTexture2GetImageSizeDelegate(texture, mipLevel);
    }
}
