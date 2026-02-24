using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WisdomWarrior.Editor.MonoGame.EditorUI;
using WisdomWarrior.Engine.Core;
using WisdomWarrior.Engine.Core.Components;

namespace WisdomWarrior.Editor.MonoGame;

public class EditorRuntime : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private EditorViewportRenderer? _editorViewportRenderer;
    private FontSystem _fontSystem;

    public bool IsHovering { get; private set; }
    public System.Numerics.Vector2 LocalMousePosition { get; set; }
    public GameEntity? SelectedEntity { get; set; }
    public bool RequestHideCursor { get; private set; }
    public float CursorScale { get; set; } = 1f;

    public bool IsDragging { get; private set; }
    private Vector2 _grabOffset;

    public EditorRuntime()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _editorViewportRenderer = new EditorViewportRenderer(GraphicsDevice);

        _fontSystem = new FontSystem();
        _fontSystem.AddFont(File.ReadAllBytes(@"C:\Windows\Fonts\arial.ttf"));

        base.LoadContent();
    }

    public void StartDragging()
    {
        if (!IsHovering || SelectedEntity == null) return;

        var transform = SelectedEntity.Components.OfType<Transform>().FirstOrDefault();
        if (transform == null) return;

        IsDragging = true;
        RequestHideCursor = true;

        // _grabOffset = transform.Position - LocalMousePosition;
    }

    public void StopDragging()
    {
        IsDragging = false;
        RequestHideCursor = false;
    }

    public void UpdateObjectPosition()
    {
        if (IsDragging && SelectedEntity != null)
        {
            var transform = SelectedEntity.Components.OfType<Transform>().FirstOrDefault();
            if (transform != null)
            {
                // Update the position based on the mouse + the offset
                var position = LocalMousePosition + _grabOffset;
                transform.Position = new System.Numerics.Vector2(position.X, position.Y);

                // // Trigger the "Dirty" flag so the scene saves!
                transform.NotifyChanged();
            }
        }
    }

    protected override void Update(GameTime gameTime)
    {
        UpdateObjectPosition();

        IsHovering = _editorViewportRenderer?.IsMouseOver(
            LocalMousePosition,
            SelectedEntity,
            (float)gameTime.TotalGameTime.TotalSeconds
        ) ?? false;

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin(blendState: BlendState.NonPremultiplied);
        _editorViewportRenderer?.Draw(_spriteBatch, SelectedEntity);
        _editorViewportRenderer?.Draw(_spriteBatch, LocalMousePosition, IsHovering, CursorScale);
        _spriteBatch.End();

        _spriteBatch.Begin(blendState: BlendState.AlphaBlend);
        var font = _fontSystem.GetFont(18); // Get a font at size 18
        _spriteBatch.DrawString(font, $"Mouse: {LocalMousePosition.ToString()}", new Vector2(10, 10), Color.White);
        if (SelectedEntity != null)
        {
            var transform = SelectedEntity.Components.OfType<Transform>().FirstOrDefault();
            var vec = new Vector2(transform.Position.X, transform.Position.Y);
            _spriteBatch.DrawString(font, $"Entity: {vec.ToString()}", new Vector2(10, 30), Color.White);
            var diff = LocalMousePosition - vec;
            _spriteBatch.DrawString(font, $"Difference: {diff.ToString()}", new Vector2(10, 50), Color.White);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}