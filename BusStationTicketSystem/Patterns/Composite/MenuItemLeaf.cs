using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace BusStationTicketSystem.Patterns.Composite
{
    public class MenuItemLeaf : IMenuComponent
    {
        public string Name { get; }
        private readonly Action? _action;

        public MenuItemLeaf(string name, Action? action = null)
        {
            Name = name;
            _action = action;
        }

        public void Add(IMenuComponent component)
        {
            throw new NotSupportedException("Leaf не може містити дочірні елементи");
        }

        public void Remove(IMenuComponent component)
        {
            throw new NotSupportedException("Leaf не може містити дочірні елементи");
        }

        public IMenuComponent? GetChild(int index)
        {
            return null;
        }

        public IEnumerable<IMenuComponent> GetChildren()
        {
            return new List<IMenuComponent>();
        }

        public void Render(MenuItem parentMenuItem)
        {
            var menuItem = CreateMenuItem();
            if (menuItem != null)
                parentMenuItem.Items.Add(menuItem);
        }

        public MenuItem CreateMenuItem()
        {
            var menuItem = new MenuItem
            {
                Header = Name
            };

            if (_action != null)
            {
                menuItem.Click += (s, e) => _action();
            }

            return menuItem;
        }
    }
}
