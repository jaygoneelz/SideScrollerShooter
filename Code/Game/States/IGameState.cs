using Microsoft.Xna.Framework.Graphics;

namespace SideScrollerShooter.Game.States
{
    /// <summary>
    /// Interface for game states in the FSM.
    /// </summary>
    public interface IGameState
    {
        void Enter();
        void Exit();
        void Update(float dt);
        void Draw(SpriteBatch sb);
    }
}
