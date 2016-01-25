﻿using Binarysharp.MemoryManagement;
using Binarysharp.MemoryManagement.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Anathema
{
    delegate void DebuggerEventHandler(Object Sender, DebuggerEventArgs Args);
    class DebuggerEventArgs : EventArgs
    {
        public UInt64 ElementCount = 0;
        public UInt64 MemorySize = 0;
    }

    interface IDebuggerView : IView
    {
        // Methods invoked by the presenter (upstream)
        void ReadValues();
        void EnableDebugger();
        void DisableDebugger();
        void UpdateMemorySizeLabel(String MemorySize, String ItemCount);
        void UpdateItemCount(Int32 ItemCount);
    }

    abstract class IDebuggerModel : RepeatedTask, IModel
    {
        // Events triggered by the model (upstream)
        public event DebuggerEventHandler EventReadValues;
        protected virtual void OnEventReadValues(DebuggerEventArgs E)
        {
            EventReadValues(this, E);
        }
        public event DebuggerEventHandler EventEnableDebugger;
        protected virtual void OnEventEnableDebugger(DebuggerEventArgs E)
        {
            EventEnableDebugger(this, E);
        }
        public event DebuggerEventHandler EventDisableDebugger;
        protected virtual void OnEventDisableDebugger(DebuggerEventArgs E)
        {
            EventDisableDebugger(this, E);
        }
        public event DebuggerEventHandler EventFlushCache;
        protected virtual void OnEventFlushCache(DebuggerEventArgs E)
        {
            if (EventFlushCache != null) EventFlushCache(this, E);
        }

        public override void Begin()
        {
            WaitTime = Settings.GetInstance().GetResultReadInterval();
            base.Begin();
        }

        protected override void Update()
        {
            WaitTime = Settings.GetInstance().GetResultReadInterval();
        }

        // Functions invoked by presenter (downstream)
        public abstract void AddSelectionToTable(Int32 Index);

        public abstract IntPtr GetAddressAtIndex(Int32 Index);
        public abstract String GetValueAtIndex(Int32 Index);
        public abstract String GetLabelAtIndex(Int32 Index);
        public abstract Type GetScanType();
        public abstract void SetScanType(Type ScanType);

        public abstract void UpdateReadBounds(Int32 StartReadIndex, Int32 EndReadIndex);
    }

    class DebuggerPresenter : Presenter<IDebuggerView, IDebuggerModel>
    {
        protected new IDebuggerView View;
        protected new IDebuggerModel Model;

        private ListViewCache ListViewCache;

        private const Int32 AddressIndex = 0;
        private const Int32 ValueIndex = 1;
        private const Int32 LabelIndex = 2;

        public DebuggerPresenter(IDebuggerView View, IDebuggerModel Model) : base(View, Model)
        {
            this.View = View;
            this.Model = Model;

            ListViewCache = new ListViewCache();

            // Bind events triggered by the model
            Model.EventReadValues += EventReadValues;
            Model.EventEnableDebugger += EventEnableDebugger;
            Model.EventDisableDebugger += EventDisableDebugger;
            Model.EventFlushCache += EventFlushCache;
        }

        #region Method definitions called by the view (downstream)
        
        public void UpdateReadBounds(Int32 StartReadIndex, Int32 EndReadIndex)
        {
            Model.UpdateReadBounds(StartReadIndex, EndReadIndex);
        }

        public ListViewItem GetItemAt(Int32 Index)
        {
            ListViewItem Item = ListViewCache.Get(Index);

            // Try to update and return the item if it is a valid item
            if (Item != null && ListViewCache.TryUpdateSubItem(Index, ValueIndex, Model.GetValueAtIndex(Index)))
                return Item;
            
            // Add the properties to the cache and get the list view item back
            Item = ListViewCache.Add(Index, new String[] { String.Empty, String.Empty, String.Empty });

            Item.SubItems[AddressIndex].Text = Conversions.ToAddress(Model.GetAddressAtIndex(Index).ToString());
            Item.SubItems[ValueIndex].Text = "-";
            Item.SubItems[LabelIndex].Text = Model.GetLabelAtIndex(Index);

            return Item;
        }

        public void AddSelectionToTable(Int32 Index)
        {
            Model.AddSelectionToTable(Index);
        }

        public void UpdateScanType(Type ScanType)
        {
            if (ScanType == typeof(Byte) || ScanType == typeof(UInt16) || ScanType == typeof(UInt32) || ScanType == typeof(UInt64))
                throw new Exception("Invalid type. ScanType parameter assumes signed type.");

            // Apply type change
            Type PreviousScanType = Model.GetScanType();
            Model.SetScanType(ScanType);

            switch (Type.GetTypeCode(ScanType))
            {
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    ChangeSign();
                    break;
                default: return;
            }
        }

        public void ChangeSign()
        {
            Type ScanType = Model.GetScanType();

            switch (Type.GetTypeCode(ScanType))
            {
                case TypeCode.Byte: ScanType = typeof(SByte); break;
                case TypeCode.SByte: ScanType = typeof(Byte); break;
                case TypeCode.Int16: ScanType = typeof(UInt16); break;
                case TypeCode.Int32: ScanType = typeof(UInt32); break;
                case TypeCode.Int64: ScanType = typeof(UInt64); break;
                case TypeCode.UInt16: ScanType = typeof(Int16); break;
                case TypeCode.UInt32: ScanType = typeof(Int32); break;
                case TypeCode.UInt64: ScanType = typeof(Int64); break;
                default: return;
            }

            Model.SetScanType(ScanType);
        }

        #endregion

        #region Event definitions for events triggered by the model (upstream)

        private void EventReadValues(Object Sender, DebuggerEventArgs E)
        {
            View.ReadValues();
        }

        private void EventEnableDebugger(Object Sender, DebuggerEventArgs E)
        {
            View.EnableDebugger();
        }

        private void EventDisableDebugger(Object Sender, DebuggerEventArgs E)
        {
            View.DisableDebugger();
        }

        private void EventFlushCache(Object Sender, DebuggerEventArgs E)
        {
            ListViewCache.FlushCache();
            View.UpdateMemorySizeLabel(Conversions.BytesToMetric(E.MemorySize), E.ElementCount.ToString());
            View.UpdateItemCount((Int32)Math.Min(E.ElementCount, Int32.MaxValue));
        }

        #endregion
    }
}