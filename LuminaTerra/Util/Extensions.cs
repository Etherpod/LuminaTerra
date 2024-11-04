using OWML.Common;
using UnityEngine;

namespace LuminaTerra.Util;

public static class Extensions
{
    public static void LTPrint(object message)
    {
        LuminaTerra.Instance.ModHelper.Console.WriteLine(message.ToString(), MessageType.Info);
    }
}