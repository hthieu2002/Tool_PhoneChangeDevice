using Services;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System;
using System.Windows.Forms;
using System.IO;
using ToolChange.Script.RoslynTest;

public class EventRoslyn
{
    RoslynStringProcessing roslyn = new RoslynStringProcessing();
    public EventRoslyn()
    {
        // Khởi tạo tài nguyên nếu có
    }

    // Phương thức xử lý ClickXY
    public async Task HandleClickXY(string input, string deviceId)
    {
        try
        {
            var coordinates = roslyn.getParseClickCoordinates(input);
            string scrcpyPath = Path.Combine(Application.StartupPath, "Resources", "scrcpy.exe");

            if (File.Exists(scrcpyPath))
            {
                string command = $" -s {deviceId} shell input tap {coordinates.Item1} {coordinates.Item2}";
                ADBService.ExecuteAdbCommand(command);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Lỗi khi thực thi lệnh ClickXY: " + ex.Message);
        }
    }

    // Swipe
    public async Task HandleSwipe(string input, string deviceId)
    {

        var coordinates = roslyn.getParseSwipeCoordinates(input);
        string scrcpyPath = Path.Combine(Application.StartupPath, "Resources", "scrcpy.exe");
        try
        {
            if (File.Exists(scrcpyPath))
            {
                string command = $" -s {deviceId} shell input swipe {coordinates.Item1} {coordinates.Item2} {coordinates.Item3} {coordinates.Item4} {coordinates.Item5}";
                ADBService.ExecuteAdbCommand(command);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Lỗi khi thực thi lệnh adb swipe: " + ex.Message);
        }
    }

    public async Task HandleRandom(string input, string deviceId)
    {
        var coordinates = roslyn.getParseRandomClickCoordinates(input);
        string adbPath = Path.Combine(Application.StartupPath, "Resources", "scrcpy.exe");
        Random random = new Random();
        try
        {
            var x = random.Next(coordinates.Item1, coordinates.Item2);
            var y = random.Next(coordinates.Item3, coordinates.Item4);
            Console.WriteLine($"{x},{y}");
            if (File.Exists(adbPath))
            {
                string command = $" -s {deviceId} shell input tap {x} {y}";
                ADBService.ExecuteAdbCommand(command);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Lỗi khi thực thi lệnh adb click");
        }
    }
}
