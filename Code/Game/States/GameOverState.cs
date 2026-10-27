using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SideScrollerShooter.Engine;
using SideScrollerShooter.Game.Managers;
namespace SideScrollerShooter.Game.States;
public sealed class GameOverState : IGameState
{
    private readonly int _score,_level;
    private readonly bool _victory;
    private string _name="",_status="Type a pilot name (letters/numbers, max 12).";
    private bool _saved;
    public GameOverState(int score,int level,bool victory) { _score=score; _level=level; _victory=victory; }
    public void Enter()=>Game1.Instance.Window.TextInput+=OnText;
    public void Exit()=>Game1.Instance.Window.TextInput-=OnText;
    private void OnText(object sender,TextInputEventArgs e)
    {
        if(_saved) return;
        if(e.Character=='\b') { if(_name.Length>0) _name=_name[..^1]; }
        else if(_name.Length<12 && e.Character<128 && char.IsLetterOrDigit(e.Character)) _name+=char.ToUpperInvariant(e.Character);
    }
    private void Save()
    {
        if(_saved) return;
        var table=new HighScoreManager(); _saved=table.Add(_name,_score,_level); _status=table.Status;
    }
    public void Update(float dt)
    {
        if(InputManager.Pressed(Keys.Enter)) Save();
        // Avoid interpreting letters typed into the pilot name as shortcuts.
        if(_saved && InputManager.Pressed(Keys.R)) StateManager.ChangeState(new PlayState());
        if(_saved && InputManager.Pressed(Keys.H)) StateManager.ChangeState(new HighScoresState());
        if(InputManager.Pressed(Keys.Escape)) { Save(); if(_saved) StateManager.ChangeState(new MenuState()); }
    }
    public void Draw(SpriteBatch sb)
    {
        sb.Begin(); Ui.Background(sb); Ui.Centre(sb,_victory?"MISSION COMPLETE":"MISSION ENDED",120,_victory?Ui.Accent:Color.Orange,2);
        Ui.Centre(sb,$"SCORE  {_score:N0}     SECTOR  {_level}",220,Color.White,1.4f);
        Ui.Panel(sb,new Rectangle(Game1.ScreenWidth/2-340,310,680,170));
        Ui.Centre(sb,"PILOT / "+(_name.Length==0?"PILOT":_name),335,Ui.Accent,1.2f);
        Ui.Centre(sb,_status,395,Color.LightGray,0.8f);
        Ui.Centre(sb,_saved?"R retry   H high scores   ESC menu":"ENTER save score   ESC save and return",550,Color.White);
        sb.End();
    }
}
