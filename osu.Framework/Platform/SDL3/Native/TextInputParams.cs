// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Drawing;
using System.Runtime.Versioning;
using SDL;
using static SDL.SDL3;

namespace osu.Framework.Platform.SDL3.Native
{
    public record TextInputParams(
        SDL_TextInputType Type,
        SDL_Capitalization Capitalization,
        bool Autocorrect,
        bool Multiline,
        Rectangle Area,
        int CursorOffset,
        [property: SupportedOSPlatform("android")]
        int? AndroidInputType = null
    )
    {
        internal void FillProps(SDL_PropertiesID props)
        {
            SDL_SetNumberProperty(props, SDL_PROP_TEXTINPUT_TYPE_NUMBER, (int)Type).ThrowIfFailed();
            SDL_SetNumberProperty(props, SDL_PROP_TEXTINPUT_CAPITALIZATION_NUMBER, (int)Capitalization).ThrowIfFailed();
            SDL_SetBooleanProperty(props, SDL_PROP_TEXTINPUT_AUTOCORRECT_BOOLEAN, Autocorrect).ThrowIfFailed();
            SDL_SetBooleanProperty(props, SDL_PROP_TEXTINPUT_MULTILINE_BOOLEAN, Multiline).ThrowIfFailed();

            if (OperatingSystem.IsAndroid() && AndroidInputType != null)
                SDL_SetNumberProperty(props, SDL_PROP_TEXTINPUT_ANDROID_INPUTTYPE_NUMBER, AndroidInputType.Value).ThrowIfFailed();
        }
    }
}
