namespace Roguelike.Features.Components
{
    internal struct PositionComponent
    {
        public int X;
        public int Y;

        public override bool Equals(object obj)
        {
            var other = obj as PositionComponent? ?? default;
            return X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
        {
            int hash = 13;
            hash = (hash * 7) + X.GetHashCode();
            hash = (hash * 7) + Y.GetHashCode();
            return hash;
        }
        
        public void SetPositions(int newX, int newY)
        {
            X = newX;
            Y = newY;
        }
    }
}
