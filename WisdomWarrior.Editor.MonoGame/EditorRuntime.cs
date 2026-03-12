using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WisdomWarrior.Editor.MonoGame.Overlays;
using WisdomWarrior.Editor.MonoGame.Tools;
using WisdomWarrior.Engine.Core.Interfaces;
using WisdomWarrior.Engine.MonoGame;

namespace WisdomWarrior.Editor.MonoGame;

public class EditorRuntime : Game
{
    private readonly ToolContext _context;
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private OverlayManager _overlayManager;
    private ToolManager _toolManager;

    private TextureManager _textureManager;
    private RenderService _renderService;
    private Engine.Core.Engine _engine;

    public EditorRuntime(ToolContext context)
    {
        _context = context;
        _overlayManager = new OverlayManager(context);
        _toolManager = new ToolManager(context);
        _graphics = new GraphicsDeviceManager(this);

        _renderService = new RenderService();
        _engine = new Engine.Core.Engine(_renderService);
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _textureManager = new TextureManager(GraphicsDevice);
        _renderService.LoadContent(_spriteBatch, _textureManager);
        _engine.LoadContent();
        _overlayManager.Load(GraphicsDevice);

        base.LoadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        _toolManager.Update();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _engine.Draw();
        _overlayManager.Draw(_spriteBatch);

        base.Draw(gameTime);
    }
}