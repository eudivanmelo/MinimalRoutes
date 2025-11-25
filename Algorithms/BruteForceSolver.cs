using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MinimalRoutes.Algorithms;

public class BruteForceSolver(int?[,] distanceMatrix)
{
    private readonly int?[,] _distanceMatrix = distanceMatrix;
    private readonly int _numNodes = distanceMatrix.GetLength(0);
    private readonly object _lock = new();
    private List<int> _currentPath = [];
    public List<int> CurrentPath 
    { 
        get 
        { 
            lock (_lock) 
            { 
                return [.. _currentPath]; 
            } 
        } 
    }
    
    public long PathsExplored { get; private set; }
    public long PartialPathsExplored { get; private set; }
    public int BestDistanceSoFar { get; private set; } = int.MaxValue;
    public bool IsRunning { get; private set; }
    
    private List<string> _logs = [];
    public List<string> Logs
    {
        get
        {
            lock (_lock)
            {
                return [.. _logs];
            }
        }
    }

    public SolverResult Solve(int startNode, int endNode, Action<List<int>> onPathUpdate = null, bool enableLog = false)
    {
        var result = new SolverResult();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            if (startNode < 0 || startNode >= _numNodes || endNode < 0 || endNode >= _numNodes)
            {
                result.Success = false;
                result.Message = "Nós inicial ou final inválidos";
                return result;
            }
            
            if (startNode == endNode)
            {
                result.Success = true;
                result.BestPath = [startNode];
                result.BestDistance = 0;
                result.Message = "Nó inicial é igual ao final";
                return result;
            }

            IsRunning = true;
            int bestDistance = int.MaxValue;
            List<int> bestPath = [];
            PathsExplored = 0;
            PartialPathsExplored = 0;
            BestDistanceSoFar = int.MaxValue;
            
            lock (_lock)
            {
                _logs.Clear();
            }
            
            if (enableLog)
            {
                lock (_lock)
                {
                    _logs.Add($"[INICIO] Algoritmo Brute Force - Menor Caminho");
                    _logs.Add($"[INFO] Buscando todos os caminhos de {startNode} ate {endNode}");
                }
            }
            
            List<int> currentPath = [startNode];
            HashSet<int> visited = [startNode];

            void ExplorePaths(int currentNode, int currentDistance)
            {
                PartialPathsExplored++;
                
                lock (_lock)
                {
                    _currentPath = [.. currentPath];
                }
                
                if (enableLog)
                {
                    lock (_lock)
                    {
                        _logs.Add($"[EXPLORANDO] Caminho: [{string.Join(", ", currentPath)}] | Distancia acumulada: {currentDistance}");
                    }
                }
                
                onPathUpdate?.Invoke([.. currentPath]);

                if (currentNode == endNode)
                {
                    PathsExplored++;
                    
                    if (enableLog)
                    {
                        lock (_lock)
                        {
                            _logs.Add($"[CAMINHO] Caminho completo #{PathsExplored}: [{string.Join(", ", currentPath)}] | Distancia: {currentDistance}");
                        }
                    }
                    
                    if (currentDistance < bestDistance)
                    {
                        bestDistance = currentDistance;
                        bestPath = [.. currentPath];
                        BestDistanceSoFar = bestDistance;
                        
                        if (enableLog)
                        {
                            lock (_lock)
                            {
                                _logs.Add($"[MELHOR] Novo melhor caminho encontrado: [{string.Join(", ", bestPath)}] | Distancia: {bestDistance}");
                            }
                        }
                        
                        onPathUpdate?.Invoke([.. bestPath]);
                    }
                    return;
                }

                for (int nextNode = 0; nextNode < _numNodes; nextNode++)
                {
                    if (visited.Contains(nextNode))
                        continue;

                    int? distance = _distanceMatrix[currentNode, nextNode];
                    if (!distance.HasValue || distance.Value == 0)
                        continue;

                    int newDistance = currentDistance + distance.Value;

                    currentPath.Add(nextNode);
                    visited.Add(nextNode);

                    ExplorePaths(nextNode, newDistance);

                    currentPath.RemoveAt(currentPath.Count - 1);
                    visited.Remove(nextNode);
                }
            }

            ExplorePaths(startNode, 0);

            stopwatch.Stop();

            if (enableLog)
            {
                lock (_lock)
                {
                    _logs.Add($"[FIM] Algoritmo finalizado em {stopwatch.Elapsed.TotalMilliseconds:F2}ms");
                    _logs.Add($"[STATS] Caminhos parciais explorados: {PartialPathsExplored:N0}");
                    _logs.Add($"[STATS] Caminhos completos encontrados: {PathsExplored}");
                }
            }

            result.BestPath = bestPath;
            result.BestDistance = bestDistance;
            result.PathsChecked = PathsExplored;
            result.PartialPathsExplored = PartialPathsExplored;
            result.ElapsedTime = stopwatch.Elapsed;
            result.Success = bestDistance < int.MaxValue;
            result.Message = result.Success 
                ? $"Melhor caminho encontrado com distância {bestDistance}" 
                : "Nenhum caminho válido encontrado";

            IsRunning = false;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            result.Success = false;
            result.Message = $"Erro: {ex.Message}";
            result.ElapsedTime = stopwatch.Elapsed;
            IsRunning = false;
        }

        return result;
    }
}
