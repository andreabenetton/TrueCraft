using System;
using System.Collections.Generic;
using System.Linq;
using TrueCraft.API.Logic;

namespace TrueCraft.Core.Logic
{
    public class ItemRepository : IItemRepository
    {
        private readonly List<IItemProvider> _itemProviders;

        public ItemRepository()
        {
            _itemProviders = new List<IItemProvider>();
        }

        public IItemProvider GetItemProvider(short id)
        {
            // TODO: Binary search
            foreach (var ip in _itemProviders)
                if (ip.ID == id)
                    return ip;

            return null;
        }

        public void RegisterItemProvider(IItemProvider provider)
        {
            int i;
            for (i = _itemProviders.Count - 1; i >= 0; i--)
            {
                if (provider.ID == _itemProviders[i].ID)
                {
                    _itemProviders[i] = provider; // Override
                    return;
                }

                if (_itemProviders[i].ID < provider.ID)
                    break;
            }

            _itemProviders.Insert(i + 1, provider);
        }

        public void DiscoverItemProviders()
        {
            var providerTypes = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            foreach (var type in assembly.GetTypes().Where(t =>
                typeof(IItemProvider).IsAssignableFrom(t) && !t.IsAbstract))
                providerTypes.Add(type);

            providerTypes.ForEach(t =>
            {
                var instance = (IItemProvider) Activator.CreateInstance(t);
                RegisterItemProvider(instance);
            });
        }
    }
}