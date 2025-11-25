using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MinimalRoutes.Components;
using MinimalRoutes.Data;
using MinimalRoutes.Algorithms;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace MinimalRoutes;

public class MainGame : Game
{
    #region Fields
    
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private SpriteFont _font;
    private Texture2D _pixel;
    
    private Button _changeGraphButton;
    private Button _bruteForceButton;
    private Button _dijkstraButton;
    private Button _aStarButton;
    
    private MouseState _currentMouseState;
    private MouseState _previousMouseState;
    
    private GraphVisualizer _graphVisualizer;
    private GraphType _currentGraphType = GraphType.Simple;
    
    private BruteForceSolver _bruteForceSolver;
    private DijkstraSolver _dijkstraSolver;
    private SolverResult _lastResult;
    private bool _isSearching = false;
    private string _currentAlgorithm = "";
    private readonly object _lockObject = new();
    private readonly Queue<List<int>> _pathUpdateQueue = new();
    private readonly object _queueLock = new();
    private readonly bool _debugMode;
    
    private const int ButtonWidth = 200;
    private const int ButtonHeight = 50;
    private const int ButtonSpacing = 20;
    private const int SidebarWidth = 240;
    private const int GraphAreaMargin = 20;
    
    #endregion

    #region Initialization
    
    public MainGame(bool debugMode = false)
    {
        _debugMode = debugMode;
        _graphics = new GraphicsDeviceManager(this)
        {
            PreferredBackBufferWidth = 1280,
            PreferredBackBufferHeight = 720
        };
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize() => base.Initialize();

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _font = Content.Load<SpriteFont>("DefaultFont");
        
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);
        
        CreateButtons();
        InitializeGraph();
    }
    
    private void InitializeGraph()
    {
        int graphWidth = _graphics.PreferredBackBufferWidth - SidebarWidth - GraphAreaMargin * 2;
        int graphHeight = _graphics.PreferredBackBufferHeight - GraphAreaMargin * 2;
        
        _graphVisualizer = new GraphVisualizer(GraphicsDevice, _font, graphWidth, graphHeight);
        _graphVisualizer?.SetStartNode(0);
        _graphVisualizer?.SetEndNode(GraphData.GetGraphData(_currentGraphType).GetLength(0) - 1);
        _graphVisualizer.LoadGraph(GraphData.GetGraphData(_currentGraphType));
    }
    
    #endregion
    
    #region UI Setup
    
    private void CreateButtons()
    {
        int x = ButtonSpacing;
        int y = ButtonSpacing;
        int algorithmY = y + ButtonHeight + ButtonSpacing * 2;

        _changeGraphButton = CreateButton(x, y, "Grafo Simples", new(50, 100, 150), new(70, 120, 170));
        _changeGraphButton.Click += ChangeGraphButton_Click;

        _bruteForceButton = CreateButton(x, algorithmY, "Força Bruta", new(180, 50, 50), new(200, 70, 70));
        _bruteForceButton.Click += BruteForceButton_Click;

        _dijkstraButton = CreateButton(x, algorithmY + ButtonHeight + ButtonSpacing, "Dijkstra", new(50, 180, 50), new(70, 200, 70));
        _dijkstraButton.Click += DijkstraButton_Click;

        _aStarButton = CreateButton(x, algorithmY + (ButtonHeight + ButtonSpacing) * 2, "A* (Desabilitado)", new(180, 180, 50), new(200, 200, 70));
        _aStarButton.Click += AStarButton_Click;
        _aStarButton.Disable();
    }
    
    private Button CreateButton(int x, int y, string text, Color bgColor, Color hoverColor)
    {
        return new Button(x, y, ButtonWidth, ButtonHeight, text, _pixel, _font)
        {
            BackgroundColor = bgColor,
            HoverColor = hoverColor
        };
    }
    
    #endregion
    
    #region Event Handlers
    
    private void ChangeGraphButton_Click(object sender, System.EventArgs e)
    {
        _currentGraphType = _currentGraphType switch
        {
            GraphType.Simple => GraphType.Complex,
            GraphType.Complex => GraphType.VeryComplex,
            GraphType.VeryComplex => GraphType.Simple,
            _ => throw new NotImplementedException()
        };

        _graphVisualizer?.ClearHighlightedPath();
        _graphVisualizer?.SetStartNode(0);
        _graphVisualizer?.SetEndNode(GraphData.GetGraphData(_currentGraphType).GetLength(0) - 1);
        _graphVisualizer?.LoadGraph(GraphData.GetGraphData(_currentGraphType));
        
        _changeGraphButton.Text = _currentGraphType switch
        {
            GraphType.Simple => "Grafo Simples",
            GraphType.Complex => "Grafo Complexo",
            GraphType.VeryComplex => "Muito Complexo",
            _ => throw new NotImplementedException()
        };

    }
    
    private void BruteForceButton_Click(object sender, System.EventArgs e)
    {
        try
        {
            if (_isSearching) return;
            
            int nodeCount = _graphVisualizer?.NodeCount ?? 0;
            if (nodeCount < 2) return;
            
            _isSearching = true;
            _currentAlgorithm = "BruteForce";
            _lastResult = null;
            _bruteForceButton?.Disable();
            
            int?[,] matrix = GraphData.GetGraphData(_currentGraphType);
            if (matrix == null) 
            {
                _isSearching = false;
                _bruteForceButton?.Enable();
                return;
            }
            
            _bruteForceSolver = new BruteForceSolver(matrix);
            
            Task.Run(() =>
            {
                try
                {
                    // Usa nós selecionados ou padrão (0 e último)
                    int startNode = _graphVisualizer.SelectedStartNode ?? 0;
                    int endNode = _graphVisualizer.SelectedEndNode ?? (nodeCount - 1);
                    
                    var result = _bruteForceSolver.Solve(startNode, endNode, (path) =>
                    {
                        // Enfileira atualização para thread principal
                        lock (_queueLock)
                        {
                            _pathUpdateQueue.Enqueue([.. path]);
                        }
                    }, _debugMode);
                    
                    // Salva logs se debug estiver ativado
                    if (_debugMode)
                    {
                        SaveLogsToFile("bruteforce_debug.txt", _bruteForceSolver.Logs);
                    }
                    
                    lock (_lockObject)
                    {
                        _lastResult = result;
                    }
                    
                    // Limpa fila de atualizações pendentes
                    lock (_queueLock)
                    {
                        _pathUpdateQueue.Clear();
                    }
                    
                    // Atualiza para o melhor caminho final
                    if (result?.Success == true && result.BestPath != null)
                    {
                        _graphVisualizer?.SetHighlightedPath(result.BestPath);
                    }
                }
                finally
                {
                    _isSearching = false;
                    _currentAlgorithm = "";
                    _bruteForceButton?.Enable();
                }
            });
        }
        catch
        {
            _isSearching = false;
            _currentAlgorithm = "";
            _bruteForceButton?.Enable();
        }
    }
    
    private void DijkstraButton_Click(object sender, System.EventArgs e)
    {
        try
        {
            if (_isSearching) return;
            
            int nodeCount = _graphVisualizer?.NodeCount ?? 0;
            if (nodeCount < 2) return;
            
            _isSearching = true;
            _currentAlgorithm = "Dijkstra";
            _lastResult = null;
            _dijkstraButton?.Disable();
            
            int?[,] matrix = GraphData.GetGraphData(_currentGraphType);
            if (matrix == null) 
            {
                _isSearching = false;
                _dijkstraButton?.Enable();
                return;
            }
            
            _dijkstraSolver = new DijkstraSolver(matrix);
            
            Task.Run(() =>
            {
                try
                {
                    // Usa nós selecionados ou padrão (0 e último)
                    int startNode = _graphVisualizer.SelectedStartNode ?? 0;
                    int endNode = _graphVisualizer.SelectedEndNode ?? (nodeCount - 1);
                    
                    var result = _dijkstraSolver.Solve(startNode, endNode, (path) =>
                    {
                        // Enfileira atualização para thread principal
                        lock (_queueLock)
                        {
                            _pathUpdateQueue.Enqueue([.. path]);
                        }
                    }, _debugMode);
                    
                    // Salva logs se debug estiver ativado
                    if (_debugMode)
                    {
                        SaveLogsToFile("dijkstra_debug.txt", _dijkstraSolver.Logs);
                    }
                    
                    lock (_lockObject)
                    {
                        _lastResult = result;
                    }
                    
                    // Limpa fila de atualizações pendentes
                    lock (_queueLock)
                    {
                        _pathUpdateQueue.Clear();
                    }
                    
                    // Atualiza para o melhor caminho final
                    if (result?.Success == true && result.BestPath != null)
                    {
                        _graphVisualizer?.SetHighlightedPath(result.BestPath);
                    }
                }
                finally
                {
                    _isSearching = false;
                    _currentAlgorithm = "";
                    _dijkstraButton?.Enable();
                }
            });
        }
        catch
        {
            _isSearching = false;
            _currentAlgorithm = "";
            _dijkstraButton?.Enable();
        }
    }
    
    private void AStarButton_Click(object sender, System.EventArgs e)
    {
    }
    
    #endregion

    #region Update & Draw
    
    protected override void Update(GameTime gameTime)
    {
        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        
        // Processa atualizações de caminho enfileiradas (thread-safe)
        lock (_queueLock)
        {
            if (_pathUpdateQueue.Count > 0)
            {
                var path = _pathUpdateQueue.Dequeue();
                _graphVisualizer?.SetHighlightedPath(path);
                // Limpa fila para evitar acúmulo (mostra apenas última atualização)
                _pathUpdateQueue.Clear();
            }
        }
        
        _previousMouseState = _currentMouseState;
        _currentMouseState = Mouse.GetState();
        
        // Detectar clique no grafo para selecionar nós
        if (!_isSearching && _currentMouseState.LeftButton == ButtonState.Pressed && 
            _previousMouseState.LeftButton == ButtonState.Released)
        {
            Vector2 clickPos = new Vector2(_currentMouseState.X, _currentMouseState.Y);
            Vector2 graphOffset = new Vector2(SidebarWidth + GraphAreaMargin, GraphAreaMargin);
            
            int? clickedNode = _graphVisualizer?.GetNodeAtPosition(clickPos, graphOffset);
            
            if (clickedNode.HasValue)
            {
                // Se não tem nó inicial selecionado, seleciona como inicial
                if (!_graphVisualizer.SelectedStartNode.HasValue)
                {
                    _graphVisualizer.SetStartNode(clickedNode.Value);
                }
                // Se já tem inicial mas não tem final, seleciona como final
                else if (!_graphVisualizer.SelectedEndNode.HasValue)
                {
                    // Não permite selecionar o mesmo nó como inicial e final
                    if (clickedNode.Value != _graphVisualizer.SelectedStartNode.Value)
                    {
                        _graphVisualizer.SetEndNode(clickedNode.Value);
                    }
                }
                // Se já tem ambos, reinicia a seleção
                else
                {
                    _graphVisualizer.ClearSelection();
                    _graphVisualizer.SetStartNode(clickedNode.Value);
                }
                
                // Limpa o caminho destacado ao mudar seleção
                _graphVisualizer?.ClearHighlightedPath();
                _lastResult = null;
            }
        }
        
        _changeGraphButton?.Update(_currentMouseState, _previousMouseState);
        _bruteForceButton?.Update(_currentMouseState, _previousMouseState);
        _dijkstraButton?.Update(_currentMouseState, _previousMouseState);
        _aStarButton?.Update(_currentMouseState, _previousMouseState);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        
        _spriteBatch.Begin();
        
        DrawSidebar();
        DrawGraph();
        
        _spriteBatch.End();

        base.Draw(gameTime);
    }
    
    private void DrawSidebar()
    {
        int height = _graphics.PreferredBackBufferHeight;
        
        _spriteBatch.Draw(_pixel, new Rectangle(0, 0, SidebarWidth, height), Color.Black * 0.8f);
        _spriteBatch.Draw(_pixel, new Rectangle(SidebarWidth, 0, 2, height), new Color(100, 100, 100));
        
        _changeGraphButton?.Draw(_spriteBatch);
        _bruteForceButton?.Draw(_spriteBatch);
        _dijkstraButton?.Draw(_spriteBatch);
        _aStarButton?.Draw(_spriteBatch);
        
        Vector2 labelPos = new(ButtonSpacing, ButtonSpacing + ButtonHeight + ButtonSpacing / 2 + 5);
        _spriteBatch.DrawString(_font, "ALGORITMOS:", labelPos, Color.Gray);
        
        // Mostrar nós selecionados
        int selectionY = _graphics.PreferredBackBufferHeight - 180;
        _spriteBatch.DrawString(_font, "SELECAO:", new Vector2(ButtonSpacing, selectionY), Color.Gray);
        
        if (_graphVisualizer?.SelectedStartNode.HasValue == true)
        {
            string startText = $"Inicio: {_graphVisualizer.SelectedStartNode.Value + 1}";
            _spriteBatch.DrawString(_font, startText, new Vector2(ButtonSpacing, selectionY + 25), new Color(100, 200, 255));
        }
        else
        {
            _spriteBatch.DrawString(_font, "Inicio: (clique)", new Vector2(ButtonSpacing, selectionY + 25), Color.DarkGray);
        }
        
        if (_graphVisualizer?.SelectedEndNode.HasValue == true)
        {
            string endText = $"Fim: {_graphVisualizer.SelectedEndNode.Value + 1}";
            _spriteBatch.DrawString(_font, endText, new Vector2(ButtonSpacing, selectionY + 50), new Color(255, 200, 100));
        }
        else
        {
            _spriteBatch.DrawString(_font, "Fim: (clique)", new Vector2(ButtonSpacing, selectionY + 50), Color.DarkGray);
        }
        
    }
    
    private void DrawGraph()
    {
        if (_graphVisualizer == null) return;
        
        Vector2 offset = new(SidebarWidth + GraphAreaMargin, GraphAreaMargin);
        _graphVisualizer.Draw(_spriteBatch, offset);
        
        DrawSearchInfo(offset);
    }
    
    private void DrawSearchInfo(Vector2 graphOffset)
    {
        int infoX = SidebarWidth + GraphAreaMargin + 10;
        int infoY = _graphics.PreferredBackBufferHeight - 130;
        
        lock (_lockObject)
        {
            if (_isSearching)
            {
                if (_currentAlgorithm == "BruteForce" && _bruteForceSolver != null)
                {
                    // Informações durante a busca (Brute Force)
                    _spriteBatch.DrawString(_font, "Buscando (Força Bruta)...", new Vector2(infoX, infoY), Color.Yellow);
                    _spriteBatch.DrawString(_font, $"Caminhos parciais: {_bruteForceSolver.PartialPathsExplored:N0}", 
                        new Vector2(infoX, infoY + 25), Color.White);
                    _spriteBatch.DrawString(_font, $"Caminhos completos: {_bruteForceSolver.PathsExplored:N0}", 
                        new Vector2(infoX, infoY + 50), Color.Gray);
                    
                    if (_bruteForceSolver.BestDistanceSoFar < int.MaxValue)
                    {
                        _spriteBatch.DrawString(_font, $"Melhor distancia: {_bruteForceSolver.BestDistanceSoFar}", 
                            new Vector2(infoX, infoY + 75), Color.Lime);
                    }
                }
                else if (_currentAlgorithm == "Dijkstra" && _dijkstraSolver != null)
                {
                    // Informações durante a busca (Dijkstra)
                    _spriteBatch.DrawString(_font, "Buscando (Dijkstra)...", new Vector2(infoX, infoY), Color.Yellow);
                    _spriteBatch.DrawString(_font, $"Nos explorados: {_dijkstraSolver.NodesExplored:N0}", 
                        new Vector2(infoX, infoY + 25), Color.White);
                    
                    if (_dijkstraSolver.BestDistanceSoFar < int.MaxValue)
                    {
                        _spriteBatch.DrawString(_font, $"Distancia atual: {_dijkstraSolver.BestDistanceSoFar}", 
                            new Vector2(infoX, infoY + 50), Color.Lime);
                    }
                }
            }
            else if (_lastResult != null)
            {
                // Resultados finais
                Color resultColor = _lastResult.Success ? Color.Lime : Color.Red;
                
                _spriteBatch.DrawString(_font, "RESULTADO:", new Vector2(infoX, infoY), resultColor);
                _spriteBatch.DrawString(_font, $"Caminhos parciais: {_lastResult.PartialPathsExplored:N0}", 
                    new Vector2(infoX, infoY + 25), Color.White);
                _spriteBatch.DrawString(_font, $"Tempo: {_lastResult.ElapsedTime.TotalSeconds:F2}s", 
                    new Vector2(infoX, infoY + 50), Color.White);
                
                if (_lastResult.Success)
                {
                    string pathStr = string.Join(", ", _lastResult.BestPath.ConvertAll(n => (n + 1).ToString()));
                    _spriteBatch.DrawString(_font, $"Melhor caminho: {pathStr}", 
                        new Vector2(infoX, infoY + 75), Color.Cyan);
                    _spriteBatch.DrawString(_font, $"Distancia: {_lastResult.BestDistance}", 
                        new Vector2(infoX, infoY + 100), Color.Lime);
                }
            }
        }
    }
    
    #endregion
    
    #region Helper Methods
    
    private void SaveLogsToFile(string filename, List<string> logs)
    {
        try
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            List<string> output =
            [
                "=".PadRight(80, '='),
                $"Log gerado em: {timestamp}",
                $"Grafo: {_currentGraphType}",
                "=".PadRight(80, '='),
                "",
                .. logs,
            ];
            
            File.WriteAllLines(filename, output);
            Console.WriteLine($"[DEBUG] Logs salvos em: {filename}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERRO] Falha ao salvar logs: {ex.Message}");
        }
    }
    
    #endregion
}
