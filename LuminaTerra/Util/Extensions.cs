using OWML.Common;
using UnityEngine;

namespace LuminaTerra.Util;

public static class Extensions
{
    public static void LTPrint(string message)
    {
        LuminaTerra.Instance.ModHelper.Console.WriteLine(message, MessageType.Info);
    }
}