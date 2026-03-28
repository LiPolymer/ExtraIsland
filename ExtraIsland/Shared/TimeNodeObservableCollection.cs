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
    void SortAll() {
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
            base.InsertItem(0, tn);
        }
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }
    public TimeNodeObservableCollection() {
        _exactTimeService = IAppHost.GetService<IExactTimeService>();
        Items.Add(new TimeNode());
    }
    
    public void GetLatest() {
        if (Items.Count <= 0 || Config is null) {
            Console.WriteLine("quit check");
            return;
        };
        SortAll();
        TimeSpan timeDistance = EiUtils.GetDateTimeSpan(!Config.IsSystemTime ?
            _exactTimeService.GetCurrentLocalDateTime()
            : DateTime.Now, 
            Config.TargetDateTime);
        if (timeDistance < TimeSpan.Zero) {
            Config.LatestNode = null;
            Console.WriteLine("Quit below 0");
            return;
        }
        Console.WriteLine(("In GL"));
        if (timeDistance >= Items.First().CountdownTime) {
            Config.LatestNode = Items.First();
            Console.WriteLine("Return Last Node:" + Config.LatestNode);
            return;
        }
        Console.WriteLine("TimeDistance" + timeDistance);
        foreach (TimeNode tn in Items) {
            Console.WriteLine("Now:"+tn);
            if (timeDistance >= tn.CountdownTime) {
                Config.LatestNode = tn;
                Console.WriteLine("Return Node in for:" + Config.LatestNode);
                return;
            }
        }
        Config.LatestNode = null;
    }
    
}