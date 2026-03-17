using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WisdomWarrior.Editor.MonoGame.Overlays;
using WisdomWarrior.Editor.MonoGame.Selection;
using WisdomWarrior.Editor.MonoGame.Tools;
using WisdomWarrior.Engine.Core;
using WisdomWarrior.Engine.Core.Components;
using WisdomWarrior.Engine.MonoGame;
using Vector2 = System.Numerics.Vector2;

namespace WisdomWarrior.Editor.MonoGame;

public class EditorRuntime : Game
{
    private readonly ToolContext _context;
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private OverlayManager _overlayManager;
    private ToolManager _toolManager;

    private TextureManager? _textureManager;
    private readonly EditorRenderService _renderService;
    private readonly Engine.Core.Engine _engine;
    private readonly SceneSpriteHitTestService _sceneSpriteHitTestService = new();
    private Guid? _preparedSceneId;

    public EditorRuntime(ToolContext context)
    {
        _context = context;
        _overlayManager = new OverlayManager(context);
        _toolManager = new ToolManager(context);
        _graphics = new GraphicsDeviceManager(this);

        _renderService = new EditorRenderService();
        _engine = new Engine.Core.Engine(_renderService);
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _textureManager = new TextureManager(GraphicsDevice);
        _renderService.LoadContent(_spriteBatch, _textureManager);
        _engine.LoadContent();
        PrepareActiveSceneForInteraction();
        _overlayManager.Load(GraphicsDevice);

        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        PrepareActiveSceneForInteraction();
        _toolManager.Update();
        _context.Input.AdvanceFrame();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _engine.Draw();
        _overlayManager.Draw(_spriteBatch);

        base.Draw(gameTime);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _textureManager?.Dispose();
            _textureManager = null;
        }

        base.Dispose(disposing);
    }

    public GameEntity? HitTestEntityAtViewportPoint(Vector2 viewportPosition)
    {
        PrepareActiveSceneForInteraction();

        var activeScene = SceneManager.GetCurrentScene();
        if (activeScene == null)
        {
            return null;
        }

        return _sceneSpriteHitTestService.HitTest(activeScene, viewportPosition, _renderService.CanRenderTexture);
    }

    public void PrepareActiveSceneForInteraction()
    {
        var activeScene = SceneManager.GetCurrentScene();
        if (activeScene == null)
        {
            _preparedSceneId = null;
            return;
        }

        if (_preparedSceneId == activeScene.Id)
        {
            return;
        }

        var texturePaths = activeScene.GetEntitiesWith<Sprite>()
            .SelectMany(entity => entity.Components.OfType<Sprite>())
            .Select(sprite => sprite.Image?.AssetPath)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        _renderService.PreloadTextures(texturePaths);
        _preparedSceneId = activeScene.Id;
    }
}
