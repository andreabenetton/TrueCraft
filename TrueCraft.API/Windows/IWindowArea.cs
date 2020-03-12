using System;

namespace TrueCraft.API.Windows
{
    public interface IWindowArea : IDisposable
    {
        int StartIndex { get; set; }
        int Length { get; set; }
        int Width { get; }
        int Height { get; }
        ItemStack[] Items { get; set; }

        ItemStack this[int index] { get; set; }
        event EventHandler<WindowChangeEventArgs> WindowChange;

        void CopyTo(IWindowArea area);
        int MoveOrMergeItem(int index, ItemStack item, IWindowArea from);
    }
}