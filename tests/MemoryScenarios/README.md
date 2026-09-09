# Memory regression scenarios

Run on Windows from the repository root:

```powershell
dotnet run --project tests/MemoryScenarios -c Release -- navdata
```

Checks bounded map tiles during travel and zooming, disposal of removed markers,
shared versus owned images, asynchronous loading after disposal, bounded multiline
and read-only chat logs (including calls through `RichTextBox`), invalid navigation
records, two-way adjacency, duplicate filtering and bounds of all supplied regions.

For comparison with a previous revision, export that revision's `NavDataReader.cs`
to a temporary file, rename its class to `BaselineNavDataReader`, and pass its
absolute path with `-p:BaselineReader=<path>` before `-- navdata`. The scenarios
then compare every point and adjacency entry, including order, and require lower
allocation totals than the baseline.

The allocation totals measure cumulative managed allocations in the .NET 8 test
process. They are not peak RAM, native GDI memory, or the working set of the
.NET Framework 4.8 application during a live game session.
