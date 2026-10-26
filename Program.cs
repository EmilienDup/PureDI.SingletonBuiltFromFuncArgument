using Pure.DI;

var composition = new Composition();
var first = composition.Top.Make("first-call");
var second = composition.Top.Make("second-call");
Console.WriteLine($"singleton S0 after the first call:  Label = {first.Label}");    // expected: default
Console.WriteLine($"singleton S0 after the second call: Label = {second.Label}");   // expected: default
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
