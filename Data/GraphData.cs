namespace MinimalRoutes.Data;

internal enum GraphType
{
    Simple,
    Complex,
    VeryComplex
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

    private static readonly int?[,] veryComplexPath = new int?[,]
    {
        { null, 15, 20, 25, null, null, null, null, 10, null, 30, null, 40, null, 50, null, null, null, null, null, null, null, null, null, null }, // 0
        { 15, null, null, 10, 18, null, 22, null, null, null, null, 35, null, null, 60, null, null, null, null, null, null, null, null, null, null }, // 1
        { 20, null, null, null, 12, 16, 28, null, null, null, null, null, null, 45, null, null, null, null, null, null, null, null, null, null, null }, // 2
        { 25, 10, null, null, null, 20, null, 30, null, null, 55, null, 15, null, null, null, 33, null, null, null, null, null, null, null, null }, // 3
        { null, 18, 12, null, null, null, null, 25, null, 38, null, null, null, null, 42, null, null, null, 21, null, null, null, null, null, null }, // 4
        { null, null, 16, 20, null, null, 14, null, 32, null, null, null, null, null, null, 27, null, null, 19, null, null, null, null, null, null }, // 5
        { null, 22, 28, null, null, 14, null, 18, null, null, 35, null, null, null, 48, null, null, null, null, 29, null, null, null, null, null }, // 6
        { null, null, null, 30, 25, null, 18, null, null, 21, null, 33, null, null, null, null, 19, null, null, null, null, null, 40, null, null }, // 7
        { 10, null, null, null, null, 32, null, null, null, 13, null, null, 29, null, null, 23, null, null, null, null, 45, null, null, null, null }, // 8
        { null, null, null, null, 38, null, null, 21, 13, null, null, null, null, 31, null, null, null, 24, null, null, null, null, 36, null, null }, // 9
        { 30, null, null, 55, null, null, 35, null, null, null, null, 15, null, null, null, null, null, null, null, 31, null, null, null, null, null }, // 10
        { null, 35, null, null, null, null, null, 33, null, null, 15, null, null, null, null, null, 44, 28, null, null, null, null, null, null, 50 }, // 11
        { 40, null, null, 15, null, null, null, null, 29, null, null, null, null, 17, null, 21, null, null, null, null, null, null, null, 30, null }, // 12
        { null, null, 45, null, null, null, null, null, null, 31, null, null, 17, null, 11, null, null, null, null, null, 55, null, null, null, null }, // 13
        { 50, 60, null, null, 42, null, 48, null, null, null, null, null, null, 11, null, null, 26, null, null, null, 40, null, null, null, null }, // 14
        { null, null, null, null, null, 27, null, null, 23, null, null, null, 21, null, null, null, 12, null, null, null, null, 34, null, null, 38 }, // 15
        { null, null, null, 33, null, null, null, 19, null, null, null, 44, null, null, 26, 12, null, 17, null, null, null, null, 20, null, null }, // 16
        { null, null, null, null, null, null, null, null, null, 24, null, 28, null, null, null, null, 17, null, 36, null, null, null, 18, null, 35 }, // 17
        { null, null, null, null, 21, 19, null, null, null, null, null, null, null, null, null, null, null, 36, null, 16, 25, null, null, null, null }, // 18
        { null, null, null, null, null, null, 29, null, null, null, 31, null, null, null, null, null, null, null, 16, null, null, 24, null, null, 42 }, // 19
        { null, null, null, null, null, null, null, null, 45, null, null, null, null, 55, 40, null, null, null, 25, null, null, 20, null, null, 28 }, // 20
        { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, 34, null, null, null, 24, 20, null, 15, 23, null }, // 21
        { null, null, null, null, null, null, null, 40, null, 36, null, null, null, null, null, null, 20, 18, null, null, null, 15, null, 10, null }, // 22
        { null, null, null, null, null, null, null, null, null, null, null, null, 30, null, null, null, null, null, null, null, null, 23, 10, null, 22 }, // 23
        { null, null, null, null, null, null, null, null, null, null, null, 50, null, null, null, 38, null, 35, null, 42, 28, null, null, 22, null }  // 24
    };

    public static int?[,] GetGraphData(GraphType graphType)
    {
        return graphType switch
        {
            GraphType.Simple => simplePath,
            GraphType.Complex => complexPath,
            GraphType.VeryComplex => veryComplexPath,
            _ => null
        };
    }
}