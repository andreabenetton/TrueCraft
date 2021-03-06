using System;
using System.Collections.Generic;
using System.Linq;
using TrueCraft.API.Logic;
using TrueCraft.API.Windows;

namespace TrueCraft.Core.Logic
{
    public class CraftingRepository : ICraftingRepository
    {
        private readonly List<ICraftingRecipe> _recipes = new List<ICraftingRecipe>();

        public ICraftingRecipe GetRecipe(IWindowArea craftingArea)
        {
            foreach (var r in _recipes)
                if (MatchRecipe(r, craftingArea))
                    return r;
            return null;
        }

        public bool TestRecipe(IWindowArea craftingArea, ICraftingRecipe recipe, int x, int y)
        {
            if (x + recipe.Pattern.GetLength(1) > craftingArea.Width ||
                y + recipe.Pattern.GetLength(0) > craftingArea.Height)
                return false;
            for (var _x = 0; _x < recipe.Pattern.GetLength(1); _x++)
            for (var _y = 0; _y < recipe.Pattern.GetLength(0); _y++)
            {
                var supplied = craftingArea[(y + _y) * craftingArea.Width + x + _x];
                var required = recipe.Pattern[_y, _x];
                if (supplied.ID != required.ID || supplied.Count < required.Count ||
                    recipe.SignificantMetadata && required.Metadata != supplied.Metadata)
                    return false;
            }

            return true;
        }

        public void RegisterRecipe(ICraftingRecipe recipe)
        {
            _recipes.Add(recipe);
        }

        public void DiscoverRecipes()
        {
            var recipeTypes = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            foreach (var type in assembly.GetTypes().Where(t =>
                typeof(ICraftingRecipe).IsAssignableFrom(t) && !t.IsAbstract))
                recipeTypes.Add(type);

            recipeTypes.ForEach(t =>
            {
                var instance = (ICraftingRecipe) Activator.CreateInstance(t);
                RegisterRecipe(instance);
            });
        }

        private bool MatchRecipe(ICraftingRecipe recipe, IWindowArea craftingArea)
        {
            for (var x = 0; x < craftingArea.Width; x++)
            for (var y = 0; y < craftingArea.Height; y++)
                if (TestRecipe(craftingArea, recipe, x, y))
                {
                    // Check to make sure there aren't any sneaky unused items in the grid
                    int minX = x, maxX = x + recipe.Pattern.GetLength(1);
                    int minY = y, maxY = y + recipe.Pattern.GetLength(0);
                    for (var _x = 0; _x < craftingArea.Width; _x++)
                    for (var _y = 0; _y < craftingArea.Height; _y++)
                        if (_x < minX || _x >= maxX || _y < minY || _y >= maxY)
                            if (!craftingArea[_y * craftingArea.Width + _x].Empty)
                                return false;
                    return true;
                }

            return false;
        }
    }
}