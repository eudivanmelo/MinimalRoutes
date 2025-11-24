namespace MinimalRoutes.Data;

internal enum GraphType
{
    Simple,
    Complex
}

internal static class GraphData
{
    private static readonly int?[,] simplePath = new int?[,]
        {
            { null, 2,null, 3, 6 },
            { 2,null, 4, 3, null },
            {null, 4,null, 7, 3 },
            { 3, 3, 7,null, 3 },
            { 6,null, 3, 3, null }
        };

    private static readonly int?[,] complexPath = new int?[,]
        {
            {null, 20,null,null,null,null,null, 29,null,null,null, 29, 37,null,null,null,null, null},
            {20,null, 25,null,null,null,null, 28,null,null,null, 39,null,null,null,null,null, null},
            {null, 25,null, 25,null,null,null, 30,null,null,null,null, 54,null,null,null,null, null},
            {null,null, 25,null, 39, 32, 42,null, 23, 33,null,null,null, 56,null,null,null, null},
            {null,null,null, 39,null, 12, 26,null,null, 19,null,null,null,null,null,null,null, null},
            {null,null,null, 32, 12,null, 17,null,null, 35, 30,null,null,null,null,null,null, null},
            {null,null,null, 42, 26, 17,null,null,null,null, 38,null,null,null,null,null,null, null},
            {29, 28, 30,null,null,null,null,null,null,null,null, 25, 22,null,null,null,null, null},
            {null,null,null, 23,null,null,null,null,null, 26,null,null, 34,null,null,null,null, null},
            {null,null,null, 33, 19, 35,null,null, 26,null, 24,null,null, 30, 19,null,null, null},
            {null,null,null,null,null, 30, 38,null,null, 24,null,null,null,null, 26,null,null, 36},
            {29, 39,null,null,null,null,null, 25,null,null,null,null, 27,null,null, 43,null, null},
            {37,null, 54,null,null,null,null, 22, 34,null,null, 27,null, 24,null, 19,null, null},
            {null,null,null, 56,null,null,null,null,null, 30,null,null, 24,null, 20, 19, 17, null},
            {null,null,null,null,null,null,null,null,null, 19, 26,null,null, 20,null,null, 18, 21},
            {null,null,null,null,null,null,null,null,null, 43,null, 43, 19, 19,null,null, 26, null},
            {null,null,null,null,null,null,null,null,null,null,null,null,null, 17, 18, 26,null, 15},
            {null,null,null,null,null,null,null,null,null,null, 36,null,null,null, 21,null, 15, 0}
        };

    public static int?[,] GetGraphData(GraphType graphType)
    {
        return graphType switch
        {
            GraphType.Simple => simplePath,
            GraphType.Complex => complexPath,
            _ => null
        };
    }
}