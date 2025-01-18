# Tracy Profiler Wrapper Stubs

This is a version of the [TracyWrapper](https://www.nuget.org/packages/TracyWrapper) package that contains no implementation.

## Conditional compilation

When it comes time to ship your application, you probably don't want to bundle it with the profiling code still enabled. You can either remove the instrumentation code manually, OR use the stubs project.

The stubs project is exactly the same as TracyWrapper but contains none of the implementation, thus you can swap out TracyWrapper for TracyWrapperStubs and your program will compile as before, but you won't have any profiling code.

[![NuGet ver](https://img.shields.io/nuget/v/TracyWrapperStubs)](https://www.nuget.org/packages/TracyWrapperStubs)
Get on nuget: https://www.nuget.org/packages/TracyWrapperStubs

Or run in the package manager:

```
NuGet\Install-Package TracyWrapperStubs -Version 0.20.2
```