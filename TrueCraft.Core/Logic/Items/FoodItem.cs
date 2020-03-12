namespace TrueCraft.Core.Logic.Items
{
    public abstract class FoodItem : ItemProvider
    {
        /// <summary>
        ///     The amount of health this food restores.
        /// </summary>
        public abstract float Restores { get; }

        //Most foods aren't stackable
        public override sbyte MaximumStack => 1;
    }
}