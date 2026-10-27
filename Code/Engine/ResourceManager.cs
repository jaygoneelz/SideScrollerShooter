using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace SideScrollerShooter.Engine
{
    /// <summary>
    /// Caches and provides textures loaded via ContentManager.
    /// </summary>
    public static class ResourceManager
    {
        private static ContentManager _cm;
        private static readonly Dictionary<string, Texture2D> _cache
            = new Dictionary<string, Texture2D>();

        // Call once with Game1.Content
        public static void Initialize(ContentManager cm) => _cm = cm;

        public static Texture2D LoadTexture(string name)
        {
            if (!_cache.ContainsKey(name))
                _cache[name] = _cm.Load<Texture2D>(name);
            return _cache[name];
        }
    }
}
