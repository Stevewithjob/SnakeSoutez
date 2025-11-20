namespace Snake_soutez
{
    public class GameState
    {
        public bool IsGameOver { get; set; }
        public string Winner { get; set; }

        public GameState()
        {
            IsGameOver = false;
            Winner = "";
        }

        public void SetWinner(string winnerText)
        {
            IsGameOver = true;
            Winner = winnerText;
        }

        public void Reset()
        {
            IsGameOver = false;
            Winner = "";
        }
    }
}
