using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Aggregation;
using DynamicData.Binding;
using Xamarin.Forms;
using XamarinFormsSuperScroll.Models;
using XamarinFormsSuperScroll.Transfer;

namespace XamarinFormsSuperScroll.ViewModels
{
    public class VirtualizingListViewModel : AbstractNotifyPropertyChanged
    {
        private readonly Subject<string> _searchSubject = new Subject<string>();
        private readonly Subject<Unit> _virtualiseSubject = new Subject<Unit>();

        private bool _isWorking;
        private int _previousSize;
        private string _searchText;

        public VirtualizingListViewModel()
        {
            IsWorking = true;

            var dataModel = DependencyService.Get<DataModel>();
            Shows.OnLoadMore += OnLoadMore;
            Shows.OnError += e => Debug.WriteLine(e.Message);

            var searchPredicate = _searchSubject
                .Throttle(TimeSpan.FromMilliseconds(100)) // Wait 100 ms after last keyboard press before searching
                .StartWith(string.Empty)
                .Select(SearchPredicate);

            var filteredList = dataModel.Items.Connect()
                .Filter(searchPredicate)
                .Publish();

            var virtualisePredicate = _virtualiseSubject
                .StartWith(new List<Unit>())
                .CombineLatest(filteredList.Count().StartWith(0), (request, count) =>
                {
                    var items = count <= 0 ? 0 : count - 1;
                    var size = _previousSize > items ? items : Math.Min(_previousSize + 20, items);
                    _previousSize = size;
                    return new VirtualRequest(0, size);
                })
                .DistinctUntilChanged();

            filteredList
                .Sort(SortExpressionComparer<ShowDTO>.Ascending(s => s.Name))
                .Virtualise(virtualisePredicate)
                .ObserveOn(Scheduler.Default)
                .Bind(Shows)
                .Subscribe();

            filteredList.Connect();

            // Init the list
            dataModel.GetMoreItems(0)
                .ToObservable()
                .Subscribe(s => IsWorking = false);

            _searchSubject.DistinctUntilChanged()
                .Do(s => _previousSize = 0)
                .Subscribe();
        }

        public bool IsWorking
        {
            get => _isWorking;
            private set => SetAndRaise(ref _isWorking, value);
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                SetAndRaise(ref _searchText, value);
                _searchSubject.OnNext(value);
            }
        }

        public InfiniteDynamicScrollCollection<ShowDTO> Shows { get; } = new InfiniteDynamicScrollCollection<ShowDTO>();

        private Func<ShowDTO, bool> SearchPredicate(string searchText)
        {
            return s => string.IsNullOrEmpty(s.Name) ||
                        s.Name.ToLowerInvariant().Contains(searchText.ToLowerInvariant());
        }

        private async Task<IEnumerable<ShowDTO>> OnLoadMore()
        {
            IsWorking = true;
            _virtualiseSubject.OnNext(Unit.Default);
            IsWorking = false;
            return new List<ShowDTO>();
        }
    }
}