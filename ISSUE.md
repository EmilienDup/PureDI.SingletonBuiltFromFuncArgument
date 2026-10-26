# A singleton reached through a `Func<T1, T>` delegate is built with the delegate argument of the first call

**Pure.DI 2.5.6**, .NET SDK 10.0.402, macOS. Repro project attached (`dotnet run`).

`S0` is a singleton with a `string` dependency that has its own binding. `S0` is only reachable from the composition through a `Func<string, S3>` delegate (`S3 -> S2 -> S1 -> S0`, and `S2` also takes `S0` directly). The singleton is created with the `string` passed to the delegate on the first call, not with the bound value, and every later call sees that first argument. The `string` binding is reported as unused.

## Minimal code

```csharp
using Pure.DI;

var composition = new Composition();
var first = composition.Top.Make("first-call");
var second = composition.Top.Make("second-call");
Console.WriteLine($"singleton S0 after the first call:  Label = {first.Label}");
Console.WriteLine($"singleton S0 after the second call: Label = {second.Label}");
Console.WriteLine($"same instance = {ReferenceEquals(first, second)}");

public sealed class S0(string label) { public string Label => label; }
public sealed class S1(S0 s0) { public S0 S0 => s0; }
public sealed class S2(S1 a, S0 b) { public S0 S0 => b; }
public sealed class S3(S2 a, S1 b) { public S0 S0 => a.S0; }
public sealed class Top(Func<string, S3> factory) { public S0 Make(string label) => factory(label).S0; }

partial class Composition
{
    private static void Setup() => DI.Setup(nameof(Composition))
        .Bind<string>().To(_ => "default")
        .Bind<S0>().As(Lifetime.Singleton).To<S0>()
        .Bind<S1>().To<S1>()
        .Bind<S2>().To<S2>()
        .Bind<S3>().To<S3>()
        .Root<Top>("Top");
}
```

## Expected

```
singleton S0 after the first call:  Label = default
singleton S0 after the second call: Label = default
same instance = True
```

A singleton has one instance for the whole composition, so its dependencies should come from the bindings, not from whichever delegate call happens to create it first.

## Actual

```
warning DIW003: The binding is not used.   (the Bind<string>() line)

singleton S0 after the first call:  Label = first-call
singleton S0 after the second call: Label = first-call
same instance = True
```

Generated initialisation of the singleton, inside the delegate's lambda:

```csharp
_singletonS0299d = new global::S0(overriddenString);
```

The result also depends on unrelated parts of the setup: adding `.Root<S0>("S0")` to the same setup makes both calls return `default`, and a shorter chain (`Top(Func<string, S1>)`, `S1(S0)`) returns `default` as well. So the same singleton is built from the bound value or from the delegate argument depending on the shape of the graph around it.
