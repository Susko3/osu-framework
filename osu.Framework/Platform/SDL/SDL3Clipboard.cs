// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using osu.Framework.Allocation;
using osu.Framework.Logging;
using SDL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;

namespace osu.Framework.Platform.SDL
{
    public class SDL3Clipboard(IImageFormat imageFormat) : Clipboard
    {
        private static IEnumerable<string> supportedImageMimeTypes => SixLabors.ImageSharp.Configuration.Default.ImageFormats.SelectMany(f => f.MimeTypes);

        // SDL cannot differentiate between string.Empty and no text (eg. empty clipboard or an image)
        // doesn't matter as text editors don't really allow copying empty strings.
        // assume that empty text means no text.
        public override string? GetText() => SDL3.SDL_HasClipboardText() == SDL_bool.SDL_TRUE ? SDL3.SDL_GetClipboardText() : null;

        public override void SetText(string text) => SDL3.SDL_SetClipboardText(text);

        public override Image<TPixel>? GetImage<TPixel>()
        {
            foreach (string mimeType in supportedImageMimeTypes)
            {
                if (tryGetData(mimeType, Image.Load<TPixel>, out var image))
                {
                    Logger.Log($"Found {mimeType} on clipboard");
                    return image;
                }
            }

            return null;
        }

        public override bool SetImage(Image image)
        {
            return trySetData(imageFormat.DefaultMimeType, () =>
            {
                using (var stream = new MemoryStream())
                {
                    image.Save(stream, imageFormat);
                    // the buffer is allowed to escape the lifetime of the MemoryStream:
                    // https://github.com/dotnet/runtime/blob/5535e31a712343a63f5d7d796cd874e563e5ac14/src/libraries/System.Private.CoreLib/src/System/IO/MemoryStream.cs#L123
                    return stream.GetBuffer().AsMemory(0, (int)stream.Length);
                }
            });
        }

        private delegate T? SpanTransform<out T>(ReadOnlySpan<byte> span);

        private static unsafe bool tryGetData<T>(string mimeType, SpanTransform<T> transform, out T? data)
        {
            if (SDL3.SDL_HasClipboardData(mimeType) == SDL_bool.SDL_FALSE)
            {
                data = default;
                return false;
            }

            UIntPtr nativeSize;
            IntPtr pointer = SDL3.SDL_GetClipboardData(mimeType, &nativeSize);

            if (pointer == IntPtr.Zero)
            {
                Logger.Log($"Failed to get SDL clipboard data for {mimeType}. SDL error: {SDL3.SDL_GetError()}");
                data = default;
                return false;
            }

            try
            {
                var nativeMemory = new ReadOnlySpan<byte>((void*)pointer, (int)nativeSize);
                data = transform(nativeMemory);
                return data != null;
            }
            catch (Exception e)
            {
                Logger.Error(e, $"Failed to decode clipboard data for {mimeType}.");
                data = default;
                return false;
            }
            finally
            {
                SDL3.SDL_free(pointer);
            }
        }

        private static unsafe bool trySetData(string mimeType, Func<ReadOnlyMemory<byte>> dataProvider)
        {
            var callbackContext = new ClipboardCallbackContext(mimeType, dataProvider);
            var objectHandle = new ObjectHandle<ClipboardCallbackContext>(callbackContext, GCHandleType.Normal);

            fixed (byte* ptr = Encoding.UTF8.GetBytes(mimeType + '\0'))
            {
                int ret = SDL3.SDL_SetClipboardData(&dataCallback, &cleanupCallback, objectHandle.Handle, &ptr, 1);

                if (ret < 0)
                    objectHandle.Dispose();

                return ret == 0;
            }
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static unsafe IntPtr dataCallback(IntPtr userdata, byte* mimeType, UIntPtr* length)
        {
            var objectHandle = new ObjectHandle<ClipboardCallbackContext>(userdata);

            if (!objectHandle.GetTarget(out var context))
            {
                *length = 0;
                return IntPtr.Zero;
            }

            Debug.Assert(context.MimeType == SDL3.PtrToStringUTF8(mimeType));

            var memory = context.GetAndPinData();
            *length = (UIntPtr)memory.Length;
            return context.Address;
        }

        [UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
        private static void cleanupCallback(IntPtr userdata)
        {
            var objectHandle = new ObjectHandle<ClipboardCallbackContext>(userdata);

            if (objectHandle.GetTarget(out var context))
            {
                context.Dispose();
                objectHandle.FreeUnsafe();
            }
        }

        private class ClipboardCallbackContext : IDisposable
        {
            public readonly string MimeType;

            private Func<ReadOnlyMemory<byte>> dataProvider;
            private MemoryHandle memoryHandle;

            public unsafe IntPtr Address => (IntPtr)memoryHandle.Pointer;

            public ClipboardCallbackContext(string mimeType, Func<ReadOnlyMemory<byte>> dataProvider)
            {
                MimeType = mimeType;
                this.dataProvider = dataProvider;
            }

            public ReadOnlyMemory<byte> GetAndPinData()
            {
                var data = dataProvider();
                dataProvider = null!;
                memoryHandle = data.Pin();
                return data;
            }

            public void Dispose()
            {
                memoryHandle.Dispose();
            }
        }
    }
}
