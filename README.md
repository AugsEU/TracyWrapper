# Tracy Profiler Wrapper for C#

This repository contains a C# wrapper for the [Tracy Profiler](https://github.com/wolfpld/tracy). It provides simple helper functions to easily profile a C# application.

The wrapper uses [Tracy-CSharp](https://github.com/clibequilibrium/Tracy-CSharp) bindings to interface with Tracy.

## Installation


#### Get Tracy Profiler

This wrapper only supports Tracy v0.11.1. [Download here.](https://github.com/wolfpld/tracy/releases/tag/v0.11.1)

#### Nuget

[![NuGet ver](https://img.shields.io/nuget/v/TracyWrapper)](https://www.nuget.org/packages/TracyWrapper)
Get on nuget: https://www.nuget.org/packages/TracyWrapper

Or run in the pakcage manager:

```
NuGet\Install-Package TracyWrapper -Version 0.11.1
```

#### Or from source

1) Clone this repository via URL: https://github.com/AugsEU/TracyWrapper.git
2) Add the .csproj to your solution.

## Usage

### 1. Call the heartbeat function

The `TracyWrapper.Profiler.HeartBeat();` function needs to be called exactly once per frame. 

E.g.
```csharp
// Called once per frame.
protected override void Update(GameTime gameTime)
{
	TracyWrapper.Profiler.HeartBeat();

	/* Game logic goes here */
}
```

### 2. Push and pop zones.

Push a zone to begin profiling a section of code. Pop the zone when the section is over. We can provide a name and a color so we can then identify this block when viewing in the profiler.

E.g.

```csharp
public void PollInputs(TimeSpan timeStamp)
{
	TracyWrapper.Profiler.PushProfileZone("Inputs", System.Drawing.Color.AliceBlue);

	/* Input polling logic. */

	TracyWrapper.Profiler.PopProfileZone();
}
```

Make sure you don't forget to pop the profile zone of the profiler will crash.

### 3. Profile a block using the ProfileScope class

Push a zone to begin profiling a section of code. Pop the zone when the section is over. This can be more convinient but is less accurate, due to the overhead of allocating `TracyWrapper.ProfileScope`.

E.g.

```csharp
public void PollInputs(TimeSpan timeStamp)
{
	using (new TracyWrapper.ProfileScope("Inputs", System.Drawing.Color.AliceBlue))
	{
		/* Input polling logic. This part is measured.*/
	}

    /* This code is not measured. */
}
```

## Profiling accuracy

The Tracy profiler running natively on C++ is extremely accurate. Propertedly it is nanosecond accurate.

Using Tracy through this C# wrapper is much less accurate, due to the fact we must allocate strings to begin a profiling block, and then interface with the C++ code through bindings.

From my measurements, profiling via "PushProfileZone" and "PopProfileZone" directly is more accurate than using the `TracyWrapper.ProfileScope` class. From my crude measurements:

**PushProfileZone/PopProfileZone** : ~150ns accuracy

**TracyWrapper.ProfileScope** : ~310ns accuracy

Any operation that takes less than 1μs = 1000 ns should not be profiled.