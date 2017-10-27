using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;
using DynamicData;
using DynamicData.Binding;
using Xamarin.Forms;
using XamarinFormsSuperScroll.Models;
using XamarinFormsSuperScroll.Transfer;

namespace XamarinFormsSuperScroll.ViewModels
{
    public class MainViewModel : AbstractNotifyPropertyChanged
    {
        private readonly DataModel _dataModel;

        private readonly Subject<string> _searchSubject = new Subject<string>();

        private bool _isWorking;
        private int _pageNumber;
        private string _searchText;

        public MainViewModel()
        {
            IsWorking = true;

            _dataModel = DependencyService.Get<DataModel>();
            Shows.OnLoadMore += OnLoadMore;
            Shows.OnError += e => Debug.WriteLine(e.Message);

            var searchPredicate = _searchSubject
                .Throttle(TimeSpan.FromMilliseconds(100))
                .StartWith(string.Empty)
                .Select(SearchPredicate);

            _dataModel.Items.Connect()
                .ObserveOn(Scheduler.Default)
                .Filter(searchPredicate)
                .Sort(SortExpressionComparer<ShowDTO>.Ascending(s => s.Name))
                .Buffer(TimeSpan.FromMilliseconds(200))
                .FlattenBufferResult<ShowDTO, int>()
                .Bind(Shows)
                .Subscribe();

            // Init the list
            _dataModel.GetMoreItems(_pageNumber)
                .ToObservable()
                .Subscribe(s => IsWorking = false);
        }

        public bool IsWorking
        {
            get => _isWorking;
            set => SetAndRaise(ref _isWorking, value);
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
            _pageNumber++;
            await Task.WhenAll(Task.Delay(1000), _dataModel.GetMoreItems(_pageNumber));
            IsWorking = false;
            return new List<ShowDTO>();
        }
    }
}