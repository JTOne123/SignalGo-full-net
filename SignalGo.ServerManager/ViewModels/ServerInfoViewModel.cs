﻿using MvvmGo.Commands;
using MvvmGo.ViewModels;
using SignalGo.Server.ServiceManager;
using SignalGo.ServerManager.Models;
using SignalGo.ServerManager.Views;
using SignalGo.Shared.Log;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SignalGo.ServerManager.ViewModels
{
    public class ServerInfoViewModel : BaseViewModel
    {
        public ServerInfoViewModel()
        {
            StartCommand = new Command(Start);
            StopCommand = new Command(Stop);
            DeleteCommand = new Command(Delete);
            ClearLogCommand = new Command(ClearLog);
            CopyCommand = new Command<TextLogInfo>(Copy);
        }

        public Command StartCommand { get; set; }
        public Command StopCommand { get; set; }
        public Command DeleteCommand { get; set; }
        public Command ClearLogCommand { get; set; }
        public Command<TextLogInfo> CopyCommand { get; set; }

        ServerInfo _ServerInfo;

        public ServerInfo ServerInfo
        {
            get
            {
                return _ServerInfo;
            }

            set
            {
                _ServerInfo = value;
                OnPropertyChanged(nameof(ServerInfo));
            }
        }


        private void Delete()
        {
            SettingInfo.Current.ServerInfoes.Remove(ServerInfo);
            SettingInfo.SaveSettingInfo();
            MainWindowViewModel.MainFrame.GoBack();
        }

        private void Stop()
        {
            ServerInfo.Stop();
        }

        private void Start()
        {
            StartServer(ServerInfo);
        }

        public static void StartServer(ServerInfo serverInfo)
        {
            serverInfo.Start();
        }

        private void ClearLog()
        {
            // ServerInfo.Logs.Clear();
        }

        private void Copy(TextLogInfo textLogInfo)
        {
            Clipboard.SetText(textLogInfo.Text);
        }

    }
}
