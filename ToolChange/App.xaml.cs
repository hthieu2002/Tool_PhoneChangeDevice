using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace ToolChange
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Thử một trong hai giá trị dưới đây để xem Win10 còn lag không
            // Bắt GPU render (mặc định)
            RenderOptions.ProcessRenderMode = RenderMode.Default;

            // Hoặc ép dùng software render (đôi khi mượt hơn nếu driver GPU cũ)
            // RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            base.OnStartup(e);
        }
    }

}
