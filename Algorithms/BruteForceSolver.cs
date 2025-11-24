using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MinimalRoutes.Algorithms;

public class BruteForceSolver
{
    private readonly int?[,] _distanceMatrix;
    private readonly int _numNodes;
    private readonly object _lock = new();

    public BruteForceSolver(int?[,] distanceMatrix)
    {
        _distanceMatrix = distanceMatrix;
        _numNodes = distanceMatrix.GetLength(0);
    }

    private List<int> _currentPath = new();
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
    
    private List<string> _logs = new();
    public List<string> Logs
    {
        get
        {
            lock (_lock)
            {
                return new List<string>(_logs);
            }
        }
    }

    public SolverResult Solve(int startNode, int endNode, Action<List<int>> onPathUpdate = null, bool enableLog = false)
    {
        var result = new SolverResult();
        var stopwatch = Stopwatch.StartNew();

        try
        {
            if (startNode < 0 || startNode >= _numNodes)
            {
                result.Success = false;
                result.Message = "Nó inicial inválido";
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
                    _logs.Add($"[INÍCIO] Algoritmo Brute Force (força bruta)");
                    _logs.Add($"[INFO] Iniciando busca exaustiva do nó {startNode}");
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
                        _logs.Add($"[EXPLORANDO] Caminho: [{string.Join(", ", currentPath)}] | Distância acumulada: {currentDistance}");
                    }
                }
                
                // Notifica o caminho atual sendo explorado
                onPathUpdate?.Invoke(new List<int>(currentPath));

                if (visited.Count == _numNodes)
                    {
                        int? returnDistance = _distanceMatrix[currentNode, startNode];
                        if (returnDistance.HasValue && returnDistance.Value > 0)
                        {
                            PathsExplored++;
                            int totalDistance = currentDistance + returnDistance.Value;
                            
                            if (enableLog)
                            {
                                lock (_lock)
                                {
                                    List<int> completePath = [.. currentPath];
                                    completePath.Add(startNode);
                                    _logs.Add($"[CICLO] Ciclo #{PathsExplored}: [{string.Join(", ", completePath)}] | Distância: {totalDistance}");
                                }
                            }
                            
                            if (totalDistance < bestDistance)
                            {
                                bestDistance = totalDistance;
                                bestPath = [.. currentPath];
                                bestPath.Add(startNode); // Adiciona o retorno ao início
                                BestDistanceSoFar = bestDistance;
                                
                                if (enableLog)
                                {
                                    lock (_lock)
                                    {
                                        _logs.Add($"[MELHOR] Novo melhor caminho encontrado: [{string.Join(", ", bestPath)}] | Distancia: {bestDistance}");
                                    }
                                }
                                
                                // Notifica quando encontrar um caminho melhor
                                onPathUpdate?.Invoke(new List<int>(bestPath));
                            }
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
                    _logs.Add($"[STATS] Ciclos completos testados: {PathsExplored}");
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
