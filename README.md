# Pure.DI: singleton built from a `Func<T1, T>` delegate argument

Minimal repro for Pure.DI 2.5.6: a singleton reached only through a `Func<string, T>` delegate is created with the argument of the first delegate call instead of its bound dependency, and keeps it for all later calls.

[`ISSUE.md`](ISSUE.md) is the text to paste into a Pure.DI GitHub issue.

```
dotnet run
```

prints `Label = first-call` twice; both are expected to be `default`. Adding `.Root<S0>("S0")` to the setup changes the result to `default`.
