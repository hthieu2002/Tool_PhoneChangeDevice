using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ToolChange.Views
{
    /// <summary>
    /// Interaction logic for Document.xaml
    /// </summary>
    public partial class Document : Page
    {
        private readonly string[] _imagePaths = new[]
{
    "/Resources/Icons/scriptAutomationview.jpg",
    "/Resources/Icons/scriptAutomationview1.jpg",
    "/Resources/Icons/scriptAutomationview2.jpg"
};

        private CancellationTokenSource _imageLoopCts;
        private int _imageIndex = 0;

        public Document()
        {
            InitializeComponent();
            this.Unloaded += Document_Unloaded;
        }

        private bool _isExpanded = false;
        private bool _isExpanded1 = false;
        private void ToggleAnimatedPanel(Border panel, ref bool isExpanded, double expandedHeight = 260)
        {
            double from = isExpanded ? expandedHeight : 0;
            double to = isExpanded ? 0 : expandedHeight;

            var animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };

            panel.BeginAnimation(HeightProperty, animation);
            isExpanded = !isExpanded;
        }
        public void StartImageRotationBackground()
        {
            _imageLoopCts = new CancellationTokenSource();
            var token = _imageLoopCts.Token;

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    await System.Windows.Application.Current.Dispatcher.InvokeAsync(async () =>
                    {
                        // Fade out
                        var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
                        AutoSwitchImage.BeginAnimation(UIElement.OpacityProperty, fadeOut);
                        await Task.Delay(300);

                        // Đổi ảnh
                        AutoSwitchImage.Source = new BitmapImage(new Uri(_imagePaths[_imageIndex], UriKind.Relative));
                        _imageIndex = (_imageIndex + 1) % _imagePaths.Length;

                        // Fade in
                        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300));
                        AutoSwitchImage.BeginAnimation(UIElement.OpacityProperty, fadeIn);
                    });

                    try
                    {
                        await Task.Delay(10000, token); // Delay 10s
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }
            }, token);
        }

        public void StopImageRotationBackground()
        {
            _imageLoopCts?.Cancel();
            _imageLoopCts = null;
        }
        private async void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://docs.google.com/document/d/1fmuHbrPjCyIGiVeuv_q7bNrzSxefpeKTqjtbxucEEmc/preview";
            WebViewContainer.Visibility = Visibility.Visible;
            await WebViewDocs.EnsureCoreWebView2Async();
            WebViewDocs.CoreWebView2.Navigate(url);
        }
        private void CloseWebView_Click(object sender, RoutedEventArgs e)
        {
            WebViewContainer.Visibility = Visibility.Collapsed;
        }


        private void Document_Unloaded(object sender, RoutedEventArgs e)
        {
            StopImageRotationBackground(); 
        }
        private void TogglePanel_Click(object sender, MouseButtonEventArgs e)
        {
            ToggleAnimatedPanel(AnimatedPanel, ref _isExpanded, 260);
        }
        private void TogglePanelDocument1_Click(object sender, MouseButtonEventArgs e)
        {
            ToggleAnimatedPanel(AnimatedPanel1, ref _isExpanded1, 650);
            if (!_isExpanded1)
            {
                StopImageRotationBackground(); // nếu panel đang mở và sắp đóng → dừng
            }
            else
            {
                StartImageRotationBackground(); // nếu panel đang đóng và sắp mở → chạy
            }
        }
        private void TogglePanelDocument2_Click(object sender, MouseButtonEventArgs e)
        {
            ToggleAnimatedPanel(AnimatedPanel2, ref _isExpanded1, 350);
           
        }
    }
}
