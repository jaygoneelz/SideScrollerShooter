namespace SideScrollerShooter.Game.States;
public sealed class RulesState : ControlsState
{
    protected override string Title=>"MISSION BRIEFING";
    protected override string[] Lines=>new[]{"Survive all three sectors to complete the mission.","White scouts fly straight; orange weavers patrol vertically.","Purple hunters lock onto your position before charging.","Destroy scouts / weavers / hunters for 100 / 150 / 200 points.","A collision costs one life, followed by brief invulnerability.","R = rapid fire; S = shield; + = repair one lost life.","Each sector changes enemy patterns, speed and spawn rate.","Your ten best runs appear in the saved high-score table."};
}
