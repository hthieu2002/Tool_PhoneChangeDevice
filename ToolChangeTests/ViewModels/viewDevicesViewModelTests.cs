using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToolChange.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolChange.ViewModels.Tests
{
    [TestClass()]
    public class viewDevicesViewModelTests
    {
        [TestMethod()]
        public void CaptureScreenAsyncTest()
        {
            CancellationToken token = new CancellationToken();
            viewDevicesViewModel viewDevicesViewModel = new viewDevicesViewModel();
            var img = viewDevicesViewModel.CaptureScreenAsync("93JAY0BKPF", token);
            Console.WriteLine(img.Result);
        }
    }
}