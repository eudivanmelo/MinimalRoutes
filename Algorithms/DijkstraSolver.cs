using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace MinimalRoutes.Algorithms;

public class DijkstraSolver(int?[,] distanceMatrix)
{
    private readonly int?[,] _distanceMatrix = distanceMatrix;
    private readonly int _numNodes = distanceMatrix.GetLength(0);
    private readonly Lock _lock = new();
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
    
    public long NodesExplored { get; private set; }
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
            NodesExplored = 0;
            BestDistanceSoFar = int.MaxValue;
            
            lock (_lock)
            {
                _logs.Clear();
            }
            
            if (enableLog)
            {
                lock (_lock)
                {
                    _logs.Add($"[INICIO] Algoritmo de Dijkstra - Menor Caminho");
                    _logs.Add($"[INFO] Buscando caminho de {startNode} ate {endNode}");
                }
            }

            PriorityQueue<(int node, int distance), int> priorityQueue = new();
            Dictionary<int, int> distances = [];
            Dictionary<int, int> previous = [];
            HashSet<int> visited = [];

            for (int i = 0; i < _numNodes; i++)
            {
                distances[i] = int.MaxValue;
            }
            distances[startNode] = 0;

            priorityQueue.Enqueue((startNode, 0), 0);

            while (priorityQueue.Count > 0)
            {
                var (currentNode, currentDistance) = priorityQueue.Dequeue();
                
                if (visited.Contains(currentNode))
                    continue;

                visited.Add(currentNode);
                NodesExplored++;

                List<int> currentPath = ReconstructPath(previous, startNode, currentNode);
                
                lock (_lock)
                {
                    _currentPath = [.. currentPath];
                }
                onPathUpdate?.Invoke([.. currentPath]);

                if (enableLog)
                {
                    lock (_lock)
                    {
                        _logs.Add($"[EXPLORANDO] No: {currentNode} | Distancia: {currentDistance} | Caminho: [{string.Join(", ", currentPath)}]");
                    }
                }

                if (currentNode == endNode)
                {
                    BestDistanceSoFar = currentDistance;
                    
                    if (enableLog)
                    {
                        lock (_lock)
                        {
                            _logs.Add($"[SOLUCAO] Caminho encontrado: [{string.Join(", ", currentPath)}] | Distancia: {currentDistance}");
                        }
                    }
                    break;
                }

                for (int neighbor = 0; neighbor < _numNodes; neighbor++)
                {
                    if (visited.Contains(neighbor))
                        continue;

                    int? edgeWeight = _distanceMatrix[currentNode, neighbor];
                    if (!edgeWeight.HasValue || edgeWeight.Value <= 0)
                        continue;

                    int newDistance = currentDistance + edgeWeight.Value;

                    if (newDistance < distances[neighbor])
                    {
                        distances[neighbor] = newDistance;
                        previous[neighbor] = currentNode;
                        priorityQueue.Enqueue((neighbor, newDistance), newDistance);
                    }
                }
            }

            stopwatch.Stop();

            if (enableLog)
            {
                lock (_lock)
                {
                    _logs.Add($"[FIM] Algoritmo finalizado em {stopwatch.Elapsed.TotalMilliseconds:F2}ms");
                    _logs.Add($"[STATS] Nos explorados: {NodesExplored}");
                }
            }

            if (distances[endNode] < int.MaxValue)
            {
                List<int> finalPath = ReconstructPath(previous, startNode, endNode);
                
                result.BestPath = finalPath;
                result.BestDistance = distances[endNode];
                result.PathsChecked = NodesExplored;
                result.PartialPathsExplored = NodesExplored;
                result.ElapsedTime = stopwatch.Elapsed;
                result.Success = true;
                result.Message = $"Caminho encontrado com distancia {distances[endNode]}";
                BestDistanceSoFar = distances[endNode];
            }
            else
            {
                result.Success = false;
                result.Message = "Nao existe caminho entre os nos";
                result.ElapsedTime = stopwatch.Elapsed;
                result.PartialPathsExplored = NodesExplored;
            }

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

    private static List<int> ReconstructPath(Dictionary<int, int> previous, int start, int end)
    {
        List<int> path = [];
        int current = end;

        while (current != start)
        {
            path.Add(current);
            if (!previous.TryGetValue(current, out int value))
                return [start];
                
            current = value;
        }
        
        path.Add(start);
        path.Reverse();
        return path;
    }
}
