using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Xna.Framework;
namespace SideScrollerShooter.Game;
public sealed class WorldConfig
{
    public int Width { get; set; }=1120;
    public int Height { get; set; }=720;
    public int Seed { get; set; }=379;
    public float PlayerX { get; set; }=130;
    public float PlayerY { get; set; }=360;
    public float PlayerSpeed { get; set; }=300;
    public int Lives { get; set; }=3;
    public float BulletSpeed { get; set; }=650;
    public float PowerUpInterval { get; set; }=9;
    public float PowerUpDuration { get; set; }=6;
    public float InvulnerabilitySeconds { get; set; }=1.25f;
    public List<LevelConfig> Levels { get; set; }=new();
    public Rectangle PlayArea => new(0,100,Width,Height-100);
    public static WorldConfig Load(string path)
    {
        var config=JsonSerializer.Deserialize<WorldConfig>(File.ReadAllText(path),new JsonSerializerOptions { PropertyNameCaseInsensitive=true }) ?? throw new InvalidDataException("Empty world configuration");
        config.Validate(); return config;
    }
    public void Validate()
    {
        if(Width<1120 || Width>1920 || Height<720 || Height>1080 || !Positive(PlayerSpeed) || PlayerSpeed>600 ||
           !Positive(BulletSpeed) || BulletSpeed>1200 || Lives<1 || Lives>9 || !Positive(PowerUpInterval) || PowerUpInterval<1 ||
           !Positive(PowerUpDuration) || !Positive(InvulnerabilitySeconds) || !float.IsFinite(PlayerX) || !float.IsFinite(PlayerY) ||
           PlayerX<60 || PlayerX>Width-60 || PlayerY<160 || PlayerY>Height-60 || Levels==null || Levels.Count==0)
            throw new InvalidDataException("Invalid world size, player settings, or levels.");
        foreach(var level in Levels)
        {
            if(level==null || string.IsNullOrWhiteSpace(level.Name) || !Positive(level.Duration) || level.Duration<5 ||
               !Positive(level.SpawnInterval) || level.SpawnInterval<0.25f || !Positive(level.EnemySpeed) || level.EnemySpeed>450 ||
               level.Pattern==null || level.Pattern.Count==0 || level.InitialEnemies==null)
                throw new InvalidDataException("Invalid level settings.");
            foreach(var type in level.Pattern) if(type!="scout" && type!="weaver" && type!="hunter") throw new InvalidDataException("Unknown enemy type.");
            foreach(var e in level.InitialEnemies)
                if(e==null || !float.IsFinite(e.X) || !float.IsFinite(e.Y) || e.X<Width/2 || e.X>Width+200 || e.Y<160 || e.Y>Height-60 ||
                   (e.Type!="scout" && e.Type!="weaver" && e.Type!="hunter")) throw new InvalidDataException("Invalid initial enemy.");
        }
    }
    private static bool Positive(float x)=>float.IsFinite(x)&&x>0;
}
public sealed class LevelConfig
{
    public string Name { get; set; }="";
    public float Duration { get; set; }
    public float SpawnInterval { get; set; }
    public float EnemySpeed { get; set; }
    public List<string> Pattern { get; set; }=new();
    public List<EnemyPlacement> InitialEnemies { get; set; }=new();
}
public sealed class EnemyPlacement
{
    public string Type { get; set; }="scout";
    public float X { get; set; }
    public float Y { get; set; }
}
