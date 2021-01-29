// /* This Source Code Form is subject to the terms of the Mozilla Public
//  * License, v. 2.0. If a copy of the MPL was not distributed with this
//  * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using log4net;
using Octgn.Core.DataExtensionMethods;
using Octgn.Core.DataManagers;
using Octgn.DataNew.Entities;
using Octgn.Library;

namespace Octgn.ViewModels
{
    public class ImageManagerViewModel : ViewModelBase
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ObservableCollection<ImageManagerGameModel> Games { get; private set; }
        private object _gameLock = new object();
        private object _cardsLock = new object();

        public ImageManagerGameModel selectedGame;
        public ImageManagerSetModel selectedSet;

        public ObservableCollection<ImageManagerCardModel> Cards { get; private set; }

        public ObservableCollection<ImageManagerImageModel> Images { get; private set; }

        public ImageManagerViewModel()
        {
            Games = new ObservableCollection<ImageManagerGameModel>();
            Cards = new ObservableCollection<ImageManagerCardModel>();
            Images = new ObservableCollection<ImageManagerImageModel>();
            BindingOperations.EnableCollectionSynchronization(Games, _gameLock);
            BindingOperations.EnableCollectionSynchronization(Cards, _cardsLock);
            BindingOperations.EnableCollectionSynchronization(Images, _cardsLock);

            LoadGames();
        }

        private async void LoadGames()
        {
            var imageDirectory = new DirectoryInfo(Config.Instance.ImageDirectoryFull);
            await Task.Run(() =>
            {
                var gamesList = imageDirectory.GetDirectories();
                lock (_gameLock)
                {
                    // register all files in image database
                    foreach (var gameDir in gamesList)
                    {
                        Log.Info("Loading Game at directory " + gameDir.FullName);
                        var game = new ImageManagerGameModel(gameDir);
                        Games.Add(game);
                    }
                    // load games
                    foreach (var game in Games)
                    {
                        game.LoadGame();
                    }
                }
            });

        }
        public ImageManagerGameModel SelectedGame
        {
            get { return this.selectedGame; }
            set
            {
                if (Equals(value, this.selectedGame))
                {
                    return;
                }
                this.selectedGame = value;
                UpdateCards();
                RaisePropertyChanged(nameof(SelectedGame));
            }
        }
        public ImageManagerSetModel SelectedSet
        {
            get { return this.selectedSet; }
            set
            {
                if (Equals(value, this.selectedSet))
                {
                    return;
                }
                this.selectedSet = value;
                UpdateCards();
                RaisePropertyChanged(nameof(SelectedSet));
                RaisePropertyChanged(nameof(IsSetSelected));
            }
        }
        public bool IsSetSelected => SelectedSet != null;


        private async void UpdateCards()
        {
            IsBusy = true;
            await Task.Run(() =>
            {
                lock (_cardsLock)
                {
                    Cards.Clear();
                    Images.Clear();
                    if (SelectedSet != null)
                    {
                        foreach (var file in SelectedSet.Files)
                        {
                            var image = new ImageManagerImageModel(file, SelectedSet);
                            if (image.IsValid)
                                Images.Add(image);
                        }
                        if (SelectedSet.Status == ImageManagerItemStatus.Installed)
                        {
                            Log.Info("Loading Cards for Set " + SelectedSet.Set?.Name ?? SelectedSet.Id.ToString());
                            foreach (var card in SelectedSet.Set.Cards)
                            {
                                foreach (var alt in card.PropertySets.Values)
                                {
                                    var cardVM = new ImageManagerCardModel(card, alt, SelectedSet);
                                    var fileName = card.ImageUri;
                                    if (!String.IsNullOrWhiteSpace(alt.Type))
                                        fileName = fileName + "." + alt.Type;

                                    var cardImage = Images.FirstOrDefault(x => x.Name == fileName);
                                    if (cardImage != null)
                                    {
                                        cardVM.Image = cardImage;
                                        Images.Remove(cardImage);
                                    }
                                    Cards.Add(cardVM);
                                }
                            }
                        }
                    }
                    else
                    {
                        //
                    }
                }
                IsBusy = false;
            });
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                _isBusy = value;
                RaisePropertyChanged(nameof(IsBusy));
            }
        }

    }

    public class ImageManagerGameModel : ViewModelBase
    {
        private static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public DirectoryInfo Directory { get; set; }
        public Guid Id { get; set; }
        public Game Game { get; private set; }
        public string Name => Game?.Name ?? "Unknown Game";

        public RelayCommand ShowSetsToggleButton { get; private set; }
        public RelayCommand BrowseFolderButton { get; private set; }
        public RelayCommand DeleteImagesButton { get; private set; }

        public int TotalSetCount => Sets.Count;
        public int InstalledSetsCount => Sets.Count(x => x.Status == ImageManagerItemStatus.Installed);

        public ObservableCollection<ImageManagerSetModel> Sets { get; private set; }

        private ImageManagerItemStatus _status;
        public ImageManagerItemStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                if (_status == value) return;
                _status = value;
                RaisePropertyChanged(nameof(Status));
            }
        }

        public bool _isExpanded;
        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                if (_isExpanded == value) return;
                if (value == true)
                {
                    Task.Run(() =>
                    {
                        foreach (var set in Sets)
                        {
                            set.LoadSet();
                        }
                    });
                }
                _isExpanded = value;
                RaisePropertyChanged(nameof(IsExpanded));
            }
        }

        public string GameIcon => Game?.IconUrl;

        public ImageManagerGameModel()
        {

        }

        public ImageManagerGameModel(DirectoryInfo directory)
        {
            ShowSetsToggleButton = new RelayCommand(ShowSetsToggle);
            BrowseFolderButton = new RelayCommand(BrowseFolder);
            DeleteImagesButton = new RelayCommand(DeleteImages);
            Directory = directory;
            Log.Info("Registering Game at directory " + directory.FullName);
            if (Guid.TryParse(directory.Name, out var id))
            {
                Id = id;
                Log.Info("-> Directory is a valid game.");
                Sets = new ObservableCollection<ImageManagerSetModel>();
                var setsDirectoryList = new DirectoryInfo(Path.Combine(Directory.FullName, "Sets")).GetDirectories();
                Log.Info("-> " + setsDirectoryList.Count() + " Set directories found.");
                foreach (var s in setsDirectoryList)
                {
                    var set = new ImageManagerSetModel(s, this);
                    Sets.Add(set);
                }
            }
            else
            {
                Log.Info("-> Directory was not a valid game.");
                Status = ImageManagerItemStatus.Invalid;
            }
        }

        public void DeleteImages()
        {

        }
        public void ShowSetsToggle()
        {
            IsExpanded = !IsExpanded;
        }

        public void BrowseFolder()
        {
            if (Directory != null && Directory.Exists)
                Process.Start("explorer.exe", Directory.FullName);
        }


        public void LoadGame()
        {
            if (Status == ImageManagerItemStatus.Unloaded)
            {
                Log.Info("Locating Game with Id " + Id.ToString());
                Game = GameManager.Get().GetById(Id);
                if (Game == null)
                {
                    Log.Info("-> Game is not installed.");
                    Status = ImageManagerItemStatus.Unknown;
                }
                else
                {
                    Log.Info("Game identified as:  " + Game.Name);
                    Status = ImageManagerItemStatus.Installed;
                }
            }
            RaisePropertyChanged(nameof(GameIcon));
            RaisePropertyChanged(nameof(Name));
        }
    }

    public class ImageManagerSetModel : ViewModelBase
    {
        private static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ImageManagerGameModel Parent { get; set; }
        public Guid Id { get;  set; }

        public string Name => Set?.Name;
        public Set Set { get; private set; }
        public DirectoryInfo Directory { get; set; }

        private ImageManagerItemStatus _status;
        public ImageManagerItemStatus Status
        {
            get
            {
                return _status;
            }
            set
            {
                if (_status == value) return;
                _status = value;
                RaisePropertyChanged(nameof(Status));
            }
        }
        public FileInfo[] Files;

        public ImageManagerSetModel(DirectoryInfo directory, ImageManagerGameModel parent)
        {
            Parent = parent;
            Directory = directory;
            Log.Info("Registering Set at directory " + directory.FullName);
            if (Guid.TryParse(directory.Name, out var id))
            {
                Id = id;
                Log.Info("-> Directory is a valid set.");
                Files = new DirectoryInfo(Path.Combine(Directory.FullName, "Cards")).GetFiles();
                Log.Info("-> " + Files.Count() + " files found.");
            }
            else
            {
                Log.Info("-> Directory was not a valid set.");
                Status = ImageManagerItemStatus.Invalid;
            }
        }

        public void LoadSet()
        {
            if (Status == ImageManagerItemStatus.Unloaded)
            {
                if (Parent.Status == ImageManagerItemStatus.Installed)
                {
                    Log.Info("Locating Set with Id " + Id.ToString());
                    Set = SetManager.Get().GetById(Id);
                    if (Set == null)
                    {
                        Log.Info("-> Set is not installed.");
                        Status = ImageManagerItemStatus.Unknown;
                    }
                    else
                    {
                        Log.Info("Set identified as:  " + Set.Name);
                        Status = ImageManagerItemStatus.Installed;
                    }
                }
            }
            RaisePropertyChanged(nameof(Name));
        }
    }

    public class ImageManagerCardModel : ViewModelBase
    {
        private static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ImageManagerImageModel Image { get; set; }
        public Card SourceCard { get; private set; }
        public CardPropertySet Alternate { get; private set; }


        public ImageManagerCardModel(Card card, CardPropertySet alt, ImageManagerSetModel parent)
        {
            SourceCard = card;
            Alternate = alt;
        }
    }

    public class ImageManagerImageModel : ViewModelBase
    {
        private static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ImageManagerSetModel Parent { get; private set; }
        public Guid Id { get; private set; }
        public string Name => Path.GetFileNameWithoutExtension(File?.Name);

        public static readonly List<string> ValidImageTypes = new List<string> { ".png", ".jpg", ".jpeg", ".gif", ".tiff", ".bmp" };
        public bool IsValid => ValidImageTypes.Contains(File?.Extension.ToLowerInvariant());
        public FileInfo File { get; private set; }

        public ImageManagerImageModel(FileInfo file, ImageManagerSetModel parent)
        { 
            File = file;
            Parent = parent;
        }

        private BitmapImage _image;
        public ImageSource Image
        {
            get
            {
                if (IsValid)
                {
                    /*    if (_image == null)
                        {
                            var time = new Stopwatch();
                            time.Start();
                            var bim = new BitmapImage();
                            bim.BeginInit();
                            bim.CacheOption = BitmapCacheOption.OnLoad;
                            bim.UriSource = new Uri(File?.FullName);
                            bim.EndInit();
                            bim.Freeze();
                            _image = bim;
                            Log.Info("Loading Image " + File?.Name + " (" + time.ElapsedMilliseconds + ")");
                        }
                        return _image;*/
                    return BitmapFrame.Create(new Uri(File?.FullName), BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnLoad);
                }
                else
                {
                    return null;
                }
            }
        }
    }

    public enum ImageManagerItemStatus
    {
        Unloaded,
        Invalid,
        Unknown,
        Installed
    }
}
