Imports Xunit

' Tests share the static Screen.GetScreen(), so they must not run in parallel.
<Assembly: CollectionBehavior(DisableTestParallelization:=True)>
