// // Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// // See the LICENCE file in the repository root for full licence text.
//
// using System;
// using System.Collections.Generic;
// using osu.Framework.Extensions;
// using osu.Framework.Extensions.TypeExtensions;
// using osuTK.Input;
//
// namespace osu.Framework.Input
// {
//     public readonly struct SomeKey : IKeyChar<Key>
//     {
//         public Key Key { get; }
//         public char Character { get; }
//
//         public SomeKey(Key key, char character)
//         {
//             Key = key;
//             Character = character;
//         }
//
//         //
//         //
//         // public SomeKey(Key key)
//         // {
//         //     Key = key;
//         //     Character = character;
//         // }
//
//         // public override string ToString()
//         // {
//         //     return (this as IKeyChar<Key>).ToString();
//         // }
//         //
//         // public string Lol()
//         // {
//         //     throw new NotImplementedException();
//         // }
//     }
//
//     public interface INotKeyChar<TKey>
//         where TKey : Enum
//     {
//         TKey Key { get; }
//         char Character { get; }
//
//         // protected virtual IKeyChar<T> FromKey()
//
//         // static IKeyChar<T> ForInputFromKey(T key)
//         // {
//         // }
//
//         static TKey AnyKey { get; }
//
//         // string Lol();
//
//         // public virtual string Lol()
//         // {
//         //     string c = Character == '\0' ? null : $", {Character.StringRepresentation()}";
//         //     return $@"{GetType().ReadableName()}({Key}{c})";
//         // }
//     }
//
//     public class Comp<TKey> : IComparer<IKeyChar<TKey>>
//         where TKey : Enum
//     {
//         public int Compare(IKeyChar<TKey> x, IKeyChar<TKey> y)
//         {
//             if (ReferenceEquals(x, y)) return 0;
//             if (ReferenceEquals(null, y)) return 1;
//             if (ReferenceEquals(null, x)) return -1;
//
//             int keyComparison = x.Key.CompareTo(y.Key);
//             if (keyComparison != 0)
//                 return keyComparison;
//
//             return x.Character.CompareTo(y.Character);
//         }
//     }
// }
