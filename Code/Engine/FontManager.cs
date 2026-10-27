using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace SideScrollerShooter.Engine
{
    public static class FontManager
    {
        static ContentManager _cm;
        static Dictionary<string, SpriteFont> _fonts = new();
        public static void Initialize(ContentManager cm) => _cm = cm;
        public static SpriteFont LoadFont(string name)
        {
            if (!_fonts.ContainsKey(name))
                _fonts[name] = _cm.Load<SpriteFont>(name);
            return _fonts[name];
        }
    }
}
