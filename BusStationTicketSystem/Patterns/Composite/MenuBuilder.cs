using System;
using System.Collections.Generic;
using System.Windows.Controls;
using BusStationTicketSystem.Patterns.Composite;

namespace BusStationTicketSystem.Patterns.Composite
{
    public class MenuBuilder
    {
        private MenuItemComposite? _rootMenu;
        private MenuItemComposite? _currentMenu;

        public MenuBuilder CreateMenu(string name)
        {
            _rootMenu = new MenuItemComposite(name);
            _currentMenu = _rootMenu;
            return this;
        }

        public MenuBuilder AddSubMenu(string name)
        {
            if (_currentMenu == null)
                throw new InvalidOperationException("Спочатку створіть головне меню");

            var subMenu = new MenuItemComposite(name);
            _currentMenu.Add(subMenu);
            _currentMenu = subMenu;
            return this;
        }

        public MenuBuilder AddMenuItem(string name, Action? action = null)
        {
            if (_currentMenu == null)
                throw new InvalidOperationException("Спочатку створіть головне меню");

            var menuItem = new MenuItemLeaf(name, action);
            _currentMenu.Add(menuItem);
            return this;
        }

        public MenuBuilder AddSeparator()
        {
            if (_currentMenu == null)
                throw new InvalidOperationException("Спочатку створіть головне меню");

            var separator = new MenuItemSeparator();
            _currentMenu.Add(separator);
            return this;
        }

        public MenuBuilder EndSubMenu()
        {
            if (_rootMenu != null)
            {
                _currentMenu = _rootMenu;
            }
            return this;
        }

        public MenuItemComposite Build()
        {
            if (_rootMenu == null)
                throw new InvalidOperationException("Меню не створено");

            return _rootMenu;
        }

        public void RenderToMenuBar(Menu menuBar)
        {
            if (_rootMenu == null)
                throw new InvalidOperationException("Меню не створено");

            try
            {
                menuBar.Items.Clear();
                
                foreach (var child in _rootMenu.GetChildren())
                {
                    if (child is MenuItemComposite composite)
                    {
                        var menuItem = composite.CreateMenuItem();
                        if (menuItem != null)
                            menuBar.Items.Add(menuItem);
                    }
                    else if (child is MenuItemLeaf leaf)
                    {
                        var menuItem = leaf.CreateMenuItem();
                        if (menuItem != null)
                            menuBar.Items.Add(menuItem);
                    }
                    else if (child is MenuItemSeparator)
                    {
                        menuBar.Items.Add(new Separator());
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Помилка рендерингу меню: {ex.Message}");
                // Продовжуємо роботу навіть якщо меню не вдалося створити
            }
        }
    }

    public class MenuItemSeparator : IMenuComponent
    {
        public string Name => "Separator";

        public void Add(IMenuComponent component)
        {
            throw new NotSupportedException("Separator не може містити дочірні елементи");
        }

        public void Remove(IMenuComponent component)
        {
            throw new NotSupportedException("Separator не може містити дочірні елементи");
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
            parentMenuItem.Items.Add(new Separator());
        }

        public MenuItem CreateMenuItem()
        {
            throw new NotSupportedException("Separator не може бути створений як MenuItem");
        }
    }
}

