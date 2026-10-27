using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace SideScrollerShooter.Engine;
public static class Ui
{
    public static Texture2D Pixel { get; private set; }
    private static Texture2D _thruster;
    private static Texture2D _background;
    public static SpriteFont Font { get; private set; }
    public static readonly Color Accent=new(84,225,244);
    private static float _scroll;
    public static void Initialize(GraphicsDevice device)
    {
        Pixel=new Texture2D(device,1,1); Pixel.SetData(new[]{Color.White});
        Font=FontManager.LoadFont("Fonts/DefaultFont");
        _background=ResourceManager.LoadTexture("Textures/spaceBackground");
        // Four distinct 32x20 flame frames in one runtime-generated sprite sheet.
        _thruster=new Texture2D(device,128,20);
        var data=new Color[128*20];
        for(int f=0;f<4;f++) for(int x=0;x<32;x++) for(int y=0;y<20;y++)
        {
            float length=18+f*4;
            if(x>32-length && Math.Abs(y-10)<(x-(32-length))*0.3f)
                data[y*128+f*32+x]=Math.Abs(y-10)<3?Color.LightCyan:Color.DeepSkyBlue;
        }
        _thruster.SetData(data);
    }
    public static void Update(float dt)=>_scroll+=dt*22;
    public static void Background(SpriteBatch sb)
    {
        int w=_background.Width,h=_background.Height;
        for(float x=-(_scroll%w);x<Game1.ScreenWidth;x+=w)
            for(int y=0;y<Game1.ScreenHeight;y+=h) sb.Draw(_background,new Vector2(x,y),Color.White*0.4f);
    }
    public static void DrawThruster(SpriteBatch sb,Vector2 position,int frame)
        =>sb.Draw(_thruster,position,new Rectangle(frame*32,0,32,20),Color.White,0,new Vector2(16,10),1,SpriteEffects.None,0);
    public static void Panel(SpriteBatch sb,Rectangle area) { sb.Draw(Pixel,area,new Color(6,16,34)*0.95f); Outline(sb,area,new Color(35,72,95)); }
    public static void Outline(SpriteBatch sb,Rectangle r,Color color,int thickness=1)
    {
        sb.Draw(Pixel,new Rectangle(r.X,r.Y,r.Width,thickness),color);
        sb.Draw(Pixel,new Rectangle(r.X,r.Bottom-thickness,r.Width,thickness),color);
        sb.Draw(Pixel,new Rectangle(r.X,r.Y,thickness,r.Height),color);
        sb.Draw(Pixel,new Rectangle(r.Right-thickness,r.Y,thickness,r.Height),color);
    }
    public static void Text(SpriteBatch sb,string text,float x,float y,Color? color=null,float scale=1)
        =>sb.DrawString(Font,text,new Vector2(x,y),color??Color.White,0,Vector2.Zero,scale,SpriteEffects.None,0);
    public static void Centre(SpriteBatch sb,string text,float y,Color? color=null,float scale=1)
    { float width=Font.MeasureString(text).X*scale; Text(sb,text,(Game1.ScreenWidth-width)/2,y,color,scale); }
    public static void Dispose() { Pixel?.Dispose(); _thruster?.Dispose(); }
}
