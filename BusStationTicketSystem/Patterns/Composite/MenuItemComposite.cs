using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;

namespace BusStationTicketSystem.Patterns.Composite
{
    public class MenuItemComposite : IMenuComponent
    {
        public string Name { get; }
        private readonly List<IMenuComponent> _children = new List<IMenuComponent>();

        public MenuItemComposite(string name)
        {
            Name = name;
        }

        public void Add(IMenuComponent component)
        {
            _children.Add(component);
        }

        public void Remove(IMenuComponent component)
        {
            _children.Remove(component);
        }

        public IMenuComponent? GetChild(int index)
        {
            if (index >= 0 && index < _children.Count)
                return _children[index];
            return null;
        }

        public IEnumerable<IMenuComponent> GetChildren()
        {
            return _children;
        }

        public void Render(MenuItem parentMenuItem)
        {
            var menuItem = CreateMenuItem();
            parentMenuItem.Items.Add(menuItem);
        }

        public MenuItem CreateMenuItem()
        {
            var menuItem = new MenuItem
            {
                Header = Name
            };

            foreach (var child in _children)
            {
                child.Render(menuItem);
            }

            return menuItem;
        }
    }
}



