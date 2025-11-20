using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Snake_soutez
{
    public interface IDrawable
    {
        void Draw(SpriteBatch spriteBatch, Texture2D texture, int cellSize);
    }
}
