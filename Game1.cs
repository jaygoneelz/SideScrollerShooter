using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SideScrollerShooter.Engine;
using SideScrollerShooter.Game;
using SideScrollerShooter.Game.States;
using SideScrollerShooter.Diagnostics;
namespace SideScrollerShooter;
public class Game1 : Microsoft.Xna.Framework.Game
{
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private readonly SmokeRunner _smoke;
    public static Game1 Instance { get; private set; }
    public static ContentManager ContentRef { get; private set; }
    public static WorldConfig World { get; private set; }
    public static int ScreenWidth=>World.Width;
    public static int ScreenHeight=>World.Height;
    public static bool DebugView { get; private set; }
    public Game1(bool smokeTest=false)
    {
        Instance=this;
        World=WorldConfig.Load(Path.Combine(AppContext.BaseDirectory,"Config","world.json"));
        _graphics=new GraphicsDeviceManager(this) { PreferredBackBufferWidth=World.Width,PreferredBackBufferHeight=World.Height };
        IsFixedTimeStep=true; TargetElapsedTime=TimeSpan.FromSeconds(1.0/60.0);
        if(smokeTest)
        {
            _smoke=new SmokeRunner(); IsFixedTimeStep=false;
            _graphics.SynchronizeWithVerticalRetrace=false;
            InputManager.ReadKeyboard=_smoke.ReadKeyboard;
        }
        Content.RootDirectory="Content"; IsMouseVisible=true; Window.Title="ORBIT / 379 - SideScrollerShooter";
    }
    protected override void LoadContent()
    {
        _spriteBatch=new SpriteBatch(GraphicsDevice); ContentRef=Content;
        ResourceManager.Initialize(Content); FontManager.Initialize(Content); AudioManager.Initialize(Content); Ui.Initialize(GraphicsDevice);
        _smoke?.CheckGameplay();
        StateManager.ChangeState(new MenuState());
    }
    protected override void Update(GameTime gameTime)
    {
        float dt=_smoke!=null?1f/60:(float)gameTime.ElapsedGameTime.TotalSeconds;
        _smoke?.BeforeUpdate();
        InputManager.Update();
        if(InputManager.Pressed(Keys.F3)) DebugView=!DebugView;
        if(InputManager.Pressed(Keys.M)) AudioManager.ToggleMute();
        Ui.Update(dt); StateManager.Update(dt); base.Update(gameTime);
    }
    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(new Color(4,10,23)); StateManager.Draw(_spriteBatch); base.Draw(gameTime);
        _smoke?.AfterDraw(GraphicsDevice);
    }
    protected override void UnloadContent()
    { StateManager.Shutdown(); AudioManager.Dispose(); Ui.Dispose(); _spriteBatch.Dispose(); base.UnloadContent(); }
}
