using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Shared;
using ExtraIsland.Components;
namespace ExtraIsland.Shared;

public class TimeNodeObservableCollection : ObservableCollection<TimeNode> {
    readonly IExactTimeService _exactTimeService;
    
    public void SortAll() {
        if (Count <= 1) return;
        var sortedList = this.OrderBy(_ => _.CountdownTime).ToList();
        bool needSort = false;
        for (int i = 0; i < Count; i++) {
            if (!ReferenceEquals(this[i], sortedList[i]))
            {
                needSort = true;
                break;
            }
        }
        if (!needSort) return;
        base.ClearItems();
        foreach (TimeNode tn in sortedList) {
            base.InsertItem(Count, tn);
        }
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
    public TimeNodeObservableCollection() {
        _exactTimeService = IAppHost.GetService<IExactTimeService>();
    }
    
    public TimeNode? GetLatest(BetterCountdownConfig config) {
        foreach (TimeNode tn in Items) {
            if((config.TargetDateTime -
               (!config.IsSystemTime ? 
                   _exactTimeService.GetCurrentLocalDateTime()
                   : DateTime.Now)) <= tn.CountdownTime) return Items[IndexOf(tn)-1];
        }
        return null;
    }
}