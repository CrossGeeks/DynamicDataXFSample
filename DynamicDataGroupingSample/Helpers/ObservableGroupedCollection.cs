using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;

namespace DynamicDataGroupingSample.Helpers
{
    public class ObservableGroupedCollection<TGroupKey, TObject, TKey> : ObservableCollectionExtended<TObject>, IDisposable
    {
        public TGroupKey Key { get; }

        public ObservableGroupedCollection(IGroup<TObject, TKey, TGroupKey> group, IObservable<Func<TObject, bool>> filter, IObservable<IComparer<TObject>> comparer)
        {
            this.Key = group.Key;

            //load and sort the grouped list
            var dataLoader = group.Cache.Connect()
                .Filter(filter)
                .Sort(comparer)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Bind(this) //make the reset threshold large because xamarin is slow when reset is called (or at least I think it is @erlend, please enlighten me )
                .Subscribe();

            _cleanUp = new CompositeDisposable(dataLoader);
        }

        public void Dispose()
        {
            _cleanUp.Dispose();
        }

        private readonly IDisposable _cleanUp;
    }
}