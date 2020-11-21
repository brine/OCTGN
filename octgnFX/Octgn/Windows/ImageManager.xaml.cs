// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using Octgn.Annotations;
using Octgn.Core;
using Octgn.Library;
using Octgn.Library.Exceptions;
using Octgn.ViewModels;

namespace Octgn.Windows
{
    public partial class ImageManager : INotifyPropertyChanged, IDisposable
    {
        public ObservableCollection<GameImagesViewModel> Games { get; }
        public GameImagesViewModel selectedGame;
        public SetImagesViewModel selectedSet;
        internal bool Disposed;
        public FileSystemWatcher Watcher { get; set; }
        public ImageManager() {

            Games = new ObservableCollection<GameImagesViewModel>();

            this.MinMaxButtonVisibility = Visibility.Collapsed;
            this.MinimizeButtonVisibility = Visibility.Collapsed;

            this.ResizeMode = ResizeMode.CanMinimize;
            this.Loaded += OnLoaded;

            Watcher = new FileSystemWatcher(Config.Instance.ImageDirectoryFull);
            Watcher.Changed += WatcherOnChanged;
            Watcher.Deleted += WatcherOnDeleted;
            Watcher.Renamed += WatcherOnRenamed;

            InitializeComponent();
        }

        private void OnLoaded(object sender, RoutedEventArgs args)
        {
            this.Loaded -= OnLoaded;
            Task.Run(() => LoadImageItems());
        }

        internal void LoadImageItems()
        {
            Watcher.EnableRaisingEvents = true;
            var imageDirectory = new DirectoryInfo(Config.Instance.ImageDirectoryFull);
            var gamesList = imageDirectory.GetDirectories().Select(x => new GameImagesViewModel(x));
            Dispatcher.Invoke(new Action(() =>
            {
                Games.Clear();
                foreach (var game in gamesList)
                {
                    Games.Add(game);
                    game.Sets.Clear();
                    var setsList = new DirectoryInfo(Path.Combine(game.Directory.FullName, "Sets")).GetDirectories().Select(x => new SetImagesViewModel(x, game));
                    foreach (var set in setsList)
                    {
                        game.Sets.Add(set);
                        set.Cards.Clear();
                        var cardsList = new DirectoryInfo(Path.Combine(set.Directory.FullName, "Cards")).GetFiles().Select(x => new CardImagesViewModel(x, set));
                        foreach (var card in cardsList)
                        {
                            set.Cards.Add(card);
                        }
                    }
                }
            }));
        }

        private void WatcherOnRenamed(object sender, RenamedEventArgs renamedEventArgs)
        {
        }

        private void WatcherOnDeleted(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
        }

        private void WatcherOnChanged(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
        }
        public new void Dispose()
        {
            if (Disposed) return;
            Watcher.EnableRaisingEvents = false;
            Watcher.Changed -= this.WatcherOnChanged;
            Watcher.Deleted -= this.WatcherOnDeleted;
            Watcher.Renamed -= this.WatcherOnRenamed;
            Disposed = true;
            Watcher.Dispose();
            base.Dispose();
        }

        public GameImagesViewModel SelectedGame
        {
            get { return this.selectedGame; }
            set
            {
                if (Equals(value, this.selectedGame))
                {
                    return;
                }
                this.selectedGame = value;
                this.OnPropertyChanged(nameof(IsGameSelected));
                this.OnPropertyChanged(nameof(SelectedGame));
            }
        }
        public bool IsGameSelected => ListBoxGames.SelectedIndex > -1;
        public SetImagesViewModel SelectedSet
        {
            get { return this.selectedSet; }
            set
            {
                if (Equals(value, this.selectedSet))
                {
                    return;
                }
                this.selectedSet = value;
                this.OnPropertyChanged(nameof(IsSetSelected));
                this.OnPropertyChanged(nameof(SelectedSet));
            }
        }
        public bool IsSetSelected => ListBoxSets.SelectedIndex > -1;

        private void ButtonCancelClick(object sender, RoutedEventArgs e) {
            this.Close();
        }

        public new event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
