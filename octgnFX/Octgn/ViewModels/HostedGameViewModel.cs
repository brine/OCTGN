﻿using System.Net;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Octgn.Core;
using Octgn.Site.Api.Models;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

using log4net;

using Octgn.Core.DataManagers;
using Octgn.DataNew.Entities;
using Octgn.Online.Hosting;
using Octgn.Online;

namespace Octgn.ViewModels
{
    public class HostedGameViewModel : INotifyPropertyChanged
    {
        internal static ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private Guid gameId;

        private string gameName;

        private Version gameVersion;

        private string name;

        private string user;

        private int port;

        private HostedGameStatus status;

        private DateTime startTime;

        private bool canPlay;

        private bool _spectator;

        private bool hasPassword;
        private IPAddress _ipAddress;
        private string _gameSource;
        private ImageSource _userImage;

        private Guid id;

        public Guid GameId
        {
            get
            {
                return this.gameId;
            }
            private set
            {
                this.gameId = value;
                this.OnPropertyChanged("GameId");
            }
        }

        public string GameName
        {
            get
            {
                return this.gameName;
            }
            private set
            {
                if (value == this.gameName)
                {
                    return;
                }
                this.gameName = value;
                this.OnPropertyChanged("GameName");
            }
        }

        public Version GameVersion
        {
            get
            {
                return this.gameVersion;
            }
            private set
            {
                if (Equals(value, this.gameVersion))
                {
                    return;
                }
                this.gameVersion = value;
                this.OnPropertyChanged("GameVersion");
            }
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            private set
            {
                if (value == this.name)
                {
                    return;
                }
                this.name = value;
                this.OnPropertyChanged("Name");
            }
        }

        public string UserId
        {
            get
            {
                return this.user;
            }
            private set
            {
                if (Equals(value, this.user))
                {
                    return;
                }
                this.user = value;
                this.OnPropertyChanged("User");
            }
        }

        public int Port
        {
            get
            {
                return this.port;
            }
            private set
            {
                if (value == this.port)
                {
                    return;
                }
                this.port = value;
                this.OnPropertyChanged("Port");
            }
        }

        public HostedGameStatus Status
        {
            get
            {
                return this.status;
            }
            private set
            {
                if (value == this.status)
                {
                    return;
                }
                this.status = value;
                this.OnPropertyChanged("Status");
            }
        }

        public DateTime StartTime
        {
            get
            {
                return this.startTime;
            }
            private set
            {
                if (value.Equals(this.startTime))
                {
                    return;
                }
                this.startTime = value;
                this.OnPropertyChanged("StartTime");
            }
        }

        public string RunTime
        {
            get
            {
                return this.runTime;
            }
            set
            {
                if (value.Equals(this.runTime))
                {
                    return;
                }
                this.runTime = value;
                this.OnPropertyChanged("RunTime");
            }
        }

        public bool CanPlay
        {
            get
            {
                return this.canPlay;
            }
            private set
            {
                if (value.Equals(this.canPlay))
                {
                    return;
                }
                this.canPlay = value;
                this.OnPropertyChanged("CanPlay");
            }
        }

        public bool HasPassword
        {
            get
            {
                return this.hasPassword;
            }
            set
            {
                if (value.Equals(this.hasPassword))
                {
                    return;
                }
                this.hasPassword = value;
                this.OnPropertyChanged("HasPassword");
            }
        }

        public IPAddress IPAddress
        {
            get { return _ipAddress; }
            set
            {
                if (value.Equals(_ipAddress))
                    return;
                _ipAddress = value;
                this.OnPropertyChanged("IPAddress");
            }
        }

        public string GameSource
        {
            get { return _gameSource; }
            set
            {
                if (value.Equals(_gameSource)) return;
                _gameSource = value;
                this.OnPropertyChanged("GameSource");
            }
        }

        public ImageSource UserImage
        {
            get { return _userImage; }
            set
            {
                if (value.Equals(_userImage))
                    return;
                _userImage = value;
                OnPropertyChanged("UserImage");
            }
        }

        public Guid Id
        {
            get
            {
                return this.id;
            }
            set
            {
                if (value == this.Id) return;
                this.id = value;
                OnPropertyChanged("Id");
            }
        }

        public bool Visible
        {
            get { return _visible; }
            set
            {
                if (value == this._visible) return;
                _visible = value;
                OnPropertyChanged("Visible");
            }
        }

        public bool Spectator
        {
            get { return _spectator; }
            set
            {
                if (value == this._spectator) return;
                _spectator = value;
                OnPropertyChanged("Spectator");
            }
        }

        //public IHostedGameData Data { get; set; }

        public HostedGameViewModel(HostedGame game)
        {
            //Data = data;
            var gameManagerGame = GameManager.Get().GetById(game.GameId);

            this.Id = game.Id;
            this.GameId = game.GameId;
            this.GameVersion = game.GameVersion;
            this.Name = game.Name;
            this.UserId = game.HostUserId;
            this.Port = game.Port;
            this.Status = game.Status;
            this.StartTime = game.DateCreated.LocalDateTime;
            this.GameName = game.GameName;
            this.HasPassword = game.HasPassword;
            this.Visible = true;
            this.Spectator = game.Spectators;
            UpdateVisibility();
            switch (game.Source)
            {
                case HostedGameSource.Online:
                    GameSource = "Online";
                    break;
                case HostedGameSource.Lan:
                    GameSource = "Lan";
                    break;
            }
            if (gameManagerGame == null) return;
            this.CanPlay = true;
            this.GameName = gameManagerGame.Name;
            this.IPAddress = game.IpAddress;
        }

        public HostedGameViewModel()
        {
        }

        private string previousIconUrl = "";

        private string runTime;
        private bool _visible;

        public void UpdateVisibility()
        {
            if (Prefs.HideUninstalledGamesInList)
            {
                if (this.CanPlay == false)
                {
                    Visible = false;
                    return;
                }
            }
            if (Prefs.SpectateGames)
            {
                if (this.Status == HostedGameStatus.StartedHosting)
                {
                    Visible = false;
                    return;
                }
                if (this.Spectator)
                {
                    Visible = true;
                    return;
                }
                Visible = false;
                return;
            }
            else
            {
                if (this.Status == HostedGameStatus.StartedHosting)
                {
                    Visible = true;
                    return;
                }
                if (this.Status == HostedGameStatus.GameInProgress)
                {
                    Visible = false;
                    return;
                }
                Visible = false;
                return;
            }
        }

        public void Update(HostedGameViewModel newer, Game[] games)
        {
            var game = games.FirstOrDefault(x => x.Id == this.gameId);
            var u = new User(UserId, true);
            if (u.ApiUser != null)
            {
                if (!String.IsNullOrWhiteSpace(u.ApiUser.IconUrl))
                {
                    if (previousIconUrl != u.ApiUser.IconUrl)
                    {
                        try
                        {
                            previousIconUrl = u.ApiUser.IconUrl;
                            UserImage = new BitmapImage(new Uri(u.ApiUser.IconUrl));
                        }
                        catch (Exception e)
                        {
                            Log.Error("Can't set UserImage to Uri", e);
                        }
                    }
                }
            }
            var ts = new TimeSpan(DateTime.Now.Ticks - StartTime.Ticks);
            RunTime = string.Format("{0}h {1}m", Math.Floor(ts.TotalHours), ts.Minutes);
            if (newer != null)
            {
                Status = newer.Status;
            }
            UpdateVisibility();
            if (game == null)
            {
                this.CanPlay = false;
                return;
            }
            this.CanPlay = true;

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}