using System.Collections.Generic;
using Roguelike.Extensions;

namespace Roguelike.Services
{
    public class GameBoardService
    {
        public int Columns;
        public int Rows;
        public ICollection<int>[,] Grid;
        
        public (bool, ICollection<int>) IsGameBoardPositionOpen(int x, int y)
        {
            bool edge = x == -1 || x == Columns || y == -1 || y == Rows;
            if (edge)
            {
                return (false, null);
            }
            var entities = Grid[x, y];
            return (entities == null || entities.Empty(), entities);
        }

        public void ReplaceGameBoard(int columns, int rows)
        {
            Columns = columns;
            Rows = rows;
            Grid = new ICollection<int>[columns,rows];
        }
    }
}