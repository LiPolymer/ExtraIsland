using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Shared;
using ExtraIsland.Components;
namespace ExtraIsland.Shared;

public class TimeNodeObservableCollection : ObservableCollection<TimeNode> {
    readonly IExactTimeService _exactTimeService;
    public BetterCountdownConfig? Config;
    TimeNode? LatestNode;
    void SortAll() {
        if (Count <= 1) return;
        List<TimeNode> sortedList = this.OrderBy(_ => _.CountdownTime).ToList();
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
            base.InsertItem(0, tn);
        }
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
    public TimeNodeObservableCollection() {
        _exactTimeService = IAppHost.GetService<IExactTimeService>();
    }
    
    public void GetLatest() {
        if (Items.Count <= 0 || Config is null) {
            return;
        };
        SortAll();
        TimeSpan timeDistance = EiUtils.GetDateTimeSpan(!Config.IsSystemTime ?
            _exactTimeService.GetCurrentLocalDateTime()
            : DateTime.Now, 
            Config.TargetDateTime);
        if (timeDistance < TimeSpan.Zero) {
            Config.LatestNode = null;
            return;
        }
        if (timeDistance >= Items.First().CountdownTime) {
            Config.LatestNode = Items.First();
            return;
        }
        foreach (TimeNode tn in Items) {
            if (timeDistance >= tn.CountdownTime) {
                Config.LatestNode = tn;
                return;
            }
        }
        Config.LatestNode = null;
    }
}