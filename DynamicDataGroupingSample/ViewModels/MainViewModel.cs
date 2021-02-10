using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using DynamicData;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReactiveUI;
using System.Reactive;
using DynamicData.Binding;
using DynamicDataGroupingSample.Helpers;

namespace DynamicDataGroupingSample
{
    public class MainViewModel : ReactiveObject, IDisposable
    {
        public MainViewModel()
        {
            // Initial data
            _sourceCache.AddOrUpdate(new List<Restaurant>()
            {
                new Restaurant("Yao","Casual","Asian Fusion","Dominican Republic"),
                new Restaurant("Chef Pepper","Casual","International","Dominican Republic"),
                new Restaurant("Bottega Fratelli","Formal","International","Dominican Republic"),
                new Restaurant("McDonalds","Fast Food","Burgers","United States"),
                new Restaurant("Burger King","Fast Food","Burgers","United States"),
                new Restaurant("Sushi Nation","Casual","Sushi","Venezuela"),
                new Restaurant("Pollo Victorina","Fast Food","Chicken","Dominican Republic"),
                new Restaurant("Pollo Rey","Fast Food","Chicken","Dominican Republic"),
                new Restaurant("Asadero Argetino","Formal","Meat","Dominican Republic"),
                new Restaurant("Hooters","Casual","Wings","United States"),
                new Restaurant("Andres Carne","Casual","Meat","Colombia"),
                new Restaurant("La Casita","Casual","Colombian Food","Colombia"),
                new Restaurant("Cielo","Formal","International","Colombia"),
            });

            //Search logic
            Func<Restaurant, bool> restaurantFilter(string text) => restaurant =>
            {
                return string.IsNullOrEmpty(text) || restaurant.Name.ToLower().Contains(text.ToLower()) || restaurant.Type.ToLower().Contains(text.ToLower());
            };

            var filterPredicate = this.WhenAnyValue(x => x.SearchText)
                                      .Throttle(TimeSpan.FromMilliseconds(250), RxApp.TaskpoolScheduler)
                                      .DistinctUntilChanged()
                                      .Select(restaurantFilter);

            //Filter logic
            Func<Restaurant, bool> countryFilter(string country) => restaurant =>
            {
                return country == "All" || country == restaurant.Country;
            };

            var countryPredicate = this.WhenAnyValue(x => x.SelectedCountryFilter)
                                       .Select(countryFilter);

            //sort
            var sortPredicate = this.WhenAnyValue(x => x.SortBy)
                                    .Select(x => x == "Type" ? SortExpressionComparer<Restaurant>.Ascending(a => a.Type) : SortExpressionComparer<Restaurant>.Ascending(a => a.Name));

            _cleanUp = _sourceCache.Connect()
            .Filter(countryPredicate)
            .Group(x => x.Category)
            .Transform(g => new ObservableGroupedCollection<string, Restaurant, string>(g, filterPredicate, sortPredicate))
            .Sort(SortExpressionComparer<ObservableGroupedCollection<string, Restaurant, string>>.Ascending(a => a.Key))
            .Bind(out _restaurants)
            .DisposeMany()
            .Subscribe();

            //Set default values
            SelectedCountryFilter = "All";
            SortBy = "Name";

            AddCommand = ReactiveCommand.CreateFromTask(async() => await ExecuteAdd());
            DeleteCommand = ReactiveCommand.Create<Restaurant>(ExecuteRemove);
            SortCommand = ReactiveCommand.CreateFromTask(ExecuteSort);
        }

        private void ExecuteRemove(Restaurant restaurant)
        {
            _sourceCache.Edit((update) =>
            {
                update.Remove(restaurant);
            });
         }

        private async Task ExecuteAdd()
        {
            var name = await App.Current.MainPage.DisplayPromptAsync("Insert Name", "");
            _sourceCache.Edit((update) =>
            {
                update.AddOrUpdate(new Restaurant(name, "Casual", "Fast Food", "US"));
            });
        }

        private async Task ExecuteSort()
        {
            var sort = await App.Current.MainPage.DisplayActionSheet("Sort by", "Cancel", null, buttons: new string[] { "Name", "Type" });
            if (sort != "Cancel")
            {
                SortBy = sort;
            }
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }

        public ReadOnlyObservableCollection<ObservableGroupedCollection<string, Restaurant, string>> RestaurantsGrouped => _restaurants;

        public string SearchText
        {
            get => _searchText;
            set => this.RaiseAndSetIfChanged(ref _searchText, value);
        }

        public string SelectedCountryFilter
        {
            get => _selectedCountryFilter;
            set => this.RaiseAndSetIfChanged(ref _selectedCountryFilter, value);
        }

        private string SortBy
        {
            get => _sortBy;
            set => this.RaiseAndSetIfChanged(ref _sortBy, value);
        }

        public ReactiveCommand<Unit, Unit> AddCommand { get; set; }
        public ReactiveCommand<Restaurant, Unit> DeleteCommand { get; }
        public ReactiveCommand<Unit, Unit> SortCommand { get; }

        private SourceCache<Restaurant, string> _sourceCache = new SourceCache<Restaurant, string>(x => x.Id);
        private readonly ReadOnlyObservableCollection<ObservableGroupedCollection<string, Restaurant, string>> _restaurants;
        private string _searchText;
        private string _selectedCountryFilter;
        private string _sortBy;

        private readonly IDisposable _cleanUp;
    }
}
