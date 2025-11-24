using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace MinimalRoutes.Algorithms;

public class DijkstraSolver(int?[,] distanceMatrix)
{
    private readonly int?[,] _distanceMatrix = distanceMatrix;
    private readonly int _numNodes = distanceMatrix.GetLength(0);
    private readonly Lock _lock = new();

    private class SearchState : IComparable<SearchState>
    {
        public int CurrentNode { get; set; }
        public List<int> Path { get; set; }
        public HashSet<int> Visited { get; set; }
        public int CurrentDistance { get; set; }

        public int CompareTo(SearchState other)
        {
            return CurrentDistance.CompareTo(other.CurrentDistance);
        }
    }

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
    
    private readonly List<string> _logs = [];
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
            if (startNode < 0 || startNode >= _numNodes)
            {
                result.Success = false;
                result.Message = "Nó inicial inválido";
                return result;
            }

            IsRunning = true;
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
                    _logs.Add($"[INÍCIO] Algoritmo Priority Queue (Dijkstra adaptado para TSP)");
                    _logs.Add($"[INFO] Iniciando busca do nó {startNode}");
                }
            }

            int bestDistance = int.MaxValue;
            List<int> bestPath = null;

            PriorityQueue<SearchState, int> priorityQueue = new();
            
            var initialState = new SearchState
            {
                CurrentNode = startNode,
                Path = [startNode],
                Visited = [startNode],
                CurrentDistance = 0
            };
            
            priorityQueue.Enqueue(initialState, initialState.CurrentDistance);

            Dictionary<string, int> bestKnown = [];

            while (priorityQueue.Count > 0)
            {
                var state = priorityQueue.Dequeue();
                PartialPathsExplored++;
                
                if (enableLog)
                {
                    lock (_lock)
                    {
                        _logs.Add($"[EXPLORANDO] Caminho: [{string.Join(", ", state.Path)}] | Distância: {state.CurrentDistance}");
                    }
                }
                
                lock (_lock)
                {
                    _currentPath = [.. state.Path];
                }
                onPathUpdate?.Invoke([.. state.Path]);

                if (state.CurrentDistance >= bestDistance)
                {
                    if (enableLog)
                    {
                        lock (_lock)
                        {
                            _logs.Add($"[PODADO] Caminho descartado (distância {state.CurrentDistance} >= melhor {bestDistance})");
                        }
                    }
                    continue;
                }

                if (state.Visited.Count == _numNodes)
                {
                    int? returnDistance = _distanceMatrix[state.CurrentNode, startNode];
                    if (returnDistance.HasValue && returnDistance.Value > 0)
                    {
                        int totalDistance = state.CurrentDistance + returnDistance.Value;
                        
                        if (totalDistance < bestDistance)
                        {
                            PathsExplored++;
                            bestDistance = totalDistance;
                            bestPath = [.. state.Path];
                            bestPath.Add(startNode);
                            BestDistanceSoFar = bestDistance;
                            
                            if (enableLog)
                            {
                                lock (_lock)
                                {
                                    _logs.Add($"[SOLUCAO] Ciclo completo encontrado: [{string.Join(", ", bestPath)}] | Distancia: {bestDistance}");
                                }
                            }
                            
                            lock (_lock)
                            {
                                _currentPath = [.. bestPath];
                            }
                            
                            onPathUpdate?.Invoke([.. bestPath]);
                        }
                    }
                    continue;
                }

                string stateKey = $"{state.CurrentNode}:{string.Join(",", state.Visited.Order())}";
                
                if (bestKnown.TryGetValue(stateKey, out int knownDistance))
                {
                    if (state.CurrentDistance >= knownDistance)
                    {
                        if (enableLog)
                        {
                            lock (_lock)
                            {
                                _logs.Add($"[MEMO] Estado já visitado com menor distância ({knownDistance} vs {state.CurrentDistance})");
                            }
                        }
                        continue;
                    }
                }
                bestKnown[stateKey] = state.CurrentDistance;

                for (int nextNode = 0; nextNode < _numNodes; nextNode++)
                {
                    if (state.Visited.Contains(nextNode))
                        continue;

                    int? distance = _distanceMatrix[state.CurrentNode, nextNode];
                    if (!distance.HasValue || distance.Value <= 0)
                        continue;

                    int newDistance = state.CurrentDistance + distance.Value;
                    
                    if (newDistance >= bestDistance)
                        continue;

                    var newVisited = new HashSet<int>(state.Visited) { nextNode };
                    
                    var newState = new SearchState
                    {
                        CurrentNode = nextNode,
                        Path = [.. state.Path, nextNode],
                        Visited = newVisited,
                        CurrentDistance = newDistance
                    };

                    priorityQueue.Enqueue(newState, newState.CurrentDistance);
                }
            }

            stopwatch.Stop();

            if (enableLog)
            {
                lock (_lock)
                {
                    _logs.Add($"[FIM] Algoritmo finalizado em {stopwatch.Elapsed.TotalMilliseconds:F2}ms");
                    _logs.Add($"[STATS] Caminhos parciais explorados: {PartialPathsExplored:N0}");
                    _logs.Add($"[STATS] Ciclos completos encontrados: {PathsExplored}");
                    _logs.Add($"[STATS] Estados únicos visitados: {bestKnown.Count}");
                }
            }

            if (bestPath != null && bestDistance < int.MaxValue)
            {
                result.BestPath = bestPath;
                result.BestDistance = bestDistance;
                result.PathsChecked = PathsExplored;
                result.PartialPathsExplored = PartialPathsExplored;
                result.ElapsedTime = stopwatch.Elapsed;
                result.Success = true;
                result.Message = $"Caminho ótimo encontrado com distância {bestDistance}";
                BestDistanceSoFar = bestDistance;
            }
            else
            {
                result.Success = false;
                result.Message = "Não foi possível encontrar um caminho válido";
                result.ElapsedTime = stopwatch.Elapsed;
                result.PartialPathsExplored = PartialPathsExplored;
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
}
