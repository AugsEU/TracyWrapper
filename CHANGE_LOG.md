# Tracy Profiler Wrapper for C#

## Change log

### 0.20.0

 * Initial stable release

### 0.20.1

 * Fix memory leak with allocating strings. We store them in a dictionary and only allocate them the first time the scope is created.
 * Use "on demand" model, meaning we don't store profiling info locally until the profiler is connected.
 * Get rid of attributes in TracyWrapperStubs so we don't accidentally leak function names into release builds.

### 0.20.2

 * Fix usage of allocations of CStrings to avoid a crash in the tracy profiler.