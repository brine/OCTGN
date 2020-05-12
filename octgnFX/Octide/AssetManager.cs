﻿// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace Octide
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;

    using GalaSoft.MvvmLight.Messaging;

    using Microsoft.Win32;
    using Octgn.DataNew.Entities;

    using Octide.ViewModel;

    public class AssetManager
    {
        #region Singleton

        internal static AssetManager SingletonContext { get; set; }

        private static readonly object AssetManagerSingletonLocker = new object();

        public static AssetManager Instance
        {
            get
            {
                if (SingletonContext == null)
                {
                    lock (AssetManagerSingletonLocker)
                    {
                        if (SingletonContext == null)
                        {
                            SingletonContext = new AssetManager();
                        }
                    }
                }
                return SingletonContext;
            }
        }

        #endregion Singleton

        internal FileSystemWatcher Watcher;

        internal AssetManager()
        {
            if (ViewModelLocator.GameLoader.Directory != null)
            {
                Watcher = new FileSystemWatcher
                {
                    IncludeSubdirectories = true
                };
                Watcher.Path = ViewModelLocator.GameLoader.Directory;
                Watcher.EnableRaisingEvents = true;
                Watcher.Changed += FileChanged;
                Watcher.Created += FileCreated;
                Watcher.Renamed += FileRenamed;
                Watcher.Deleted += FileDeleted;
            }
            else
            {
                Watcher.EnableRaisingEvents = false;
            }
        }

        ~AssetManager()
        {
            Watcher.Changed -= FileChanged;
        }
        private void FileChanged(object sender, FileSystemEventArgs args)
        {
            Messenger.Default.Send(new AssetManagerUpdatedMessage());
        }
        private void FileCreated(object sender, FileSystemEventArgs args)
        {
            Messenger.Default.Send(new AssetManagerUpdatedMessage());
        }
        private void FileRenamed(object sender, FileSystemEventArgs args)
        {
            Messenger.Default.Send(new AssetManagerUpdatedMessage());
        }
        private void FileDeleted(object sender, FileSystemEventArgs args)
        {
            Messenger.Default.Send(new AssetManagerUpdatedMessage());
        }

        private ObservableCollection<Asset> _assets;

        public ObservableCollection<Asset> Assets
        {
            get
            {
                if (_assets == null)
                {
                    _assets = new ObservableCollection<Asset>();
                   // AssetManager.Instance.CollectAssets();
                }
                return _assets;
            }
            set
            {
                _assets = value;
            }
        }

        public Asset LoadAsset(AssetType validAssetType)
        {

            var fo = new OpenFileDialog
            {
                Filter = Asset.GetAssetFilters(validAssetType)
            };
            if ((bool)fo.ShowDialog() == false)
            {
                return null;
            }
            return LoadAsset(validAssetType, new FileInfo(fo.FileName));
        }
       
        public Asset LoadAsset(AssetType validAssetType, FileInfo file)
        {
            if (Asset.GetAssetType(file) != validAssetType) return null;
            var assetPath = Path.Combine(ViewModelLocator.GameLoader.Directory, "Assets");
            if (!Directory.Exists(assetPath))
                Directory.CreateDirectory(assetPath);

            var fileCopy = file.CopyTo(Utils.GetUniqueFilename(Path.Combine(assetPath, file.Name)));
            var asset = Asset.Load(fileCopy);
            Assets.Add(asset);
            return asset;
        }

        public void CollectAssets()
        {
            var di = new DirectoryInfo(ViewModelLocator.GameLoader.Directory);
            var files = di.GetFiles("*.*", SearchOption.AllDirectories);
            var ret = files.Select(Asset.Load);
            Assets = new ObservableCollection<Asset>(ret);
        }

    }

}