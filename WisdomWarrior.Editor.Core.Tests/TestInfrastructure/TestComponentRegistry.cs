using System.Threading;
using WisdomWarrior.Engine.Core.Components;

namespace WisdomWarrior.Editor.Core.Tests.TestInfrastructure;

public static class TestComponentRegistry
{
    private static int _bootstrapped;

    public static void EnsureBootstrapped()
    {
        if (Interlocked.Exchange(ref _bootstrapped, 1) == 0)
        {
            ComponentRegistry.Bootstrap();
        }
    }
}
