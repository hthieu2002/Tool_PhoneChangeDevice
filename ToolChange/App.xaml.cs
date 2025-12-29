using System.Reflection;
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


            CheckFirstRunOfNewVersion();
            // Thử một trong hai giá trị dưới đây để xem Win10 còn lag không
            // Bắt GPU render (mặc định)
            RenderOptions.ProcessRenderMode = RenderMode.Default;

            // Hoặc ép dùng software render (đôi khi mượt hơn nếu driver GPU cũ)
            // RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;

            base.OnStartup(e);
        }


        private void CheckFirstRunOfNewVersion()
        {
            string currentVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string savedVersion = DeepDroid.Properties.Settings.Default.Version;

            if (currentVersion != savedVersion)
            {
                // Reset về mặc định
                DeepDroid.Properties.Settings.Default.Reset();

                // Cập nhật version hiện tại để lần sau không reset nữa
                DeepDroid.Properties.Settings.Default.Version = currentVersion;
                DeepDroid.Properties.Settings.Default.Save();
            }
        }

    }

}
