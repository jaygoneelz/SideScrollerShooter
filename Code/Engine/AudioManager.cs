using System;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
namespace SideScrollerShooter.Engine;
public static class AudioManager
{
    private static SoundEffect _shoot,_hit;
    private static SoundEffectInstance _music;
    public static bool Muted { get; private set; }
    public static string Status { get; private set; }="";
    public static void Initialize(ContentManager content)
    {
        try
        {
            _shoot=content.Load<SoundEffect>("Audio/Laser_shoot_sound");
            _hit=content.Load<SoundEffect>("Audio/damage_sound");
            _music=content.Load<SoundEffect>("Audio/orbit_theme").CreateInstance();
            _music.IsLooped=true; _music.Volume=0.18f; _music.Play();
        }
        catch(Exception e) when(e is NoAudioHardwareException or InstancePlayLimitException)
        { Status="Audio unavailable on this device."; }
    }
    public static void ToggleMute()
    {
        Muted=!Muted;
        if(_music!=null) _music.Volume=Muted?0:0.18f;
    }
    public static void PlayShoot() { if(!Muted) _shoot?.Play(0.2f,0,0); }
    public static void PlayHit() { if(!Muted) _hit?.Play(0.3f,0,0); }
    public static void Dispose() { _music?.Stop(); _music?.Dispose(); }
}
