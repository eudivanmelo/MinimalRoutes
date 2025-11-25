using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace MinimalRoutes.Components;

public class GraphVisualizer
{
    private readonly SpriteFont _font;
    private readonly Texture2D _pixel;
    
    private readonly Dictionary<int, Vector2> _nodePositions;
    private readonly List<(int from, int to, int weight)> _edges;
    private List<int> _currentHighlightedPath;
    private int? _selectedStartNode = null;
    private int? _selectedEndNode = null;
    
    private const int NodeRadius = 20;
    private readonly int _width;
    private readonly int _height;
    
    // Cores
    private readonly Color _edgeColor = Color.Gray;

    private readonly Color _nodeColor = Color.Red;
    private readonly Color _nodeBorderColor = Color.DarkRed;
    private readonly Color _nodeTextColor = Color.White;
    private readonly Color _highlightedEdgeColor = new(255, 100, 100);
    private readonly Color _highlightedNodeColor = new(100, 255, 100);
    private readonly Color _selectedStartNodeColor = new(100, 200, 255); // Azul para nó inicial
    private readonly Color _selectedEndNodeColor = new(255, 200, 100);   // Laranja para nó final
    
    private const float EdgeThickness = 1.5f;
    private const float HighlightedEdgeThickness = 3f;
    
    public GraphVisualizer(GraphicsDevice graphicsDevice, SpriteFont font, int width, int height)
    {
        _font = font;
        _width = width;
        _height = height;
        
        _pixel = new Texture2D(graphicsDevice, 1, 1);
        _pixel.SetData([Color.White]);
        
        _nodePositions = [];
        _edges = [];
        _currentHighlightedPath = null;
    }
    
    // ========== MÉTODOS PÚBLICOS ==========
    
    /// <summary>
    /// Carrega e processa a matriz de distâncias do grafo
    /// </summary>
    public void LoadGraph(int?[,] distanceMatrix)
    {
        if (distanceMatrix == null) return;
        
        int nodeCount = distanceMatrix.GetLength(0);
        BuildEdges(distanceMatrix, nodeCount);
        CalculateNodePositions(nodeCount);
    }
    
    /// <summary>
    /// Define o caminho atual a ser destacado
    /// </summary>
    public void SetHighlightedPath(List<int> path)
    {
        _currentHighlightedPath = path;
    }
    
    /// <summary>
    /// Limpa o caminho destacado
    /// </summary>
    public void ClearHighlightedPath()
    {
        _currentHighlightedPath = null;
    }
    
    /// <summary>
    /// Define o nó inicial selecionado
    /// </summary>
    public void SetStartNode(int? node)
    {
        _selectedStartNode = node;
    }
    
    /// <summary>
    /// Define o nó final selecionado
    /// </summary>
    public void SetEndNode(int? node)
    {
        _selectedEndNode = node;
    }
    
    /// <summary>
    /// Limpa a seleção de nós
    /// </summary>
    public void ClearSelection()
    {
        _selectedStartNode = null;
        _selectedEndNode = null;
    }
    
    /// <summary>
    /// Retorna o nó clicado na posição especificada, ou null se não houver
    /// </summary>
    public int? GetNodeAtPosition(Vector2 clickPosition, Vector2 offset)
    {
        foreach (var (node, pos) in _nodePositions)
        {
            Vector2 nodeScreenPos = pos + offset;
            float distance = Vector2.Distance(clickPosition, nodeScreenPos);
            
            if (distance <= NodeRadius)
            {
                return node;
            }
        }
        return null;
    }
    
    public int? SelectedStartNode => _selectedStartNode;
    public int? SelectedEndNode => _selectedEndNode;
    
    /// <summary>
    /// Desenha o grafo completo
    /// </summary>
    public void Draw(SpriteBatch spriteBatch, Vector2 offset)
    {
        if (_nodePositions.Count == 0) return;
        
        DrawEdges(spriteBatch, offset);
        DrawNodes(spriteBatch, offset);
    }
    
    public int NodeCount => _nodePositions.Count;
    
    // ========== CONSTRUÇÃO DO GRAFO ==========
    
    private void BuildEdges(int?[,] distanceMatrix, int nodeCount)
    {
        _edges.Clear();
        
        for (int i = 0; i < nodeCount; i++)
        {
            for (int j = i + 1; j < nodeCount; j++)
            {
                if (distanceMatrix[i, j].HasValue && distanceMatrix[i, j].Value > 0)
                {
                    _edges.Add((i, j, distanceMatrix[i, j].Value));
                }
            }
        }
    }
    
    private void CalculateNodePositions(int nodeCount)
    {
        _nodePositions.Clear();
        
        Random rand = new(321);
        float width = _width;
        float height = _height;
        
        // Posicionamento inicial aleatório
        for (int i = 0; i < nodeCount; i++)
        {
            _nodePositions[i] = new Vector2(
                (float)(rand.NextDouble() * (width - 90) + rand.Next(25, 75)),
                (float)(rand.NextDouble() * (height - 90) + rand.Next(25, 75))
            );
        }
        
        // Aplicar Force-Directed Layout (Fruchterman-Reingold)
        ApplyForceDirectedLayout(nodeCount, width, height);
    }
    
    private void ApplyForceDirectedLayout(int nodeCount, float width, float height)
    {
        float k = (float)Math.Sqrt(width * height / nodeCount); // Constante de mola ideal
        int iterations = 100;
        
        for (int iter = 0; iter < iterations; iter++)
        {
            Dictionary<int, Vector2> displacement = [];
            
            // Inicializar deslocamentos
            for (int i = 0; i < nodeCount; i++)
            {
                displacement[i] = Vector2.Zero;
            }
            
            // Forças repulsivas entre todos os nós
            for (int i = 0; i < nodeCount; i++)
            {
                for (int j = i + 1; j < nodeCount; j++)
                {
                    Vector2 delta = _nodePositions[i] - _nodePositions[j];
                    float distance = delta.Length();
                    
                    if (distance < 0.01f) distance = 0.01f;
                    
                    float repulsiveForce = k * k / distance;
                    Vector2 force = Vector2.Normalize(delta) * repulsiveForce;
                    
                    displacement[i] += force;
                    displacement[j] -= force;
                }
            }
            
            // Forças atrativas apenas entre nós conectados
            foreach (var (from, to, weight) in _edges)
            {
                Vector2 delta = _nodePositions[from] - _nodePositions[to];
                float distance = delta.Length();
                
                if (distance < 0.01f) distance = 0.01f;
                
                float attractiveForce = distance * distance / k;
                Vector2 force = Vector2.Normalize(delta) * attractiveForce;
                
                displacement[from] -= force;
                displacement[to] += force;
            }
            
            // Aplicar deslocamentos com temperatura decrescente
            float temperature = width / 10f * (1f - (float)iter / iterations);
            
            for (int i = 0; i < nodeCount; i++)
            {
                float dispLength = displacement[i].Length();
                
                if (dispLength > 0.01f)
                {
                    float limitedDisp = Math.Min(dispLength, temperature);
                    _nodePositions[i] += Vector2.Normalize(displacement[i]) * limitedDisp;
                    
                    // Manter dentro dos limites
                    _nodePositions[i] = new Vector2(
                        Math.Clamp(_nodePositions[i].X, 20, width - 20),
                        Math.Clamp(_nodePositions[i].Y, 20, height - 20)
                    );
                }
            }
        }
    }
    
    // ========== RENDERIZAÇÃO ==========
    
    private void DrawEdges(SpriteBatch spriteBatch, Vector2 offset)
    {
        foreach (var (from, to, weight) in _edges)
        {
            Vector2 fromPos = _nodePositions[from] + offset;
            Vector2 toPos = _nodePositions[to] + offset;
            
            bool isHighlighted = IsEdgeHighlighted(from, to);
            Color edgeColor = isHighlighted ? _highlightedEdgeColor : _edgeColor;
            float thickness = isHighlighted ? HighlightedEdgeThickness : EdgeThickness;
            
            DrawLine(spriteBatch, fromPos, toPos, edgeColor, thickness);
            DrawEdgeWeight(spriteBatch, fromPos, toPos, weight);
        }
    }
    
    private void DrawEdgeWeight(SpriteBatch spriteBatch, Vector2 from, Vector2 to, int weight)
    {
        Vector2 midPoint = (from + to) / 2;
        string weightText = weight.ToString();
        Vector2 textSize = _font.MeasureString(weightText);
        
        Rectangle textRect = new(
            (int)(midPoint.X - textSize.X / 2 - 2),
            (int)(midPoint.Y - textSize.Y / 2 - 1),
            (int)(textSize.X + 4),
            (int)(textSize.Y + 2)
        );
        spriteBatch.Draw(_pixel, textRect, Color.Black);
        spriteBatch.DrawString(_font, weightText, midPoint - textSize / 2, Color.White);
    }
    
    private void DrawNodes(SpriteBatch spriteBatch, Vector2 offset)
    {
        foreach (var node in _nodePositions)
        {
            Vector2 position = node.Value + offset;
            bool isHighlighted = _currentHighlightedPath?.Contains(node.Key) ?? false;
            bool isStartNode = _selectedStartNode.HasValue && _selectedStartNode.Value == node.Key;
            bool isEndNode = _selectedEndNode.HasValue && _selectedEndNode.Value == node.Key;
            
            Color nodeColor;
            Color borderColor;
            
            // Prioridade: nós selecionados > caminho destacado > nós normais
            if (isStartNode)
            {
                nodeColor = _selectedStartNodeColor;
                borderColor = Color.DarkBlue;
            }
            else if (isEndNode)
            {
                nodeColor = _selectedEndNodeColor;
                borderColor = Color.DarkOrange;
            }
            else if (isHighlighted)
            {
                nodeColor = _highlightedNodeColor;
                borderColor = Color.DarkGreen;
            }
            else
            {
                nodeColor = _nodeColor;
                borderColor = _nodeBorderColor;
            }
            
            DrawCircle(spriteBatch, position, NodeRadius, nodeColor, borderColor);
            
            string nodeText = (node.Key + 1).ToString();
            Vector2 textSize = _font.MeasureString(nodeText);
            spriteBatch.DrawString(_font, nodeText, position - textSize / 2, _nodeTextColor);
        }
    }
    
    // ========== UTILITÁRIOS ==========
    
    private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness)
    {
        Vector2 edge = end - start;
        float angle = (float)Math.Atan2(edge.Y, edge.X);
        
        spriteBatch.Draw(_pixel,
            new Rectangle((int)start.X, (int)start.Y, (int)edge.Length(), (int)thickness),
            null, color, angle, new Vector2(0, 0.5f), SpriteEffects.None, 0);
    }
    
    private void DrawCircle(SpriteBatch spriteBatch, Vector2 center, int radius, Color fillColor, Color borderColor)
    {
        // Desenhar círculo preenchido
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    spriteBatch.Draw(_pixel,
                        new Vector2(center.X + x, center.Y + y),
                        fillColor);
                }
            }
        }
        
        // Desenhar borda
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                int distSq = x * x + y * y;
                if (distSq >= (radius - 2) * (radius - 2) && distSq <= radius * radius)
                {
                    spriteBatch.Draw(_pixel,
                        new Vector2(center.X + x, center.Y + y),
                        borderColor);
                }
            }
        }
    }
    
    private bool IsEdgeHighlighted(int from, int to)
    {
        if (_currentHighlightedPath == null || _currentHighlightedPath.Count < 2) 
            return false;
        
        for (int i = 0; i < _currentHighlightedPath.Count - 1; i++)
        {
            if ((_currentHighlightedPath[i] == from && _currentHighlightedPath[i + 1] == to) ||
                (_currentHighlightedPath[i] == to && _currentHighlightedPath[i + 1] == from))
            {
                return true;
            }
        }
        
        return false;
    }
}
