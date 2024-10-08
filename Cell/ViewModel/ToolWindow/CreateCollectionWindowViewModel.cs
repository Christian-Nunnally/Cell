﻿using Cell.Data;
using Cell.Model.Plugin;
using Cell.Persistence;
using System.Collections.ObjectModel;
using System.Windows;

namespace Cell.ViewModel.ToolWindow
{
    public class CreateCollectionWindowViewModel : ToolWindowViewModel
    {
        private readonly ObservableCollection<UserCollection> _collections;
        private readonly UserCollectionLoader _userCollectionLoader;
        private bool _isBaseOnCheckBoxChecked;
        public CreateCollectionWindowViewModel(UserCollectionLoader userCollectionLoader)
        {
            _userCollectionLoader = userCollectionLoader;
            _collections = _userCollectionLoader.ObservableCollections;
            CollectionBaseOptions = new ObservableCollection<string>(_collections.Select(x => x.Name))
            {
                "---"
            };
            SelectedItemType = PluginTypeNames.FirstOrDefault(string.Empty);
        }

        public override double DefaultHeight => 250;

        public override double DefaultWidth => 350;

        public ObservableCollection<string> CollectionBaseOptions { get; set; }

        public Visibility CollectionBaseSettingVisibility { get; private set; } = Visibility.Collapsed;

        public Visibility CollectionTypeSettingVisibility { get; private set; } = Visibility.Visible;

        public bool IsBaseOnCheckBoxChecked
        {
            get => _isBaseOnCheckBoxChecked; set
            {
                _isBaseOnCheckBoxChecked = value;
                CollectionBaseSettingVisibility = _isBaseOnCheckBoxChecked ? Visibility.Visible : Visibility.Collapsed;
                CollectionTypeSettingVisibility = _isBaseOnCheckBoxChecked ? Visibility.Collapsed : Visibility.Visible;
                NotifyPropertyChanged(nameof(CollectionBaseSettingVisibility));
                NotifyPropertyChanged(nameof(CollectionTypeSettingVisibility));
            }
        }

        public string NewCollectionBaseName { get; set; } = string.Empty;

        public string NewCollectionName { get; set; } = string.Empty;

        public ObservableCollection<string> PluginTypeNames { get; } = new ObservableCollection<string>(PluginModel.GetPluginDataTypeNames());

        public string SelectedItemType { get; set; } = string.Empty;

        public void AddCurrentCollection()
        {
            var collectionName = NewCollectionName;
            if (string.IsNullOrEmpty(collectionName)) return;
            if (_collections.Any(x => x.Name == collectionName)) return;

            if (IsBaseOnCheckBoxChecked)
            {
                var basedOnCollection = NewCollectionBaseName;
                if (string.IsNullOrEmpty(basedOnCollection)) return;

                var baseCollection = _collections.FirstOrDefault(x => x.Name == basedOnCollection);
                if (baseCollection == null) return;

                _userCollectionLoader.CreateCollection(collectionName, baseCollection.Model.ItemTypeName, baseCollection.Name);
            }
            else
            {
                var collectionType = SelectedItemType;
                if (string.IsNullOrEmpty(collectionType)) return;

                _userCollectionLoader.CreateCollection(collectionName, collectionType);
            }
        }
    }
}
