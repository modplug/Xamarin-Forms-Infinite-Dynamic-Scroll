using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using DynamicData.Binding;
using InfiniteScrolling;

namespace XamarinFormsSuperScroll
{
    public class InfiniteDynamicScrollCollection<T> : ObservableCollectionExtended<T>, IInfiniteScrollLoader,
        IInfiniteScrollLoading
    {
        private bool isLoadingMore;

        public InfiniteDynamicScrollCollection()
        {
        }

        public InfiniteDynamicScrollCollection(IEnumerable<T> collection)
            : base(collection)
        {
        }

        public Action OnBeforeLoadMore { get; set; }

        public Action OnAfterLoadMore { get; set; }

        public Action<Exception> OnError { get; set; }

        public Func<bool> OnCanLoadMore { get; set; }

        public Func<Task<IEnumerable<T>>> OnLoadMore { get; set; }

        public virtual bool CanLoadMore => OnCanLoadMore?.Invoke() ?? true;

        public async Task LoadMoreAsync()
        {
            try
            {
                IsLoadingMore = true;
                OnBeforeLoadMore?.Invoke();

                var result = await OnLoadMore();

                if (result != null)
                    AddRange(result);
            }
            catch (Exception ex) when (OnError != null)
            {
                OnError.Invoke(ex);
            }
            finally
            {
                IsLoadingMore = false;
                OnAfterLoadMore?.Invoke();
            }
        }

        public bool IsLoadingMore
        {
            get => isLoadingMore;
            private set
            {
                if (isLoadingMore != value)
                {
                    isLoadingMore = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsLoadingMore)));

                    LoadingMore?.Invoke(this, new LoadingMoreEventArgs(IsLoadingMore));
                }
            }
        }

        public event EventHandler<LoadingMoreEventArgs> LoadingMore;
    }
}