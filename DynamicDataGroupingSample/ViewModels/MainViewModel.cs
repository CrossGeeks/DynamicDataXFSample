using System;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using DynamicData;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace DynamicDataGroupingSample
{
    public class MainViewModel : IDisposable
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

            _cleanUp = _sourceCache.Connect()
                        .RefCount()
                        .Bind(out _restaurants)
                        .DisposeMany()
                        .Subscribe();

            AddCommand = new Command(async() => await ExecuteAdd());
            DeleteCommand = new Command<Restaurant>(ExecuteRemove);
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

        public void Dispose()
        {
            _cleanUp.Dispose();
        }

        public ReadOnlyObservableCollection<Restaurant> Restaurants => _restaurants;

        public ICommand AddCommand { get; set; }
        public ICommand DeleteCommand { get; }

        private SourceCache<Restaurant, string> _sourceCache = new SourceCache<Restaurant, string>(x => x.Id);
        private readonly ReadOnlyObservableCollection<Restaurant> _restaurants;

        private readonly IDisposable _cleanUp;
    }
}
