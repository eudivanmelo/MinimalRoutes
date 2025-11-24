using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace MinimalRoutes.Components
{
    public class Button(Rectangle bounds, string text, Texture2D pixel, SpriteFont font)
    {
        private readonly SpriteFont _font = font;
        private readonly Texture2D _pixel = pixel;
        
        private MouseState _previousMouseState;
        private bool _isHovering;
        private bool _isPressed;

        public Rectangle Bounds { get; set; } = bounds;
        public string Text { get; set; } = text;
        public bool IsEnabled { get; set; } = true;
        
        // Cores customizáveis
        public Color BackgroundColor { get; set; } = new Color(60, 60, 60);
        public Color HoverColor { get; set; } = new Color(80, 80, 80);
        public Color PressedColor { get; set; } = new Color(40, 40, 40);
        public Color DisabledColor { get; set; } = new Color(30, 30, 30);
        public Color TextColor { get; set; } = Color.White;
        public Color DisabledTextColor { get; set; } = Color.Gray;
        public Color BorderColor { get; set; } = new Color(100, 100, 100);
        
        public int BorderThickness { get; set; } = 2;

        public event EventHandler Click;

        public Button(int x, int y, int width, int height, string text, Texture2D pixel, SpriteFont font)
            : this(new Rectangle(x, y, width, height), text, pixel, font)
        {}

        public void Update(MouseState currentMouseState, MouseState previousMouseState)
        {
            _previousMouseState = previousMouseState;
            
            if (!IsEnabled)
            {
                _isHovering = false;
                _isPressed = false;
                return;
            }

            Point mousePosition = currentMouseState.Position;
            _isHovering = Bounds.Contains(mousePosition);

            if (_isHovering && currentMouseState.LeftButton == ButtonState.Pressed)
            {
                _isPressed = true;
            }

            if (_isPressed && currentMouseState.LeftButton == ButtonState.Released)
            {
                if (_isHovering)
                {
                    Click?.Invoke(this, EventArgs.Empty);
                }
                _isPressed = false;
            }

            if (!_isHovering && currentMouseState.LeftButton == ButtonState.Released)
            {
                _isPressed = false;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Color backgroundColor = GetCurrentBackgroundColor();
            Color textColor = IsEnabled ? TextColor : DisabledTextColor;

            // Desenhar fundo
            spriteBatch.Draw(_pixel, Bounds, backgroundColor);

            // Desenhar borda
            if (BorderThickness > 0)
            {
                DrawBorder(spriteBatch, Bounds, BorderThickness, BorderColor);
            }

            // Desenhar texto centralizado
            if (!string.IsNullOrEmpty(Text))
            {
                Vector2 textSize = _font.MeasureString(Text);
                Vector2 textPosition = new Vector2(
                    Bounds.X + (Bounds.Width - textSize.X) / 2,
                    Bounds.Y + (Bounds.Height - textSize.Y) / 2
                );
                
                spriteBatch.DrawString(_font, Text, textPosition, textColor);
            }
        }

        private Color GetCurrentBackgroundColor()
        {
            if (!IsEnabled)
                return DisabledColor;
            
            if (_isPressed)
                return PressedColor;
            
            if (_isHovering)
                return HoverColor;
            
            return BackgroundColor;
        }

        private void DrawBorder(SpriteBatch spriteBatch, Rectangle bounds, int thickness, Color color)
        {
            // Top
            spriteBatch.Draw(_pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, thickness), color);
            // Bottom
            spriteBatch.Draw(_pixel, new Rectangle(bounds.X, bounds.Bottom - thickness, bounds.Width, thickness), color);
            // Left
            spriteBatch.Draw(_pixel, new Rectangle(bounds.X, bounds.Y, thickness, bounds.Height), color);
            // Right
            spriteBatch.Draw(_pixel, new Rectangle(bounds.Right - thickness, bounds.Y, thickness, bounds.Height), color);
        }
        
        // Métodos auxiliares para facilitar o uso
        public void SetPosition(int x, int y)
        {
            Bounds = new Rectangle(x, y, Bounds.Width, Bounds.Height);
        }
        
        public void SetSize(int width, int height)
        {
            Bounds = new Rectangle(Bounds.X, Bounds.Y, width, height);
        }
        
        public void Enable() => IsEnabled = true;
        
        public void Disable() => IsEnabled = false;
    }
}
