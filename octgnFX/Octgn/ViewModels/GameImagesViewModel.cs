// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using Octgn.Core.DataExtensionMethods;
using Octgn.Core.DataManagers;
namespace Octgn.ViewModels
{
    public class GameImagesViewModel : INotifyPropertyChanged
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public bool IsInstalled { get; private set; }
        public bool IsValid { get; private set; }
        public DirectoryInfo Directory { get; private set; }
        public ObservableCollection<SetImagesViewModel> Sets { get; private set; }

        public GameImagesViewModel(DirectoryInfo directory)
        {
            Directory = directory;
            Sets = new ObservableCollection<SetImagesViewModel>();
            if (Guid.TryParse(directory.Name, out var id))
            {
                Id = id;
                IsValid = true;
                var game = GameManager.Get().GetById(id);
                if (game == null)
                {
                    IsInstalled = false;
                }
                else
                {
                    Name = game.Name;
                    IsInstalled = true;
                }
            }
            else
            {
                IsValid = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class SetImagesViewModel : INotifyPropertyChanged
    {
        public GameImagesViewModel Parent { get; private set; }
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public bool IsInstalled { get; private set; }
        public bool IsValid { get; private set; }
        public DirectoryInfo Directory { get; private set; }
        public ObservableCollection<CardImagesViewModel> Cards { get; private set; }

        public SetImagesViewModel(DirectoryInfo directory, GameImagesViewModel parent)
        {
            Parent = parent;
            Directory = directory;
            Cards = new ObservableCollection<CardImagesViewModel>();
            if (Guid.TryParse(directory.Name, out var id))
            {
                Id = id;
                IsValid = true;
                if (Parent.IsInstalled)
                {
                    var set = SetManager.Get().GetById(id);
                    if (set != null)
                    {
                        Name = set.Name;
                        IsInstalled = true;
                    }
                }
            }
            else
            {
                IsValid = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
    public class CardImagesViewModel : INotifyPropertyChanged
    {
        public SetImagesViewModel Parent { get; private set; }
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public bool IsInstalled { get; private set; }
        public bool IsValid { get; private set; }
        public FileInfo File { get; private set; }

        public CardImagesViewModel(FileInfo file, SetImagesViewModel parent)
        {
            File = file;
            Parent = parent;
            if (Guid.TryParse(file.Name, out var id))
            {
                Id = id;
                IsValid = true;
                var card = Parent.Cards.FirstOrDefault(x => x.Id == id);
                if (card != null)
                {
                    Name = card.Name;
                    IsInstalled = true;
                }
            }
            else
            {
                IsValid = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
