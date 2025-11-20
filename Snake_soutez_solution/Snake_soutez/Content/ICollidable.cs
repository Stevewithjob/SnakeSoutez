using Microsoft.Xna.Framework;

namespace Snake_soutez
{
    public interface ICollidable
    {
        bool CheckCollision(Point position);
    }
}
