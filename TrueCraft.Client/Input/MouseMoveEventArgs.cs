namespace TrueCraft.Client.Input
{
    /// <summary>
    ///     Provides the event data for mouse movement events.
    /// </summary>
    public readonly struct MouseMoveEventArgs
    {
        public MouseMoveEventArgs(int x, int y, int deltaX, int deltaY)
        {
            X = x;
            Y = y;
            DeltaX = deltaX;
            DeltaY = deltaY;
        }

        public int X { get; }
        public int Y { get; }
        public int DeltaX { get; }
        public int DeltaY { get; }
    }
}
