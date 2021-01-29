// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using Octgn.Annotations;
using Octgn.Core;
using Octgn.Library;
using Octgn.Library.Exceptions;
using Octgn.ViewModels;

namespace Octgn.Windows
{
    public partial class ImageManager : INotifyPropertyChanged, IDisposable
    {
        internal bool Disposed;

        public FileSystemWatcher Watcher { get; set; }
        public ImageManager()
        {
            Watcher = new FileSystemWatcher(Config.Instance.ImageDirectoryFull);
            Watcher.Changed += WatcherOnChanged;
            Watcher.Deleted += WatcherOnDeleted;
            Watcher.Renamed += WatcherOnRenamed;
            Watcher.EnableRaisingEvents = true;

            InitializeComponent();
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

        private void ButtonCancelClick(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void GameIcon_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            ((Image)sender).Source = new BitmapImage(new Uri("pack://application:,,,/OCTGN;component/Resources/noimage.png", UriKind.Absolute));
        }

    }
}
