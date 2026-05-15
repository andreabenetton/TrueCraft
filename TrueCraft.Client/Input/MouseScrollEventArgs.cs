namespace TrueCraft.Client.Input
{
    /// <summary>
    ///     Provides the event data for mouse scroll events.
    /// </summary>
    public readonly struct MouseScrollEventArgs
    {
        public MouseScrollEventArgs(int x, int y, int value, int deltaValue)
        {
            X = x;
            Y = y;
            Value = value;
            DeltaValue = deltaValue;
        }

        public int X { get; }
        public int Y { get; }
        public int Value { get; }
        public int DeltaValue { get; }
    }
}
