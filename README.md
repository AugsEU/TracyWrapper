# Tracy Profiler Wrapper for C#

This repository contains a C# wrapper for the [Tracy Profiler](https://github.com/wolfpld/tracy). It provides simple helper functions to easily profile a C# application.

The wrapper uses [Tracy-CSharp](https://github.com/clibequilibrium/Tracy-CSharp) bindings to interface with Tracy.

![Example game profile](https://i.imgur.com/aQI7t7z.png)

## Installation

#### Get Tracy Profiler

This wrapper only supports Tracy v0.11.1. [Download here.](https://github.com/wolfpld/tracy/releases/tag/v0.11.1)

#### Nuget

[![NuGet ver](https://img.shields.io/nuget/v/TracyWrapper)](https://www.nuget.org/packages/TracyWrapper)
Get on nuget: https://www.nuget.org/packages/TracyWrapper

Or run in the package manager:

```
NuGet\Install-Package TracyWrapper -Version 0.20.0
```

#### Or from source

1) Clone this repository via URL: https://github.com/AugsEU/TracyWrapper.git
2) Add the .csproj to your solution.

## Usage

### 1. Initialise each thread you wish to profile

You must call `TracyWrapper.Profiler.InitThread();` exactly ONCE on each thread you wish to profile. You can supply your own custom display name, otherwise the thread's name is used.

E.g.
```csharp
// Called once at application start.
protected override void Initialize()
{
	TracyWrapper.Profiler.InitThread();

	/* Init logic */
}
```

### 2. Call the heartbeat function(optional)

The `TracyWrapper.Profiler.HeartBeat();` function can be called to mark the end of each frame. Call this right after you have presented to teh screen.

E.g.
```csharp
// Called once per frame.
protected override void Draw(GameTime gameTime)
{
	/* Game logic goes here */

    TracyWrapper.Profiler.HeartBeat();
}
```

### 3. Instrument your functions

Push a zone to begin profiling a section of code. Pop the zone when the section is over. We can provide a name and a color so we can then identify this block when viewing in the profiler. The color can be a System.Drawing.Color, or you can supply a uint directly. TracyWrapper.ZoneC has many preset uint constants you can use.

E.g.

```csharp
public void Update(GameTime gameTime)
{
	Profiler.PushProfileZone("Inputs", System.Drawing.Color.AliceBlue);
	/* Input polling logic. */
	Profiler.PopProfileZone();

    Profiler.PushProfileZone("Physics update", ZoneC.BLUE_VIOLET);
    /* Physics update. */
    Profiler.PopProfileZone();

    // Use a profile scope to automatically pop the profile zone.
    using (new TracyWrapper.ProfileScope("Tilemap update", ZoneC.RED))
	{
		/* Tilemap update.*/
	}
}
```

Make sure you don't forget to pop the profile zone or the profiler will crash.

## Documentation

For full documentation, check out the github pages: https://augseu.github.io/TracyWrapper/namespace_tracy_wrapper.html

## Conditional compilation

When it comes time to ship your application, you probably don't want to bundle it with the profiling code still enabled. You can either remove the instrumentation code manually, OR use the stubs project.

The stubs project is exactly the same as TracyWrapper but contains none of the implementation, thus you can swap out TracyWrapper for TracyWrapperStubs and your program will compile as before, but you won't have any profiling code.

[![NuGet ver](https://img.shields.io/nuget/v/TracyWrapperStubs)](https://www.nuget.org/packages/TracyWrapperStubs)
Get on nuget: https://www.nuget.org/packages/TracyWrapperStubs

Or run in the package manager:

```
NuGet\Install-Package TracyWrapperStubs -Version 0.20.0
```

## Profiling accuracy

The Tracy profiler running natively on C++ is extremely accurate. Purportedly it is nanosecond accurate.

Using Tracy through this C# wrapper is much less accurate, due to the fact we must allocate strings to begin a profiling block, and then interface with the C++ code through bindings.

From my measurements, profiling via "PushProfileZone" and "PopProfileZone" directly is more accurate than using the `TracyWrapper.ProfileScope` class. From my crude measurements:

**PushProfileZone/PopProfileZone** : ~150ns accuracy

**TracyWrapper.ProfileScope** : ~310ns accuracy

Any operation that takes less than 1μs = 1000 ns should not be profiled.
