using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SideScrollerShooter.Engine;
using SideScrollerShooter.Game.Managers;
namespace SideScrollerShooter.Game.States;
public sealed class HighScoresState : IGameState
{
    private HighScoreManager _scores;
    public void Enter()=>_scores=new HighScoreManager();
    public void Exit() { }
    public void Update(float dt) { if(InputManager.Pressed(Keys.Escape)) StateManager.ChangeState(new MenuState()); }
    public void Draw(SpriteBatch sb)
    {
        sb.Begin(); Ui.Background(sb); Ui.Centre(sb,"FLIGHT RECORDS",70,Ui.Accent,1.8f);
        Ui.Panel(sb,new Rectangle(100,140,Game1.ScreenWidth-200,475));
        Ui.Text(sb,"RANK",125,165,Ui.Accent); Ui.Text(sb,"PILOT",230,165,Ui.Accent); Ui.Text(sb,"SCORE",490,165,Ui.Accent); Ui.Text(sb,"SECTOR",670,165,Ui.Accent); Ui.Text(sb,"DATE (UTC)",820,165,Ui.Accent);
        for(int i=0;i<_scores.Entries.Count;i++)
        {
            var e=_scores.Entries[i]; float y=205+i*37;
            Ui.Text(sb,$"{i+1:00}",125,y); Ui.Text(sb,e.Name,230,y); Ui.Text(sb,e.Score.ToString("N0"),490,y); Ui.Text(sb,e.Level.ToString(),670,y); Ui.Text(sb,e.RecordedAt.UtcDateTime.ToString("yyyy-MM-dd"),820,y,Color.LightGray,0.8f);
        }
        if(_scores.Entries.Count==0) Ui.Centre(sb,"NO FLIGHTS RECORDED YET",340,Color.SlateGray);
        Ui.Centre(sb,_scores.Status,630,Color.Orange,0.75f); Ui.Centre(sb,"ESC / RETURN TO MENU",665,Ui.Accent); sb.End();
    }
}
