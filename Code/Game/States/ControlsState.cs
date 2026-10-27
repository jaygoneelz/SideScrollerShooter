using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SideScrollerShooter.Engine;
namespace SideScrollerShooter.Game.States;
public class ControlsState : IGameState
{
    protected virtual string Title=>"FLIGHT CONTROLS";
    protected virtual string[] Lines=>new[]{"WASD / Arrow keys    Move the ship in four directions","Hold SPACE          Fire lasers","P                   Pause / resume","ESC                 Return to menu","M                   Mute / unmute music and effects","F3                  Bounds, enemy states and profiling","R after a mission   Retry","High scores         Enter a pilot name, then press Enter"};
    public void Enter() { }
    public void Exit() { }
    public void Update(float dt) { if(InputManager.Pressed(Keys.Escape)) StateManager.ChangeState(new MenuState()); }
    public void Draw(SpriteBatch sb)
    {
        sb.Begin(); Ui.Background(sb); Ui.Centre(sb,Title,100,Ui.Accent,1.8f);
        Ui.Panel(sb,new Rectangle(70,185,Game1.ScreenWidth-140,390));
        for(int i=0;i<Lines.Length;i++) Ui.Text(sb,Lines[i],100,215+i*40,Color.LightGray,0.9f);
        Ui.Centre(sb,"ESC / RETURN TO MENU",620,Ui.Accent); sb.End();
    }
}
