﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Graphics.OpenGL.Textures;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Textures;
using osuTK;
using osuTK.Graphics.ES30;

namespace osu.Framework.Graphics.OpenGL.Buffers
{
    internal class FrameBuffer : IFrameBuffer
    {
        public Texture Texture { get; }

        private readonly List<RenderBuffer> attachedRenderBuffers = new List<RenderBuffer>();
        private readonly OpenGLRenderer renderer;
        private readonly TextureGL textureGL;
        private readonly int frameBuffer;

        public FrameBuffer(OpenGLRenderer renderer, RenderbufferInternalFormat[]? renderBufferFormats = null, All filteringMode = All.Linear)
        {
            this.renderer = renderer;
            frameBuffer = GL.GenFramebuffer();
            Texture = renderer.CreateTexture(textureGL = new FrameBufferTexture(renderer, filteringMode), WrapMode.None, WrapMode.None);

            renderer.BindFrameBuffer(frameBuffer);

            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget2d.Texture2D, textureGL.TextureId, 0);
            renderer.BindTexture(0);

            if (renderBufferFormats != null)
            {
                foreach (var format in renderBufferFormats)
                    attachedRenderBuffers.Add(new RenderBuffer(renderer, format));
            }

            renderer.UnbindFrameBuffer(frameBuffer);
        }

        private Vector2 size = Vector2.One;

        /// <summary>
        /// Sets the size of the texture of this frame buffer.
        /// </summary>
        public Vector2 Size
        {
            get => size;
            set
            {
                if (value == size)
                    return;

                size = value;

                textureGL.Width = (int)Math.Ceiling(size.X);
                textureGL.Height = (int)Math.Ceiling(size.Y);
                textureGL.SetData(new TextureUpload());
                textureGL.Upload();
            }
        }

        /// <summary>
        /// Binds the framebuffer.
        /// <para>Does not clear the buffer or reset the viewport/ortho.</para>
        /// </summary>
        public void Bind()
        {
            renderer.BindFrameBuffer(frameBuffer);

            foreach (var buffer in attachedRenderBuffers)
                buffer.Bind(Size);
        }

        /// <summary>
        /// Unbinds the framebuffer.
        /// </summary>
        public void Unbind()
        {
            // See: https://community.arm.com/developer/tools-software/graphics/b/blog/posts/mali-performance-2-how-to-correctly-handle-framebuffers
            // Unbinding renderbuffers causes an invalidation of the relevant attachment of this framebuffer on embedded devices, causing the renderbuffers to remain transient.
            // This must be done _before_ the framebuffer is flushed via the framebuffer unbind process, otherwise the renderbuffer may be copied to system memory.
            foreach (var buffer in attachedRenderBuffers)
                buffer.Unbind();

            renderer.UnbindFrameBuffer(frameBuffer);
        }

        #region Disposal

        ~FrameBuffer()
        {
            renderer.ScheduleDisposal(b => b.Dispose(false), this);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool isDisposed;

        protected virtual void Dispose(bool disposing)
        {
            if (isDisposed)
                return;

            textureGL.Dispose();
            renderer.DeleteFrameBuffer(frameBuffer);

            foreach (var buffer in attachedRenderBuffers)
                buffer.Dispose();

            isDisposed = true;
        }

        #endregion

        private class FrameBufferTexture : TextureGL
        {
            public FrameBufferTexture(OpenGLRenderer renderer, All filteringMode = All.Linear)
                : base(renderer, 1, 1, true, filteringMode)
            {
                BypassTextureUploadQueueing = true;

                SetData(new TextureUpload());
                Upload();
            }

            public override int Width
            {
                get => base.Width;
                set => base.Width = Math.Clamp(value, 1, Renderer.MaxTextureSize);
            }

            public override int Height
            {
                get => base.Height;
                set => base.Height = Math.Clamp(value, 1, Renderer.MaxTextureSize);
            }
        }
    }
}
