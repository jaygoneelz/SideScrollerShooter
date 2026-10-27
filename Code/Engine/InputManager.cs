using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
namespace SideScrollerShooter.Engine;

/// <summary>Only the input layer reads hardware; gameplay receives semantic actions.</summary>
public static class InputManager
{
    private static KeyboardState _current, _previous;
    public static Func<KeyboardState> ReadKeyboard { get; set; }=Keyboard.GetState;
    public static void Update() { _previous=_current; _current=ReadKeyboard(); }
    public static bool Pressed(Keys key) => _current.IsKeyDown(key) && _previous.IsKeyUp(key);
    public static void PublishGameplay()
    {
        var move=Vector2.Zero;
        if(_current.IsKeyDown(Keys.W)||_current.IsKeyDown(Keys.Up)) move.Y--;
        if(_current.IsKeyDown(Keys.S)||_current.IsKeyDown(Keys.Down)) move.Y++;
        if(_current.IsKeyDown(Keys.A)||_current.IsKeyDown(Keys.Left)) move.X--;
        if(_current.IsKeyDown(Keys.D)||_current.IsKeyDown(Keys.Right)) move.X++;
        if(move.LengthSquared()>1) move.Normalize();
        EventManager.Publish(Events.Move,move);
        if(_current.IsKeyDown(Keys.Space)) EventManager.Publish(Events.Shoot);
    }
}
