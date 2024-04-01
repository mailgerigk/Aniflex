using System;

namespace Aniflex.Core;

public static class OnExit
{
    public static event Action? Do;

    public static void Invoke()
    {
        Do?.Invoke();
    }
}
