using System.Collections.Generic;
using System.Windows.Controls;

namespace BusStationTicketSystem.Patterns.Composite
{
    public interface IMenuComponent
    {
        string Name { get; }
        void Add(IMenuComponent component);
        void Remove(IMenuComponent component);
        IMenuComponent? GetChild(int index);
        IEnumerable<IMenuComponent> GetChildren();
        void Render(MenuItem parentMenuItem);
        MenuItem CreateMenuItem();
    }
}
