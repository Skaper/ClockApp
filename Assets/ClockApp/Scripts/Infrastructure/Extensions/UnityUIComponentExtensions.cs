using System;
using TMPro;

namespace UniRx
{
    public static partial class UnityUIComponentExtensions
    {
        public static IDisposable SubscribeToText(
            this IObservable<string> source,
            TextMeshProUGUI text)
        {
            return source.SubscribeWithState(text, (value, t) => t.text = value);
        }
    }
}