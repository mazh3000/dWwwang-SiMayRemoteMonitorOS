﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SiMay.Core;
using SiMay.Core.Enums;
using SiMay.Core.PacketModelBinder.Attributes;
using SiMay.Core.PacketModelBinding;
using SiMay.Core.Packets.Startup;
using SiMay.Net.SessionProvider.SessionBased;

namespace SiMay.RemoteControlsCore.HandlerAdapters
{
    public class StartupAdapterHandler : AdapterHandlerBase
    {
        public readonly IReadOnlyList<GroupItem> StartupGroupItems;

        public event Action<StartupAdapterHandler, IEnumerable<StartupItemPack>> OnStartupItemHandlerEvent;

        private PacketModelBinder<SessionHandler, MessageHead> _handlerBinder = new PacketModelBinder<SessionHandler, MessageHead>();
        public StartupAdapterHandler()
        {
            var starupItems = new List<GroupItem>();
            starupItems.Add(new GroupItem
            {
                StartupType = StartupType.LocalMachineRun,
                StartupPath = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run"
            });
            starupItems.Add(new GroupItem
            {
                StartupType = StartupType.LocalMachineRunOnce,
                StartupPath = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce"
            });
            starupItems.Add(new GroupItem
            {
                StartupType = StartupType.CurrentUserRun,
                StartupPath = "HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run"
            });
            starupItems.Add(new GroupItem
            {
                StartupType = StartupType.CurrentUserRunOnce,
                StartupPath = "HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\RunOnce"
            });
            starupItems.Add(new GroupItem
            {
                StartupType = StartupType.LocalMachineWoW64Run,
                StartupPath = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\Run"
            });

            starupItems.Add(new GroupItem
            {
                StartupType = StartupType.LocalMachineWoW64RunOnce,
                StartupPath = "HKEY_LOCAL_MACHINE\\SOFTWARE\\Wow6432Node\\Microsoft\\Windows\\CurrentVersion\\RunOnce"
            });
            starupItems.Add(new GroupItem
            {
                StartupType = StartupType.StartMenu,
                StartupPath = "%APPDATA%\\Microsoft\\Windows\\Start Menu\\Programs\\Startup"
            });

            StartupGroupItems = starupItems;
        }
        internal override void MessageReceived(SessionHandler session)
        {
            if (this.IsClose)
                return;

            _handlerBinder.InvokePacketHandler(session, session.CompletedBuffer.GetMessageHead<MessageHead>(), this);
        }

        [PacketHandler(MessageHead.C_STARTUP_LIST)]
        private void HandlerStartupItems(SessionHandler session)
        {
            var pack = session.CompletedBuffer.GetMessageEntity<StartupItemsPack>();
            OnStartupItemHandlerEvent?.Invoke(this, pack.StartupItems);
        }

        public void GetStartup()
        {
            SendAsyncMessage(MessageHead.S_STARTUP_GET_LIST);
        }

        public void AddStartupItem(string path, string name, StartupType startupType)
        {
            SendAsyncMessage(MessageHead.S_STARTUP_ADD_ITEM, new StartupItemPack()
            {
                Name = name,
                Path = path,
                Type = startupType
            });
        }

        public void RemoveStartupItem(IEnumerable<StartupItemPack> startupItems)
        {
            SendAsyncMessage(MessageHead.S_STARTUP_REMOVE_ITEM, new StartupItemsPack()
            {
                StartupItems = startupItems.ToArray()
            });
        }

        public override void CloseHandler()
        {
            this._handlerBinder.Dispose();
            base.CloseHandler();
        }

        public class GroupItem
        {
            public StartupType StartupType { get; set; }
            public string StartupPath { get; set; }
        }
    }
}
