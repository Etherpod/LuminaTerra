using System.Collections.Generic;
using System.Linq;
using DitzyExtensions;
using OWML.Common;
using UnityEngine;

namespace LuminaTerra.Util;

public static class Extensions
{
    public static void LTPrint(object message)
    {
        LuminaTerra.Instance.ModHelper.Console.WriteLine(message.ToString(), MessageType.Info);
    }

    public static string AsString<T>(this IList<T> source) =>
        new[]
        {
            "[",
            source
                .Select(t => $"{t.ToString()}")
                .Join(", "),
            "]"
        }.Join("");

    public static string AsString<K, V>(this IDictionary<K, V> source) =>
        source
            .Select(kv => $"({kv.Key.ToString()}, {kv.Value.ToString()})")
            .Join("\n");
}