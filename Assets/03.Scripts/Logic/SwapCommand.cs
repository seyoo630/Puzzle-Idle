public class SwapCommand
{
    public Tile tileA;
    public Tile tileB;
    public void Execute(Board board) => board.Swap(tileA, tileB);
}