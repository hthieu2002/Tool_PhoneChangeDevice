using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.IO.Ports;
using System.Windows.Forms;
using System.Linq;

namespace WindowsFormsApp.Script.RoslynScript
{
    static class MethodFactory
    {
        // Tạo phương thức Run() với các câu lệnh truyền vào
        public static MethodDeclarationSyntax CreateRunMethod(List<StatementSyntax> statements)
        {
            return SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    "Run")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .WithBody(SyntaxFactory.Block(statements));
        }

        // Tạo class CommandExecutor với ClickXY và Run()
        public static ClassDeclarationSyntax CreateCommandExecutorClass(MethodDeclarationSyntax runMethod, string deviceID)
        {
            // Field _deviceID
            var deviceIdField = SyntaxFactory.FieldDeclaration(
                SyntaxFactory.VariableDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)))
                .AddVariables(
                    SyntaxFactory.VariableDeclarator("_deviceID")
                        .WithInitializer(
                            SyntaxFactory.EqualsValueClause(
                                SyntaxFactory.LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    SyntaxFactory.Literal(deviceID)
                                )
                            )
                        )
                    )
                )
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));

            var deviceIdParam = SyntaxFactory.Parameter(SyntaxFactory.Identifier("deviceID"))
                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)));

            var constructor = SyntaxFactory.ConstructorDeclaration("CommandExecutor")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(deviceIdParam)
                .WithBody(SyntaxFactory.Block(
                    SyntaxFactory.ParseStatement("_deviceID = deviceID;")
                ));
            // click tool box
            var clickMethod = CreateMethodClickXY();
            var waitMethod = CreateMethodWait();
            var swipeMethod = CreateMethodSwipe();
            var randomClickMethod = CreateMethodRandomClick();
            var searchAndClickMethod = CreateMethodSearchAndClick();
            var searchWaitClickMethod = CreateMethodSearchWaitClick();
            var searchAndContinueMethod = CreateMethodSearchAndContinue();
            var stopScriptMethod = CretateMethodStopScrit();
            //var logMessageMethod = CretateMethodLogMessage();
            var findAndClick = CreateMethodFindAndClickTesseract();
            var findWaitClick = CreateMethodFindWaitClickTesseract();
            var findAndContinue = CreateMethodFindAndContinueTesseract();
            // text tool box
            var sendText = CreateMethodSendText();
            var randomTextAndSend = CreateMethodRandomTextAndSend();
            var sendTextFromFileDel = CreateMethodSendTextFromFileDel();
            var delTextChar = CreateMethodDelTextChar();
            var delAllText = CreateMethodDelAllText();
            // key button tool box
            var sendKey = CreateMethodSendKey();
            var ctrlA = CreateMethodCtrlA();
            var checkKeyboard = CreateMethodCheckKeyboard();
            //general 
            var onWifi = CreateMethodOnWifi();
            var offWifi = CreateMethodOffWifi();
            var openUrl = CreateMethodOpenUrl();
            var openApp = CreateMethodOpenApp();
            var closeApp = CreateMethodCloseApp();
            var enableApp = CreateMethodEnableApp();
            var disableApp = CreateMethodDisbledApp();
            var installApp = CreateMethodInstallApp();
            var uninstallApp = CreateMethodUninstallApp();
            var clearDataApp = CreateMethodClearDataApp();
            var swipeCloseApp = CreateMethodSwipeCloseApp();
            var commandShell = CreateMethodCommandShell();
            // data change info
            var waitReboot = CreateMethodWaitForReboot();
            var waitInternet = CreateMethodWaitForInternet();
            var pushFile = CreateMethodPushFile();
            var pullFile = CreateMethodPullFile();
            var sendTextRandomFromFile = CreateMethodSendTextRandomFromFile();
            return SyntaxFactory.ClassDeclaration("CommandExecutor")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(
                    deviceIdField,
                    constructor,
                    clickMethod,
                    waitMethod,
                    swipeMethod,
                    findAndClick,
                    findWaitClick,
                    findAndContinue,
                    randomClickMethod,
                    searchAndClickMethod,
                    searchWaitClickMethod,
                    searchAndContinueMethod,
                    stopScriptMethod,
                   // logMessageMethod,
                    sendText,
                    randomTextAndSend,
                    sendTextRandomFromFile,
                    sendTextFromFileDel,
                    delTextChar,
                    delAllText,
                    sendKey,
                    ctrlA,
                    checkKeyboard,
                    onWifi,
                    offWifi,
                    openUrl,
                    waitReboot,
                    waitInternet,
                    pushFile,
                    pullFile,
                    openApp,
                    closeApp,
                    enableApp,
                    disableApp,
                    installApp,
                    uninstallApp,
                    clearDataApp,
                    swipeCloseApp,
                    commandShell,
                    runMethod
                );
        }

        /// <summary>
        /// text tool box
        /// </summary>
        // Phương thức ClickXY(int x, int y)
        public static MethodDeclarationSyntax CreateMethodClickXY()
        {
            var paramX = SyntaxFactory.Parameter(SyntaxFactory.Identifier("x"))
                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)));
            var paramY = SyntaxFactory.Parameter(SyntaxFactory.Identifier("y"))
                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)));

            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                "ClickXY")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                .AddParameterListParameters(paramX, paramY)
                .WithBody(SyntaxFactory.Block(
                    SyntaxFactory.ParseStatement("Console.WriteLine($\"Click tại ({x},{y}) trên thiết bị {_deviceID}\");"),
                    SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} shell input tap {x} {y}\");")
                ));
        }
        // wait 
        public static MethodDeclarationSyntax CreateMethodWait()
        {
            var paramMs = SyntaxFactory.Parameter(SyntaxFactory.Identifier("ms"))
                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)));

            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                "Wait")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                .AddParameterListParameters(paramMs)
                .WithBody(SyntaxFactory.Block(
                    SyntaxFactory.ParseStatement("Console.WriteLine($\"Sleep {ms} ms trên thiết bị {_deviceID}\");"),
                    SyntaxFactory.ParseStatement("Thread.Sleep(ms);")
                ));
        }
        // swipe 
        public static MethodDeclarationSyntax CreateMethodSwipe()
        {
            var paramX = SyntaxFactory.Parameter(SyntaxFactory.Identifier("x1"))
                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)));
            var paramY = SyntaxFactory.Parameter(SyntaxFactory.Identifier("y1"))
               .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)));
            var paramX1 = SyntaxFactory.Parameter(SyntaxFactory.Identifier("x2"))
                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)));
            var paramY1 = SyntaxFactory.Parameter(SyntaxFactory.Identifier("y2"))
               .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)));
            var paramDuration = SyntaxFactory.Parameter(SyntaxFactory.Identifier("duration"))
               .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)))
               .WithDefault(SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(500)
             )
         )
     );

            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                "Swipe")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                .AddParameterListParameters(paramX, paramY, paramX1, paramY1, paramDuration)
                .WithBody(SyntaxFactory.Block(
                        SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} shell input swipe {x1} {y1} {x2} {y2} {duration}\");")
                    ));
        }
        // random click 
        public static MethodDeclarationSyntax CreateMethodRandomClick()
        {
            var paramX = SyntaxFactory.Parameter(SyntaxFactory.Identifier("x"))
                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)));
            var paramX1 = SyntaxFactory.Parameter(SyntaxFactory.Identifier("x1"))
               .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)));
            var paramY = SyntaxFactory.Parameter(SyntaxFactory.Identifier("y"))
               .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)));
            var paramY1 = SyntaxFactory.Parameter(SyntaxFactory.Identifier("y1"))
               .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)));

            return SyntaxFactory.MethodDeclaration(
               SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                "RandomClick")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                .AddParameterListParameters(paramX, paramX1, paramY, paramY1)
                .WithBody(SyntaxFactory.Block(
                        SyntaxFactory.ParseStatement("Random rand = new Random();"),
                        SyntaxFactory.ParseStatement("int randX = rand.Next(x, x1);"),
                        SyntaxFactory.ParseStatement("int randY = rand.Next(y, y1);"),
                        SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} shell input tap {randX} {randY}\");")
                    ));
        }
        // find text and click
        public static MethodDeclarationSyntax CreateMethodSearchAndClick()
        {
            var paramText = SyntaxFactory.Parameter(SyntaxFactory.Identifier("text"))
                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)));

            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                "SearchAndClick")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(paramText)
                .WithBody(SyntaxFactory.Block(
                    SyntaxFactory.ParseStatement("System.IO.Directory.CreateDirectory($\"./{_deviceID}\");"),
                    SyntaxFactory.ParseStatement("ADBService.EnsureFileDeleted($\"./{_deviceID}/window_dump.xml\");"),
                    SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} shell uiautomator dump /sdcard/window_dump.xml\");"),
                    SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} pull /sdcard/window_dump.xml ./{_deviceID}/window_dump.xml\");"),
                    SyntaxFactory.ParseStatement("ADBService.WaitFileExists($\"./{_deviceID}/window_dump.xml\");"),
                    SyntaxFactory.ParseStatement("var bounds = ADBService.FindBoundsByText($\"./{_deviceID}/window_dump.xml\", text);"),
                    SyntaxFactory.ParseStatement("var (x, y) = ADBService.ParseCenter(bounds);"),
                    SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} shell input tap {x} {y}\");")
                ));
        }

        public static MethodDeclarationSyntax CreateMethodSearchWaitClick()
        {
            var paramText = SyntaxFactory.Parameter(SyntaxFactory.Identifier("text"))
                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)));
            var paramMs = SyntaxFactory.Parameter(SyntaxFactory.Identifier("ms"))
             .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)));

            return SyntaxFactory.MethodDeclaration(
                  SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                "SearchWaitClick")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
                .AddParameterListParameters(paramText, paramMs)
                .WithBody(SyntaxFactory.Block(
                    SyntaxFactory.ParseStatement("System.IO.Directory.CreateDirectory($\"./{_deviceID}\");"),
                    SyntaxFactory.ParseStatement("ADBService.EnsureFileDeleted($\"./{_deviceID}/window_dump.xml\");"),
                    SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} shell uiautomator dump /sdcard/window_dump.xml\");"),
                    SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} pull /sdcard/window_dump.xml ./{_deviceID}/window_dump.xml\");"),
                    SyntaxFactory.ParseStatement($"ADBService.WaitFileExists($\"./{{_deviceID}}/window_dump.xml\");"),
                    SyntaxFactory.ParseStatement("var bounds = ADBService.FindBoundsByText($\"./{_deviceID}/window_dump.xml\", text);"),
                    SyntaxFactory.ParseStatement("var (x, y) = ADBService.ParseCenter(bounds);"),
                    SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} shell input tap {x} {y}\");"),
                    SyntaxFactory.ParseStatement("Thread.Sleep(ms);")
                    ));
        }
        // method bool Search and continue
        public static MethodDeclarationSyntax CreateMethodSearchAndContinue()
        {
            var paramText = SyntaxFactory.Parameter(SyntaxFactory.Identifier("text"))
                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)));

            return SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                    "SearchAndContinue")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(paramText)
                .WithBody(SyntaxFactory.Block(
                    SyntaxFactory.ParseStatement("System.IO.Directory.CreateDirectory($\"./{_deviceID}\");"),
                    SyntaxFactory.ParseStatement("ADBService.EnsureFileDeleted($\"./{_deviceID}/window_dump.xml\");"),
                    SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} shell uiautomator dump /sdcard/window_dump.xml\");"),
                    SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} pull /sdcard/window_dump.xml ./{_deviceID}/window_dump.xml\");"),
                    SyntaxFactory.ParseStatement($"ADBService.WaitFileExists($\"./{{_deviceID}}/window_dump.xml\");"),
                    SyntaxFactory.ParseStatement("var bounds = ADBService.FindBoundsByText($\"./{_deviceID}/window_dump.xml\", text);"),
                    SyntaxFactory.ParseStatement(
                        @"if (bounds == null)
                        {
                            Console.WriteLine($""Text '{text}' không tìm thấy trên thiết bị {_deviceID}."");
                            return false;
                        }"
                    ),
                    SyntaxFactory.ParseStatement("var (x, y) = ADBService.ParseCenter(bounds);"),
                    SyntaxFactory.ParseStatement("return true;")
                ));
        }
        // stop script 
        public static MethodDeclarationSyntax CretateMethodStopScrit()
        {
            return SyntaxFactory.MethodDeclaration(
                   SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                   "StopScript")
               .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
               .AddParameterListParameters()
               .WithBody(SyntaxFactory.Block(
                   SyntaxFactory.ParseStatement("return;")
               ));
        }
        // log message 
        public static MethodDeclarationSyntax CretateMethodLogMessage()
        {
            var paramText = SyntaxFactory.Parameter(SyntaxFactory.Identifier("text"))
               .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)));

            return SyntaxFactory.MethodDeclaration(
                   SyntaxFactory.IdentifierName("Task"), // Trả về Task cho async method
                   "Log")
               .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.AsyncKeyword))
               .AddParameterListParameters(paramText)
               .WithBody(SyntaxFactory.Block(
                   SyntaxFactory.ParseStatement(@"await _view.UpdateProgressGridView($""{_deviceID}"", text, 5);")
               ));
        }
        // FindAndClick
        public static MethodDeclarationSyntax CreateMethodFindAndClickTesseract()
        {
            var paramText = SyntaxFactory.Parameter(SyntaxFactory.Identifier("text"))
                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)));

            return SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    "FindAndClick")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(paramText)
                .WithBody(SyntaxFactory.Block(
                    SyntaxFactory.ParseStatement(@"ADBService.CaptureAndPullScreenshot($""{_deviceID}"");"),
                    SyntaxFactory.ParseStatement("System.Threading.Thread.Sleep(1000);"),

                    SyntaxFactory.ParseStatement(@"var imagePath = $""./{_deviceID}/{_deviceID}_Tesseract.png"";"),

                    // Xử lý Tesseract để tìm tọa độ
                    SyntaxFactory.ParseStatement(@"using (var engine = new Tesseract.TesseractEngine(@""./Resources"", ""eng"", Tesseract.EngineMode.Default))"),
                    SyntaxFactory.ParseStatement(@"using (var img = Tesseract.Pix.LoadFromFile(imagePath))"),
                    SyntaxFactory.ParseStatement(@"using (var page = engine.Process(img))"),
                    SyntaxFactory.ParseStatement(@"using (var iter = page.GetIterator())"),
                    SyntaxFactory.ParseStatement(@"{"),
                    SyntaxFactory.ParseStatement(@"    iter.Begin();"),
                    SyntaxFactory.ParseStatement(@"    do"),
                    SyntaxFactory.ParseStatement(@"    {"),
                    SyntaxFactory.ParseStatement(@"        string word = iter.GetText(Tesseract.PageIteratorLevel.Word);"),
                    SyntaxFactory.ParseStatement(@"        if (!string.IsNullOrEmpty(word) && word.Trim().ToLower().Contains(text.ToLower()))"),
                    SyntaxFactory.ParseStatement(@"        {"),
                    SyntaxFactory.ParseStatement(@"            if (iter.TryGetBoundingBox(Tesseract.PageIteratorLevel.Word, out var rect))"),
                    SyntaxFactory.ParseStatement(@"            {"),
                    SyntaxFactory.ParseStatement(@"                int centerX = rect.X1 + (rect.Width / 2);"),
                    SyntaxFactory.ParseStatement(@"                int centerY = rect.Y1 + (rect.Height / 2);"),
                    SyntaxFactory.ParseStatement(@"                ADBService.ExecuteAdbCommand($"" -s {_deviceID} shell input tap {centerX} {centerY}"");"),
                    SyntaxFactory.ParseStatement(@"                return;"),
                    SyntaxFactory.ParseStatement(@"            }"),
                    SyntaxFactory.ParseStatement(@"        }"),
                    SyntaxFactory.ParseStatement(@"    } while (iter.Next(Tesseract.PageIteratorLevel.Word));"),
                    SyntaxFactory.ParseStatement(@"}")
                ));
        }


        // FindWaitClick
        public static MethodDeclarationSyntax CreateMethodFindWaitClickTesseract()
        {
            var paramText = SyntaxFactory.Parameter(SyntaxFactory.Identifier("text"))
                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)));
            var paramWait = SyntaxFactory.Parameter(SyntaxFactory.Identifier("wait"))
                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)))
                .WithDefault(SyntaxFactory.EqualsValueClause(
                    SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(100))));

            return SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    "FindWaitClick")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(paramText, paramWait)
                .WithBody(SyntaxFactory.Block(
                    SyntaxFactory.ParseStatement(@"ADBService.CaptureAndPullScreenshot($""{_deviceID}"");"),
                    SyntaxFactory.ParseStatement("System.Threading.Thread.Sleep(1000);"),

                    SyntaxFactory.ParseStatement(@"var imagePath = $""./{_deviceID}/{_deviceID}_Tesseract.png"";"),

                    // Xử lý Tesseract để tìm tọa độ
                    SyntaxFactory.ParseStatement(@"using (var engine = new Tesseract.TesseractEngine(@""./Resources"", ""eng"", Tesseract.EngineMode.Default))"),
                    SyntaxFactory.ParseStatement(@"using (var img = Tesseract.Pix.LoadFromFile(imagePath))"),
                    SyntaxFactory.ParseStatement(@"using (var page = engine.Process(img))"),
                    SyntaxFactory.ParseStatement(@"using (var iter = page.GetIterator())"),
                    SyntaxFactory.ParseStatement(@"{"),
                    SyntaxFactory.ParseStatement(@"    iter.Begin();"),
                    SyntaxFactory.ParseStatement(@"    do"),
                    SyntaxFactory.ParseStatement(@"    {"),
                    SyntaxFactory.ParseStatement(@"        string word = iter.GetText(Tesseract.PageIteratorLevel.Word);"),
                    SyntaxFactory.ParseStatement(@"        if (!string.IsNullOrEmpty(word) && word.Trim().ToLower().Contains(text.ToLower()))"),
                    SyntaxFactory.ParseStatement(@"        {"),
                    SyntaxFactory.ParseStatement(@"            if (iter.TryGetBoundingBox(Tesseract.PageIteratorLevel.Word, out var rect))"),
                    SyntaxFactory.ParseStatement(@"            {"),
                    SyntaxFactory.ParseStatement(@"                int centerX = rect.X1 + (rect.Width / 2);"),
                    SyntaxFactory.ParseStatement(@"                int centerY = rect.Y1 + (rect.Height / 2);"),
                    SyntaxFactory.ParseStatement(@"                System.Threading.Thread.Sleep(wait);"),
                    SyntaxFactory.ParseStatement(@"                ADBService.ExecuteAdbCommand($"" -s {_deviceID} shell input tap {centerX} {centerY}"");"),
                    SyntaxFactory.ParseStatement(@"                return;"),
                    SyntaxFactory.ParseStatement(@"            }"),
                    SyntaxFactory.ParseStatement(@"        }"),
                    SyntaxFactory.ParseStatement(@"    } while (iter.Next(Tesseract.PageIteratorLevel.Word));"),
                    SyntaxFactory.ParseStatement(@"}")
                ));
        }
        // FindAndContinue
        public static MethodDeclarationSyntax CreateMethodFindAndContinueTesseract()
        {
            var paramText = SyntaxFactory.Parameter(SyntaxFactory.Identifier("text"))
                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)));

            return SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                    "FindAndContinue")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(paramText)
                .WithBody(SyntaxFactory.Block(
                    SyntaxFactory.ParseStatement(@"ADBService.CaptureAndPullScreenshot($""{_deviceID}"");"),
                    SyntaxFactory.ParseStatement("System.Threading.Thread.Sleep(1000);"),

                    SyntaxFactory.ParseStatement(@"var imagePath = $""./{_deviceID}/{_deviceID}_Tesseract.png"";"),

                    SyntaxFactory.ParseStatement(@"using (var engine = new Tesseract.TesseractEngine(@""./Resources"", ""eng"", Tesseract.EngineMode.Default))"),
                    SyntaxFactory.ParseStatement(@"using (var img = Tesseract.Pix.LoadFromFile(imagePath))"),
                    SyntaxFactory.ParseStatement(@"using (var page = engine.Process(img))"),
                    SyntaxFactory.ParseStatement(@"using (var iter = page.GetIterator())"),
                    SyntaxFactory.ParseStatement(@"{"),
                    SyntaxFactory.ParseStatement(@"    iter.Begin();"),
                    SyntaxFactory.ParseStatement(@"    do"),
                    SyntaxFactory.ParseStatement(@"    {"),
                    SyntaxFactory.ParseStatement(@"        string word = iter.GetText(Tesseract.PageIteratorLevel.Word);"),
                    SyntaxFactory.ParseStatement(@"        if (!string.IsNullOrEmpty(word) && word.Trim().ToLower().Contains(text.ToLower()))"),
                    SyntaxFactory.ParseStatement(@"        {"),
                    SyntaxFactory.ParseStatement(@"            if (iter.TryGetBoundingBox(Tesseract.PageIteratorLevel.Word, out var rect))"),
                    SyntaxFactory.ParseStatement(@"            {"),
                    SyntaxFactory.ParseStatement(@"                int centerX = rect.X1 + (rect.Width / 2);"),
                    SyntaxFactory.ParseStatement(@"                int centerY = rect.Y1 + (rect.Height / 2);"),
                    SyntaxFactory.ParseStatement(@"                return true;"),
                    SyntaxFactory.ParseStatement(@"            }"),
                    SyntaxFactory.ParseStatement(@"        }"),
                    SyntaxFactory.ParseStatement(@"    } while (iter.Next(Tesseract.PageIteratorLevel.Word));"),
                    SyntaxFactory.ParseStatement(@"}"),

                    SyntaxFactory.ParseStatement("return false;")
                ));
        }

        /// <summary>
        /// text tool box
        /// </summary>
        // send text 
        public static MethodDeclarationSyntax CreateMethodSendText()
        {
            var paramText = SyntaxFactory.Parameter(SyntaxFactory.Identifier("text"))
                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)));


            return SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    "SendText")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(paramText)
                .WithBody(SyntaxFactory.Block(
                        SyntaxFactory.ParseStatement("var input = ADBService.EscapeAdbInputText(text);"),
                        SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} shell input text {input}\");")



                    ));
        }
        // random text and send text 15 
        public static MethodDeclarationSyntax CreateMethodRandomTextAndSend()
        {
            var paramRandomCount = SyntaxFactory.Parameter(SyntaxFactory.Identifier("count"))
             .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)))
             .WithDefault(SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(15)
             )));

            return SyntaxFactory.MethodDeclaration(
                  SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                  "RandomTextAndSend")
              .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
              .AddParameterListParameters(paramRandomCount)
              .WithBody(SyntaxFactory.Block(
                  SyntaxFactory.ParseStatement("var input = ADBService.RandomString(count);"),
                  SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} shell input text {input}\");")
                  ));
        }
        // send text file and delete
        public static MethodDeclarationSyntax CreateMethodSendTextFromFileDel()
        {
            var paramUrlFile = SyntaxFactory.Parameter(SyntaxFactory.Identifier("url"))
                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)));

            var statements = new List<StatementSyntax>();

            statements.Add(SyntaxFactory.ParseStatement("System.IO.Directory.CreateDirectory(\"./Resources/script/file\");"));
            statements.Add(SyntaxFactory.ParseStatement("if (!System.IO.File.Exists($\"./Resources/script/file/{url}\")) { Console.WriteLine($\"[Log] File không tồn tại: {url}\"); return; }"));

            statements.Add(SyntaxFactory.ParseStatement("var lines = System.IO.File.ReadAllLines($\"./Resources/script/file/{url}\");"));
            statements.Add(SyntaxFactory.ParseStatement("Console.WriteLine($\"[Log] Đã đọc {lines.Length} dòng từ file.\");"));

            statements.Add(SyntaxFactory.ParseStatement("if (lines.Length == 0) { Console.WriteLine(\"[Log] File rỗng, không có dòng nào để gửi.\"); return; }"));

            statements.Add(SyntaxFactory.ParseStatement("var firstLine = lines[0];"));
            statements.Add(SyntaxFactory.ParseStatement("Console.WriteLine($\"[Log] Dòng đầu tiên sẽ gửi: {firstLine}\");"));

            statements.Add(SyntaxFactory.ParseStatement("SendText(firstLine);"));
            statements.Add(SyntaxFactory.ParseStatement("Console.WriteLine(\"[Log] Đã gửi dòng đầu tiên.\");"));

            statements.Add(SyntaxFactory.ParseStatement("var remainingLines = lines.Skip(1).ToArray();"));
            statements.Add(SyntaxFactory.ParseStatement("System.IO.File.WriteAllLines($\"./Resources/script/file/{url}\", remainingLines);"));
            statements.Add(SyntaxFactory.ParseStatement("Console.WriteLine($\"[Log] Đã xóa dòng đầu và ghi lại file. Còn lại {remainingLines.Length} dòng.\");"));

            return SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    "SendTextFromFileDel")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(paramUrlFile)
                .WithBody(SyntaxFactory.Block(statements));
        }
        // xóa 1 ký tự
        public static MethodDeclarationSyntax CreateMethodDelTextChar()
        {
            var paramCount = SyntaxFactory.Parameter(SyntaxFactory.Identifier("count"))
                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)));

            var statements = new List<StatementSyntax>();

            var forStatement = SyntaxFactory.ForStatement(
                    SyntaxFactory.Block(
                        SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} shell input keyevent 67\");")
                    ))
                .WithDeclaration(
                    SyntaxFactory.VariableDeclaration(
                        SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)))
                    .AddVariables(SyntaxFactory.VariableDeclarator("i")
                        .WithInitializer(SyntaxFactory.EqualsValueClause(SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0))))))
                .WithCondition(
                    SyntaxFactory.BinaryExpression(SyntaxKind.LessThanExpression,
                        SyntaxFactory.IdentifierName("i"),
                        SyntaxFactory.IdentifierName("count")))
                .WithIncrementors(
                    SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
                        SyntaxFactory.PostfixUnaryExpression(SyntaxKind.PostIncrementExpression, SyntaxFactory.IdentifierName("i"))));

            statements.Add(forStatement);

            return SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                    "DelTextChar")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(paramCount)
                .WithBody(SyntaxFactory.Block(statements));
        }
        // xóa toàn bộ
        public static MethodDeclarationSyntax CreateMethodDelAllText()
        {
            return SyntaxFactory.MethodDeclaration(
                  SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                  "DelAllText")
              .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
              .AddParameterListParameters()
              .WithBody(SyntaxFactory.Block(
                  SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} shell input keycombination 113 29\");"),
                  SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} shell input keyevent 67\");")
                  ));
        }
        // send text random to form
        public static MethodDeclarationSyntax CreateMethodSendTextRandomFromFile()
        {
            var paramUrlFile = SyntaxFactory.Parameter(SyntaxFactory.Identifier("url"))
                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)));

            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                "SendTextRandomFromFile")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(paramUrlFile)
                .WithBody(SyntaxFactory.Block(
                       SyntaxFactory.ParseStatement("string[] lines = File.ReadAllLines(url);"),
                       SyntaxFactory.ParseStatement("var nonEmptyLines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToArray();"),
                       SyntaxFactory.ParseStatement("if (nonEmptyLines.Length == 0) { Console.WriteLine(\"File không có dòng hợp lệ.\"); return; }"),
                       SyntaxFactory.ParseStatement("Random rnd = new Random();"),
                       SyntaxFactory.ParseStatement("string randomLine = nonEmptyLines[rnd.Next(nonEmptyLines.Length)];"),
                       SyntaxFactory.ParseStatement("Console.WriteLine(\"Dòng được chọn ngẫu nhiên: \" + randomLine);"),
                       SyntaxFactory.ParseStatement("var input = ADBService.EscapeAdbInputText(randomLine);"),
                       SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} shell input text {input}\");")
                    ));
        }
        /// <summary>
        /// Key button
        /// </summary>
        /// <returns></returns>
        // send key
        public static MethodDeclarationSyntax CreateMethodSendKey()
        {
            var paramKey = SyntaxFactory.Parameter(SyntaxFactory.Identifier("key"))
                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)));

            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "SendKey")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(paramKey)
                .WithBody(SyntaxFactory.Block(
                     SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} shell input keyevent {key}\");")
                    ));
        }
        // ctrl A
        public static MethodDeclarationSyntax CreateMethodCtrlA()
        {
            return SyntaxFactory.MethodDeclaration(
                  SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                  "CtrlA")
              .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
              .AddParameterListParameters()
              .WithBody(SyntaxFactory.Block(
                  SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} shell input keycombination 113 29\");")
                  ));
        }
        // check key board
        public static MethodDeclarationSyntax CreateMethodCheckKeyboard()
        {
            return SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                    "CheckKeyboard")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword)) // ❌ Không thêm Static
                .AddParameterListParameters()
                .WithBody(SyntaxFactory.Block(
                    SyntaxFactory.LocalDeclarationStatement(
                        SyntaxFactory.VariableDeclaration(
                            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)))
                        .AddVariables(
                            SyntaxFactory.VariableDeclarator("output")
                                .WithInitializer(SyntaxFactory.EqualsValueClause(
                                    SyntaxFactory.ParseExpression("ADBService.ExecuteAdbCommandString($\" -s {_deviceID} shell dumpsys input_method\")")
                                ))
                        )
                    ),
                    SyntaxFactory.ReturnStatement(
                        SyntaxFactory.BinaryExpression(
                            SyntaxKind.LogicalOrExpression,
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName("output"),
                                    SyntaxFactory.IdentifierName("Contains")
                                )
                            ).WithArgumentList(SyntaxFactory.ArgumentList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                                            SyntaxFactory.Literal("mInputShown=true"))
                                    )
                                )
                            )),
                            SyntaxFactory.InvocationExpression(
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName("output"),
                                    SyntaxFactory.IdentifierName("Contains")
                                )
                            ).WithArgumentList(SyntaxFactory.ArgumentList(
                                SyntaxFactory.SingletonSeparatedList(
                                    SyntaxFactory.Argument(
                                        SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                                            SyntaxFactory.Literal("mIsInputViewShown=true"))
                                    )
                                )
                            ))
                        )
                    )
                ));
        }
        /// <summary>
        /// data change info
        /// </summary>
        // backup

        // restore

        // login gmail

        // load play store

        // change info

        // change sim

        // wipe Account 

        //Wait rebot
        public static MethodDeclarationSyntax CreateMethodWaitForReboot()
        {
            return SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                    "WaitForReboot")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters()
                .WithBody(
                    SyntaxFactory.Block(
                        SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommandString($\" wait-for-device\");"),
                        SyntaxFactory.WhileStatement(
                            SyntaxFactory.PrefixUnaryExpression(
                                SyntaxKind.LogicalNotExpression,
                                SyntaxFactory.InvocationExpression(
                                    SyntaxFactory.MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.IdentifierName("ADBService"),
                                                SyntaxFactory.IdentifierName("ExecuteAdbCommandString")
                                            )
                                        ).WithArgumentList(
                                            SyntaxFactory.ArgumentList(
                                                SyntaxFactory.SingletonSeparatedList(
                                                    SyntaxFactory.Argument(
                                                        SyntaxFactory.ParseExpression($"\" -s {{_deviceID}} shell getprop sys.boot_completed\"")
                                                    )
                                                )
                                            )
                                        ),
                                        SyntaxFactory.IdentifierName("Contains")
                                    )
                                ).WithArgumentList(
                                    SyntaxFactory.ArgumentList(
                                        SyntaxFactory.SingletonSeparatedList(
                                            SyntaxFactory.Argument(
                                                SyntaxFactory.LiteralExpression(
                                                    SyntaxKind.StringLiteralExpression,
                                                    SyntaxFactory.Literal("1")
                                                )
                                            )
                                        )
                                    )
                                )
                            ),
                            SyntaxFactory.Block(
                                SyntaxFactory.ParseStatement("System.Threading.Thread.Sleep(1000);")
                            )
                        ),
                        SyntaxFactory.ParseStatement("return true;")
                    )
                );
        }

        // wait internet 
        public static MethodDeclarationSyntax CreateMethodWaitForInternet()
        {
            return SyntaxFactory.MethodDeclaration(
                    SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)),
                    "WaitInternet")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters()
                .WithBody(
                    SyntaxFactory.Block(
                        SyntaxFactory.WhileStatement(
                            SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression),
                            SyntaxFactory.Block(
                                SyntaxFactory.LocalDeclarationStatement(
                                    SyntaxFactory.VariableDeclaration(
                                        SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)))
                                    .AddVariables(
                                        SyntaxFactory.VariableDeclarator("output")
                                            .WithInitializer(SyntaxFactory.EqualsValueClause(
                                                SyntaxFactory.ParseExpression("ADBService.ExecuteAdbCommandString($\" -s {_deviceID} shell ping -c 1 8.8.8.8\")")
                                            ))
                                    )
                                ),
                                SyntaxFactory.IfStatement(
                                    SyntaxFactory.BinaryExpression(
                                        SyntaxKind.LogicalAndExpression,
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.IdentifierName("output"),
                                                SyntaxFactory.IdentifierName("Contains")
                                            )
                                        ).WithArgumentList(SyntaxFactory.ArgumentList(
                                            SyntaxFactory.SingletonSeparatedList(
                                                SyntaxFactory.Argument(
                                                    SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                                                        SyntaxFactory.Literal("icmp_seq=1"))
                                                )
                                            )
                                        )),
                                        SyntaxFactory.InvocationExpression(
                                            SyntaxFactory.MemberAccessExpression(
                                                SyntaxKind.SimpleMemberAccessExpression,
                                                SyntaxFactory.IdentifierName("output"),
                                                SyntaxFactory.IdentifierName("Contains")
                                            )
                                        ).WithArgumentList(SyntaxFactory.ArgumentList(
                                            SyntaxFactory.SingletonSeparatedList(
                                                SyntaxFactory.Argument(
                                                    SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression,
                                                        SyntaxFactory.Literal("1 packets transmitted, 1 received"))
                                                )
                                            )
                                        ))
                                    ),
                                    SyntaxFactory.Block(
                                        SyntaxFactory.ParseStatement("return true;")
                                    )
                                ),
                                SyntaxFactory.ParseStatement("System.Threading.Thread.Sleep(2000);")
                            )
                        ),
                        SyntaxFactory.ParseStatement("return false;")
                    )
                );
        }
        // push file to phone 
        public static MethodDeclarationSyntax CreateMethodPushFile()
        {
            var paramUrlFromPc = SyntaxFactory.Parameter(SyntaxFactory.Identifier("FromPC"))
                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)));
            var paramUrlFromPhone = SyntaxFactory.Parameter(SyntaxFactory.Identifier("FromPhone"))
               .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)));

            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "PushFile")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(paramUrlFromPc, paramUrlFromPhone)
                .WithBody(SyntaxFactory.Block(
                     SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} push {FromPC} {FromPhone}\");")
                    ));
        }
        // pull file to pc
        public static MethodDeclarationSyntax CreateMethodPullFile()
        {
            var paramUrlFromPc = SyntaxFactory.Parameter(SyntaxFactory.Identifier("FromPC"))
                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)));
            var paramUrlFromPhone = SyntaxFactory.Parameter(SyntaxFactory.Identifier("FromPhone"))
               .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)));

            return SyntaxFactory.MethodDeclaration(
                SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)), "PullFile")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(paramUrlFromPhone, paramUrlFromPc)
                .WithBody(SyntaxFactory.Block(
                     SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} pull {FromPhone} {FromPC}\");")
                    ));
        }
        /// <summary>
        /// general
        /// </summary>
        // wifi on
        public static MethodDeclarationSyntax CreateMethodOnWifi()
        {
            return SyntaxFactory.MethodDeclaration(
                  SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                  "WiFiON")
              .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
              .AddParameterListParameters()
              .WithBody(SyntaxFactory.Block(
                  SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} shell svc wifi enable\");")
                  ));
        }
        // wifi off
        public static MethodDeclarationSyntax CreateMethodOffWifi()
        {
            return SyntaxFactory.MethodDeclaration(
                  SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                  "WiFiOFF")
              .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
              .AddParameterListParameters()
              .WithBody(SyntaxFactory.Block(
                  SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} shell svc wifi disable\");")
                  ));
        }
        // open url
        public static MethodDeclarationSyntax CreateMethodOpenUrl()
        {
            var paramUrl = SyntaxFactory.Parameter(SyntaxFactory.Identifier("url"))
               .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)));

            return SyntaxFactory.MethodDeclaration(
                  SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                  "OpenURL")
              .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
              .AddParameterListParameters(paramUrl)
              .WithBody(SyntaxFactory.Block(
                  SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} shell am start -a android.intent.action.VIEW -d {url}\");")
                  ));
        }
        // on Bproxy

        // auto proxy

        // check sim

        // command (shell)
        public static MethodDeclarationSyntax CreateMethodCommandShell()
        {
            var paramShell = SyntaxFactory.Parameter(SyntaxFactory.Identifier("shell"))
                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)));


            return SyntaxFactory.MethodDeclaration(
                  SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                  "RunCommandShell")
              .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
              .AddParameterListParameters(paramShell)
              .WithBody(SyntaxFactory.Block(
                  SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} shell {shell}\");")
                  ));
        }
        // open app
        public static MethodDeclarationSyntax CreateMethodOpenApp()
        {
            var paramPackage = SyntaxFactory.Parameter(SyntaxFactory.Identifier("package"))
                                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)));

            return SyntaxFactory.MethodDeclaration(
                  SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                  "OpenApp")
              .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
              .AddParameterListParameters(paramPackage)
              .WithBody(SyntaxFactory.Block(
                  SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} shell monkey -p {package} -c android.intent.category.LAUNCHER 1\");")
                  ));
        }
        // close app
        public static MethodDeclarationSyntax CreateMethodCloseApp()
        {
            var paramPackage = SyntaxFactory.Parameter(SyntaxFactory.Identifier("package"))
                                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)));

            return SyntaxFactory.MethodDeclaration(
                  SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                  "CloseApp")
              .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
              .AddParameterListParameters(paramPackage)
              .WithBody(SyntaxFactory.Block(
                  SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} shell am force-stop {package}\");")
                  ));
        }
        //enable app
        public static MethodDeclarationSyntax CreateMethodEnableApp()
        {
            var paramPackage = SyntaxFactory.Parameter(SyntaxFactory.Identifier("package"))
                                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)));

            return SyntaxFactory.MethodDeclaration(
                  SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                  "EnableApp")
              .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
              .AddParameterListParameters(paramPackage)
              .WithBody(SyntaxFactory.Block(
                  SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} shell pm enable {package}\");")
                  ));
        }
        // disable app 
        public static MethodDeclarationSyntax CreateMethodDisbledApp()
        {
            var paramPackage = SyntaxFactory.Parameter(SyntaxFactory.Identifier("package"))
                                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)));

            return SyntaxFactory.MethodDeclaration(
                  SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                  "DisbledApp")
              .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
              .AddParameterListParameters(paramPackage)
              .WithBody(SyntaxFactory.Block(
                  SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} shell pm disable {package}\");")
                  ));
        }
        // install app 
        public static MethodDeclarationSyntax CreateMethodInstallApp()
        {
            var paramPath = SyntaxFactory.Parameter(SyntaxFactory.Identifier("path"))
                                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)));

            return SyntaxFactory.MethodDeclaration(
                  SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                  "InstallApp")
              .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
              .AddParameterListParameters(paramPath)
              .WithBody(SyntaxFactory.Block(
                  SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} install {path}\");")
                  ));
        }
        // uninstall app 
        public static MethodDeclarationSyntax CreateMethodUninstallApp()
        {
            var paramPath = SyntaxFactory.Parameter(SyntaxFactory.Identifier("path"))
                                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)));

            return SyntaxFactory.MethodDeclaration(
                  SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                  "UninstallApp")
              .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
              .AddParameterListParameters(paramPath)
              .WithBody(SyntaxFactory.Block(
                  SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} uninstall {path}\");")
                  ));
        }
        // clear data app
        public static MethodDeclarationSyntax CreateMethodClearDataApp()
        {
            var paramPackage = SyntaxFactory.Parameter(SyntaxFactory.Identifier("package"))
                                .WithType(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword)));

            return SyntaxFactory.MethodDeclaration(
                  SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                  "ClearDataApp")
              .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
              .AddParameterListParameters(paramPackage)
              .WithBody(SyntaxFactory.Block(
                  SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} shell pm clear {package}\");")
                  ));
        }
        // SwipeCloseApp
        public static MethodDeclarationSyntax CreateMethodSwipeCloseApp()
        {
            return SyntaxFactory.MethodDeclaration(
                  SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.VoidKeyword)),
                  "SwipeCloseApp")
              .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
              .AddParameterListParameters()
              .WithBody(SyntaxFactory.Block(
                  SyntaxFactory.ParseStatement("ADBService.ExecuteAdbCommand($\" -s {_deviceID} shell input keyevent 3\");")
                  ));
        }
    }
}
