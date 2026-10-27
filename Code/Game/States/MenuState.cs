using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SideScrollerShooter.Engine;
namespace SideScrollerShooter.Game.States;
public sealed class MenuState : IGameState
{
    private int _selected;
    private readonly string[] _options={"START MISSION","HIGH SCORES","HOW TO PLAY","CONTROLS","EXIT"};
    public void Enter() { }
    public void Exit() { }
    public void Update(float dt)
    {
        if(InputManager.Pressed(Keys.Down)||InputManager.Pressed(Keys.S)) _selected=(_selected+1)%_options.Length;
        if(InputManager.Pressed(Keys.Up)||InputManager.Pressed(Keys.W)) _selected=(_selected+_options.Length-1)%_options.Length;
        if(!InputManager.Pressed(Keys.Enter)) return;
        switch(_selected)
        {
            case 0: StateManager.ChangeState(new PlayState()); break;
            case 1: StateManager.ChangeState(new HighScoresState()); break;
            case 2: StateManager.ChangeState(new RulesState()); break;
            case 3: StateManager.ChangeState(new ControlsState()); break;
            case 4: Game1.Instance.Exit(); break;
        }
    }
    public void Draw(SpriteBatch sb)
    {
        sb.Begin(); Ui.Background(sb);
        Ui.Centre(sb,"ORBIT / 379",100,Ui.Accent,2.6f);
        Ui.Centre(sb,"SIDE SCROLLER SHOOTER",164,Color.LightGray,0.9f);
        Ui.Centre(sb,"Survive three sectors. Break the high-score table.",215,Color.LightGray);
        Ui.Panel(sb,new Rectangle(Game1.ScreenWidth/2-240,265,480,265));
        for(int i=0;i<_options.Length;i++) Ui.Centre(sb,(i==_selected?">  ":"   ")+_options[i],285+i*46,i==_selected?Ui.Accent:Color.SlateGray);
        Ui.Centre(sb,"WASD / ARROWS move   SPACE fire   P pause",575,Color.White,0.85f);
        Ui.Centre(sb,"UP / DOWN select   ENTER confirm   M mute   F3 diagnostics",610,Color.SlateGray,0.8f);
        Ui.Centre(sb,AudioManager.Status,650,Color.Orange,0.75f);
        sb.End();
    }
}
