using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MinimalRoutes.Algorithms;

public class DijkstraSolver
{
    private readonly int?[,] _distanceMatrix;
    private readonly int _numNodes;
    private readonly object _lock = new();

    // Classe para representar um estado na fila de prioridade
    private class SearchState : IComparable<SearchState>
    {
        public int CurrentNode { get; set; }
        public List<int> Path { get; set; }
        public HashSet<int> Visited { get; set; }
        public int CurrentDistance { get; set; }
        public int EstimatedTotal { get; set; } // Distância atual + estimativa do resto

        public int CompareTo(SearchState other)
        {
            // Menor distância estimada tem prioridade
            return EstimatedTotal.CompareTo(other.EstimatedTotal);
        }
    }

    public DijkstraSolver(int?[,] distanceMatrix)
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
                return new List<int>(_currentPath); 
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

    // Calcula estimativa de distância mínima restante (heurística)
    private int CalculateLowerBound(HashSet<int> visited, int currentNode)
    {
        if (visited.Count == _numNodes)
            return 0;

        // Encontra as menores arestas não visitadas
        int minEdge = int.MaxValue;
        
        for (int i = 0; i < _numNodes; i++)
        {
            if (visited.Contains(i))
                continue;
                
            for (int j = 0; j < _numNodes; j++)
            {
                if (i == j || visited.Contains(j))
                    continue;
                    
                int? distance = _distanceMatrix[i, j];
                if (distance.HasValue && distance.Value > 0 && distance.Value < minEdge)
                {
                    minEdge = distance.Value;
                }
            }
        }
        
        // Estimativa conservadora: menor aresta * nós restantes
        int nodesRemaining = _numNodes - visited.Count;
        return minEdge != int.MaxValue ? minEdge * nodesRemaining : 0;
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

            // Fila de prioridade - explora caminhos mais promissores primeiro
            PriorityQueue<SearchState, int> priorityQueue = new();
            
            // Estado inicial
            var initialState = new SearchState
            {
                CurrentNode = startNode,
                Path = new List<int> { startNode },
                Visited = new HashSet<int> { startNode },
                CurrentDistance = 0,
                EstimatedTotal = CalculateLowerBound(new HashSet<int> { startNode }, startNode)
            };
            
            priorityQueue.Enqueue(initialState, initialState.EstimatedTotal);

            // Armazena a melhor distância para cada estado (nó atual + conjunto visitados)
            Dictionary<string, int> bestKnown = new();

            while (priorityQueue.Count > 0)
            {
                var state = priorityQueue.Dequeue();
                PartialPathsExplored++;
                
                if (enableLog)
                {
                    lock (_lock)
                    {
                        _logs.Add($"[EXPLORANDO] Caminho: [{string.Join(", ", state.Path)}] | Distância: {state.CurrentDistance} | Estimativa Total: {state.EstimatedTotal}");
                    }
                }
                
                // Atualiza caminho atual para visualização
                lock (_lock)
                {
                    _currentPath = new List<int>(state.Path);
                }
                onPathUpdate?.Invoke(new List<int>(state.Path));

                // Poda: se a distância atual já é >= melhor encontrada, ignora
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

                // Verifica se visitou todos os nós
                if (state.Visited.Count == _numNodes)
                {
                    // Tenta retornar ao início
                    int? returnDistance = _distanceMatrix[state.CurrentNode, startNode];
                    if (returnDistance.HasValue && returnDistance.Value > 0)
                    {
                        int totalDistance = state.CurrentDistance + returnDistance.Value;
                        
                        if (totalDistance < bestDistance)
                        {
                            PathsExplored++;
                            bestDistance = totalDistance;
                            bestPath = new List<int>(state.Path);
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
                                _currentPath = new List<int>(bestPath);
                            }
                            
                            onPathUpdate?.Invoke(new List<int>(bestPath));
                        }
                    }
                    continue;
                }

                // Gera chave para memoização (estado = nó atual + conjunto visitados)
                string stateKey = $"{state.CurrentNode}:{string.Join(",", state.Visited.Order())}";
                
                // Poda por memoização: se já encontramos este estado com distância menor, ignora
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

                // Explora todos os vizinhos não visitados
                for (int nextNode = 0; nextNode < _numNodes; nextNode++)
                {
                    if (state.Visited.Contains(nextNode))
                        continue;

                    int? distance = _distanceMatrix[state.CurrentNode, nextNode];
                    if (!distance.HasValue || distance.Value <= 0)
                        continue;

                    int newDistance = state.CurrentDistance + distance.Value;
                    
                    // Poda: não adiciona à fila se já é pior que a melhor solução
                    if (newDistance >= bestDistance)
                        continue;

                    // Cria novo estado
                    var newVisited = new HashSet<int>(state.Visited) { nextNode };
                    int lowerBound = CalculateLowerBound(newVisited, nextNode);
                    
                    var newState = new SearchState
                    {
                        CurrentNode = nextNode,
                        Path = new List<int>(state.Path) { nextNode },
                        Visited = newVisited,
                        CurrentDistance = newDistance,
                        EstimatedTotal = newDistance + lowerBound
                    };

                    // Poda com estimativa: se estimativa total já é pior, nem adiciona
                    if (newState.EstimatedTotal >= bestDistance)
                        continue;

                    priorityQueue.Enqueue(newState, newState.EstimatedTotal);
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
                result.Message = "Não foi possível encontrar um ciclo hamiltoniano";
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
