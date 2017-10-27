using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using DynamicData;
using Newtonsoft.Json;
using XamarinFormsSuperScroll.Transfer;

namespace XamarinFormsSuperScroll.Models
{
    public class DataModel
    {
        private readonly HttpClient _client;
        private readonly SourceCache<ShowDTO, int> _internalSourceCache;

        public DataModel()
        {
            _internalSourceCache = new SourceCache<ShowDTO, int>(o => o.Id);
            _client = new HttpClient();
        }

        public IObservableCache<ShowDTO, int> Items => _internalSourceCache;

        public async Task GetMoreItems(int pageNr)
        {
            try
            {
                var result = await _client.GetStringAsync($"http://api.tvmaze.com/shows?page={pageNr}");

                var items = JsonConvert.DeserializeObject<List<ShowDTO>>(result);
                _internalSourceCache.AddOrUpdate(items);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Exception handling by logging :) " + e.Message);
            }
        }
    }
}