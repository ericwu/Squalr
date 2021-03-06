﻿namespace Squalr.View
{
    using Squalr.Source.Results;
    using Squalr.Source.Scanners.Pointers;
    using SqualrCore.Source.Controls;
    using SqualrCore.Source.Engine.Types;
    using SqualrCore.Source.Utils;
    using SqualrCore.Source.Utils.Extensions;
    using System;
    using System.Threading.Tasks;
    using System.Windows.Controls;

    /// <summary>
    /// Interaction logic for PointerScanner.xaml.
    /// </summary>
    internal partial class PointerScanner : UserControl, IResultDataTypeObserver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ManualScanner"/> class.
        /// </summary>
        public PointerScanner()
        {
            this.InitializeComponent();

            // Windows Forms hosting -- TODO: Phase this out
            this.PointerScanAddressHexDecBox = new HexDecTextBox(DataTypes.UInt64);
            this.PointerScanAddressHexDecBox.IsHex = true;
            this.PointerScanAddressHexDecBox.TextChanged += this.PointerScanAddressUpdated;
            this.pointerScanAddressHexDecBox.Children.Add(WinformsHostingHelper.CreateHostedControl(this.PointerScanAddressHexDecBox));

            this.DepthHexDecBox = new HexDecTextBox(DataTypes.Int32);
            this.DepthHexDecBox.TextChanged += this.DepthUpdated;
            this.DepthHexDecBox.SetValue(PointerScannerViewModel.DefaultPointerScanDepth);
            this.depthHexDecBox.Children.Add(WinformsHostingHelper.CreateHostedControl(this.DepthHexDecBox));

            this.PointerRadiusHexDecBox = new HexDecTextBox(DataTypes.Int32);
            this.PointerRadiusHexDecBox.TextChanged += this.PointerRadiusUpdated;
            this.PointerRadiusHexDecBox.SetValue(PointerScannerViewModel.DefaultPointerScanRadius);
            this.pointerRadiusHexDecBox.Children.Add(WinformsHostingHelper.CreateHostedControl(this.PointerRadiusHexDecBox));

            this.PointerRescanAddressHexDecBox = new HexDecTextBox(DataTypes.UInt64);
            this.PointerRescanAddressHexDecBox.IsHex = true;
            this.PointerRescanAddressHexDecBox.TextChanged += this.PointerRescanAddressUpdated;
            this.pointerRescanAddressHexDecBox.Children.Add(WinformsHostingHelper.CreateHostedControl(this.PointerRescanAddressHexDecBox));

            this.PointerRescanValueHexDecBox = new HexDecTextBox();
            this.PointerRescanValueHexDecBox.TextChanged += this.PointerRescanValueUpdated;
            this.pointerRescanValueHexDecBox.Children.Add(WinformsHostingHelper.CreateHostedControl(this.PointerRescanValueHexDecBox));

            Task.Run(() => PointerScanResultsViewModel.GetInstance().Subscribe(this));
        }

        /// <summary>
        /// Gets the view model associated with this view.
        /// </summary>
        public PointerScannerViewModel PointerScannerViewModel
        {
            get
            {
                return this.DataContext as PointerScannerViewModel;
            }
        }

        /// <summary>
        /// Gets or sets the hex dec box used to display the current pointer scan address being edited.
        /// </summary>
        private HexDecTextBox PointerScanAddressHexDecBox { get; set; }

        /// <summary>
        /// Gets or sets the value hex dec box used to display the current pointer rescan address being edited.
        /// </summary>
        private HexDecTextBox PointerRescanAddressHexDecBox { get; set; }

        /// <summary>
        /// Gets or sets the value hex dec box used to display the current pointer rescan value being edited.
        /// </summary>
        private HexDecTextBox PointerRescanValueHexDecBox { get; set; }

        /// <summary>
        /// Gets or sets the value hex dec box used to display the current depth being edited.
        /// </summary>
        private HexDecTextBox DepthHexDecBox { get; set; }

        /// <summary>
        /// Gets or sets the value hex dec box used to display the current pointer radius being edited.
        /// </summary>
        private HexDecTextBox PointerRadiusHexDecBox { get; set; }

        /// <summary>
        /// Updates the active type.
        /// </summary>
        /// <param name="activeType">The new active type.</param>
        public void Update(DataType activeType)
        {
            this.PointerRescanValueHexDecBox.ElementType = activeType;
        }

        /// <summary>
        /// Invoked when the current value is changed, and informs the viewmodel.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event args.</param>
        private void PointerScanAddressUpdated(Object sender, EventArgs e)
        {
            Object value = this.PointerScanAddressHexDecBox.GetValue();
            this.PointerScannerViewModel.SetPointerScanAddressCommand.Execute(value);
        }

        /// <summary>
        /// Invoked when the current value is changed, and informs the viewmodel.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event args.</param>
        private void PointerRescanAddressUpdated(Object sender, EventArgs e)
        {
            Object value = this.PointerRescanAddressHexDecBox.GetValue();
            this.PointerScannerViewModel.SetPointerRescanAddressCommand.Execute(value);
        }

        /// <summary>
        /// Invoked when the current value is changed, and informs the viewmodel.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event args.</param>
        private void PointerRescanValueUpdated(Object sender, EventArgs e)
        {
            Object value = this.PointerRescanValueHexDecBox.GetValue();
            this.PointerScannerViewModel.SetPointerRescanValueCommand.Execute(value);
        }

        /// <summary>
        /// Invoked when the current value is changed, and informs the viewmodel.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event args.</param>
        private void DepthUpdated(Object sender, EventArgs e)
        {
            Object value = this.DepthHexDecBox.GetValue();
            Int32 realValue = value == null ? 0 : (Int32)Conversions.ParsePrimitiveStringAsPrimitive(DataTypes.Int32, value.ToString());

            if (this.DepthHexDecBox.IsValid())
            {
                this.DepthHexDecBox.SetValue(realValue.Clamp<Int32>(0, PointerScannerViewModel.MaximumPointerScanDepth));
            }

            this.PointerScannerViewModel.SetDepthCommand.Execute(realValue);
        }

        /// <summary>
        /// Invoked when the current value is changed, and informs the viewmodel.
        /// </summary>
        /// <param name="sender">Sending object.</param>
        /// <param name="e">Event args.</param>
        private void PointerRadiusUpdated(Object sender, EventArgs e)
        {
            Object value = this.PointerRadiusHexDecBox.GetValue();
            this.PointerScannerViewModel.SetPointerRadiusCommand.Execute(value == null ? 0 : Conversions.ParsePrimitiveStringAsPrimitive(DataTypes.Int32, value.ToString()));
        }
    }
    //// End class
}
//// End namespace