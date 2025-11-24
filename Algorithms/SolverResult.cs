using System;
using System.Collections.Generic;

namespace MinimalRoutes.Algorithms;

public class SolverResult
{
    public List<int> BestPath { get; set; } = new();
    public int BestDistance { get; set; }
    public long PathsChecked { get; set; }
    public long PartialPathsExplored { get; set; }
    public TimeSpan ElapsedTime { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
