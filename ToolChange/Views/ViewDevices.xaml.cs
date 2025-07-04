using LibVLCSharp.Shared;
using Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Interop;
using System.Windows.Media;
using ToolChange.Services;
using ToolChange.ViewModels;

namespace ToolChange.Views
{
    public partial class ViewDevices : System.Windows.Controls.Page
    {
        private viewDevicesViewModel ViewModel => DataContext as viewDevicesViewModel;

        public ViewDevices()
        {
            InitializeComponent();
            DataContext = ViewModelLocator.ViewDevicesViewModel;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel?.StartMonitoring();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            ViewModel?.StopMonitoring();
        }
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is Slider slider)
            {
                if (slider.Name == "ResolutionValueSlider")
                    ResolutionValue.Text = ((int)slider.Value).ToString();
                else if (slider.Name == "ScaleValueSlider")
                    ScaleValue.Text = ((int)slider.Value).ToString();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Tắt màn hình khi xem đã được kích hoạt!");
        }
        private void CloseAllScrcpy_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.CloseScrcpyWindows();
        }

    }
}