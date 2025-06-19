using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ToolChange.Models;

namespace WindowsFormsApp.Script.Roslyn
{
    // test function
    public class ScriptAutomation
    {
        public async Task TestFunction(string textRoslyn, string deviceId)
        {
            string command = textRoslyn.Split('(')[0].Trim();
            await ExecuteRoslynScript(command, textRoslyn, deviceId);
        }

        private async Task ExecuteRoslynScript(string command, string textRoslyn, string deviceId)
        {
            EventRoslyn eventRoslyn = new EventRoslyn();

            var globals = new Globals { eventRoslyn = eventRoslyn };

            switch (command)    
            {
                case "ClickXY":
                    string code = $"await eventRoslyn.HandleClickXY(\"{textRoslyn}\", \"{deviceId}\");";
                    await ExecuteDynamicCode(code, globals);
                    break;
                case "Swipe":
                    string swipeCode = $"await eventRoslyn.HandleSwipe(\"{textRoslyn}\", \"{deviceId}\");";
                    await ExecuteDynamicCode(swipeCode, globals);
                    break;
                case "RandomClick":
                    string randomClickCode = $"await eventRoslyn.HandleRandom(\"{textRoslyn}\", \"{deviceId}\")";
                    await ExecuteDynamicCode(randomClickCode, globals);
                    break;
                default:
                    Console.WriteLine("Lệnh không nhận diện.");
                    break;
            }
        }


        private async Task ExecuteDynamicCode(string code, Globals globals)
        {
            try
            {
                var references = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.Location))  
                    .Select(a => MetadataReference.CreateFromFile(a.Location));

                if (!references.Any())
                {
                    throw new InvalidOperationException("Không có assembly hợp lệ để tham chiếu.");
                }

                var result = await CSharpScript.EvaluateAsync<object>(
                    code,
                    globals: globals,  
                    options: ScriptOptions.Default
                        .WithReferences(references)  
                        .WithImports("System", "System.IO", "System.Linq") 
                );

                Console.WriteLine($"Mã đã được thực thi: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi thực thi mã động: {ex.Message}");
            }
        }
    }
}

