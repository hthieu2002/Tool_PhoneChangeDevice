using Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;

namespace ToolChange.Views
{
    public partial class ViewDevices : System.Windows.Controls.Page
    {
        public ViewDevices()
        {
            InitializeComponent();
           
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
    }
}