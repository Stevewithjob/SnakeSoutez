namespace Snake_soutez
{
    public interface IMovable
    {
        void Move();
        Direction CurrentDirection { get; set; }
    }
}