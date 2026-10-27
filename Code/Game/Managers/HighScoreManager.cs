using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
namespace SideScrollerShooter.Game.Managers;
public sealed record ScoreEntry(string Name,int Score,int Level,DateTimeOffset RecordedAt);
public sealed class HighScoreManager
{
    public static string DefaultPath=>Path.Combine(Environment.GetEnvironmentVariable("SIDESCROLLERSHOOTER_DATA")??Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"SideScrollerShooter"),"highscores.json");
    private readonly string _path;
    public List<ScoreEntry> Entries { get; private set; }=new();
    public string Status { get; private set; }="";
    public HighScoreManager(string path=null) { _path=path??DefaultPath; Load(); }
    public void Load()
    {
        try
        {
            if(!File.Exists(_path)) return;
            string text=File.ReadAllText(_path);
            if(int.TryParse(text,out int legacy)) Entries=new() { new("LEGACY",Math.Max(0,legacy),1,DateTimeOffset.UtcNow) };
            else Entries=JsonSerializer.Deserialize<List<ScoreEntry>>(text)??new();
            Entries=Entries.Where(e=>e!=null && !string.IsNullOrWhiteSpace(e.Name) && e.Name.Length<=12 && e.Score>=0 && e.Level>=1).OrderByDescending(e=>e.Score).ThenBy(e=>e.RecordedAt).Take(10).ToList();
        }
        catch(Exception e) when(e is IOException or UnauthorizedAccessException or JsonException)
        { Entries=new(); Status="Saved table could not be read. It has been left untouched."; }
    }
    public bool Add(string name,int score,int level)
    {
        if(score<0 || level<1) throw new ArgumentOutOfRangeException();
        name=string.IsNullOrWhiteSpace(name)?"PILOT":name.Trim().ToUpperInvariant();
        if(name.Length>12) name=name[..12];
        var next=Entries.Append(new ScoreEntry(name,score,level,DateTimeOffset.UtcNow)).OrderByDescending(e=>e.Score).ThenBy(e=>e.RecordedAt).Take(10).ToList();
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_path)));
            string temp=_path+".tmp";
            File.WriteAllText(temp,JsonSerializer.Serialize(next,new JsonSerializerOptions { WriteIndented=true }));
            File.Move(temp,_path,true); Entries=next; Status="Score saved."; return true;
        }
        catch(Exception e) when(e is IOException or UnauthorizedAccessException)
        { Status="Score could not be saved. Check folder permissions."; return false; }
    }
}
