using NStack;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Terminal.Gui;
using Rune = System.Rune;
//using ConsoleAppFramework;
using System.Threading;
//using System.Diagnostics;

namespace SmartThingsTerminal
{
    class Program
    {
        private static Toplevel _top;
        private static MenuBar _menu;
        private static int _nameColumnWidth;
        private static FrameView _leftPane;
        private static List<string> _categories;
        private static ListView _categoryListView;
        private static FrameView _rightPane;
        private static FrameView _appTitlePane;
        //private static List<Type> _scenarios;
        private static List<string> _scenarios;
        private static ListView _scenarioListView;
        private static StatusBar _statusBar;
        private static StatusItem _capslock;
        private static StatusItem _numlock;
        private static StatusItem _scrolllock;
        private static int _categoryListViewItem;
        private static int _scenarioListViewItem;
        private static Scenario _runningScenario = null;
        private static bool _useSystemConsole = false;
        private static SmartThingsClient _stClient;

        static void Main(string[] args)
        {
            /*
                Startup startup = new Startup();

                if (startup.Configure(args))
                {
                    Init(startup.Options);
                }
            */
            Init();

        }

        //private static void Init(Options opts)
        private static void Init()
        {
            Console.Title = "SmartThings Terminal";
            //_stClient = new SmartThingsClient(opts.AccessToken);
            _stClient = new SmartThingsClient("{accesstoken}");

            if (Debugger.IsAttached)
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");


            //if (opts.ApiName != null)
            //{
            //}

            Scenario scenario;
            while ((scenario = GetScenarioToRun()) != null)
            {
                Application.UseSystemConsole = _useSystemConsole;
                Application.Init();
                scenario.Init(Application.Top, _baseColorScheme, _stClient);
                scenario.Setup();
                scenario.Run();
            }
            Application.Shutdown();
        }

        private static void Init2()
        {
            Console.Title = "SmartThings Terminal";
            //_stClient = new SmartThingsClient(opts.AccessToken);
            _stClient = new SmartThingsClient("{accesstoken}");

            if (Debugger.IsAttached)
                CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.GetCultureInfo("en-US");



            Scenario scenario;
            while ((scenario = GetScenarioToRun()) != null)
            {
                Application.UseSystemConsole = _useSystemConsole;
                Application.Init();
                scenario.Init(Application.Top, _baseColorScheme, _stClient);
                scenario.Setup();
                scenario.Run();
            }

            //Application.Shutdown();
        }
        /// <summary>
        /// This shows the selection UI. Each time it is run, it calls Application.Init to reset everything.
        /// </summary>
        /// <returns></returns>
        private static Scenario GetScenarioToRun()
        {
            Application.UseSystemConsole = false;
            Application.Init();

            // Set this here because not initilzied until driver is loaded
            _baseColorScheme = Colors.Base;

            _menu = MenuHelper.GetStandardMenuBar(_baseColorScheme);

            //_leftPane = new FrameView("API")
            _leftPane = new FrameView("SITE LOC")
            {
                X = 0,
                Y = 1, // for menu
                Width = 30,
                Height = Dim.Fill(1),
                CanFocus = true,
            };

            //GS
            //_categories = Scenario.GetAllCategories().OrderBy(c => c).ToList();
            _categories = new List<string>()
            {
                "pref",
                "kcb",
                "kozu",
        "hogehoge"
            };

            _categoryListView = new ListView(_categories)
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(0),
                Height = Dim.Fill(0),
                AllowsMarking = false,
                CanFocus = true
            };
            _categoryListView.OpenSelectedItem += (a) =>
            {

                _rightPane.SetFocus();
            };

            _categoryListView.SelectedItemChanged += CategoryListView_SelectedChanged;
            _leftPane.Add(_categoryListView);

            Label appNameView = new Label() { X = 0, Y = 0, Height = Dim.Fill(), Width = Dim.Fill(), CanFocus = false, Text = MenuHelper.GetAppTitle() };
            _appTitlePane = new FrameView()
            {
                X = 30,
                Y = 1, // for menu
                Width = Dim.Fill(),
                Height = 9,
                CanFocus = false,
            };
            _appTitlePane.Add(appNameView);

            _rightPane = new FrameView("API Description")
            {
                X = 30,
                //Y = 1, // for menu
                Y = Pos.Bottom(_appTitlePane),
                Width = Dim.Fill(),
                Height = Dim.Fill(1),
                CanFocus = true,
            };

            //_nameColumnWidth = Scenario.ScenarioMetadata.GetName(_scenarios.OrderByDescending(t => Scenario.ScenarioMetadata.GetName(t).Length).FirstOrDefault()).Length;
            _scenarioListView = new ListView()
            {
                X = 0,
                Y = 0,
                Width = Dim.Fill(0),
                Height = Dim.Fill(0),
                AllowsMarking = false,
                CanFocus = true,
            };

            _scenarioListView.OpenSelectedItem += _scenarioListView_OpenSelectedItem;
            _rightPane.Add(_scenarioListView);

            //_categoryListView.SelectedItem = _categoryListViewItem;
            _categoryListView.OnSelectedChanged();

            _capslock = new StatusItem(Key.CharMask, "Caps", null);
            _numlock = new StatusItem(Key.CharMask, "Num", null);
            _scrolllock = new StatusItem(Key.CharMask, "Scroll", null);

            _statusBar = new StatusBar(new StatusItem[] {
                _capslock,
                _numlock,
                _scrolllock,
                new StatusItem(Key.F5, "~F5~ Refresh Data", () => {
                    _stClient.ResetData();
                }),
                new StatusItem(Key.F9, "~F9~ Menu", () => {
                    _stClient.ResetData();
                }),
                new StatusItem(Key.Q | Key.CtrlMask, "~CTRL-Q~ Quit", () => {



                    if (_runningScenario is null){
						// This causes GetScenarioToRun to return null
						_runningScenario = null;
                        Application.RequestStop();
                    } else {
                        _runningScenario.RequestStop();
                    }
                }),
            });

            MenuHelper.SetColorScheme(_baseColorScheme);
            _top = Application.Top;
            _top.KeyDown += KeyDownHandler;
            _top.Add(_menu);
            _top.Add(_leftPane);
            _top.Add(_appTitlePane);
            _top.Add(_rightPane);
            _top.Add(_statusBar);
            _top.CanFocus = true;
            _top.Ready += () =>
            {
                if (_runningScenario != null)
                {
                    _runningScenario = null;
                }
            };
            try
            {
                Application.Run(_top);
            }
            catch (System.NullReferenceException e)
            {
                Console.WriteLine("GS::" + e.Message);

            };
            Application.Shutdown();
            return _runningScenario;
        }

        static ColorScheme _baseColorScheme;

        private static void
         AsyncProcTest()
        {
            //var si = new ProcessStartInfo("dotnet", "--info");
            var si = new ProcessStartInfo("vi", "/etc/passwd");
            // ウィンドウ表示を完全に消したい場合
            // si.CreateNoWindow = true;
            si.RedirectStandardError = true;
            si.RedirectStandardOutput = true;
            si.RedirectStandardInput = true;
            si.UseShellExecute = false;
            using (var proc = new Process())
            using (var ctoken = new CancellationTokenSource())
            {
                proc.EnableRaisingEvents = true;
                proc.StartInfo = si;
                // コールバックの設定
                proc.OutputDataReceived += (sender, ev) =>
                {
                    Console.WriteLine($"stdout={ev.Data}");
                };
                proc.ErrorDataReceived += (sender, ev) =>
                {
                    Console.WriteLine($"stderr={ev.Data}");
                };
                proc.Exited += (sender, ev) =>
                {
                    Console.WriteLine($"exited");
                    // プロセスが終了すると呼ばれる
                    ctoken.Cancel();
                };
                // プロセスの開始
                proc.Start();
                // 非同期出力読出し開始
                proc.BeginErrorReadLine();
                proc.BeginOutputReadLine();

                using (var sw = proc.StandardInput)
                {
                    if (sw.BaseStream.CanWrite)
                        sw.WriteLine("ABN");
                }

                // 終了まで待つ
                ctoken.Token.WaitHandle.WaitOne();
                proc.WaitForExit();


            }
        }

        private static void
         SyncProcTest()
        {
            //var si = new ProcessStartInfo("dotnet", "--info");
            var si = new ProcessStartInfo("vi", "/etc/passwd");
            //var si = new ProcessStartInfo("bash" );
            // ウィンドウ表示を完全に消したい場合
            // si.CreateNoWindow = true;
            si.RedirectStandardError = true;
            si.RedirectStandardOutput = true;
            si.RedirectStandardInput = true;
            si.UseShellExecute = false;
            var proc = new Process();
            proc.EnableRaisingEvents = true;
            proc.StartInfo = si;
            // コールバックの設定
            proc.OutputDataReceived += (sender, ev) =>
            {
                Console.WriteLine($"stdout={ev.Data}");
            };
            proc.ErrorDataReceived += (sender, ev) =>
            {
                Console.WriteLine($"stderr={ev.Data}");
            };
            /*
                proc.Exited += (sender, ev) =>
                {
                    Console.WriteLine($"exited");
                    // プロセスが終了すると呼ばれる
                };
            */
            // プロセスの開始
            proc.Start();
            // 非同期出力読出し開始
            proc.BeginErrorReadLine();
            proc.BeginOutputReadLine();
            /*
                           using(var sw = proc.StandardInput)
                            {
                                if (sw.BaseStream.CanWrite)
                                    sw.WriteLine("ABN");
                            }
            */
            // 終了まで待つ
            proc.WaitForExit();


        }

        private static void process()
        {

            var app = new ProcessStartInfo();
            app.FileName = "vim";
            app.Arguments = "/etc/passwd";
            app.CreateNoWindow = true; // コンソール・ウィンドウを開かない
            app.UseShellExecute = false; // シェル機能を使用しない

            Process p = Process.Start(app);
            p.WaitForExit();

            /*
                        var app2 = new ProcessStartInfo();
                        app2.FileName = "ssh";
                        app2.Arguments = "devg1120@localhost";
                        app2.CreateNoWindow = true; // コンソール・ウィンドウを開かない
                        app2.UseShellExecute = false; // シェル機能を使用しない

                        Process p2 = Process.Start(app2);
                        p2.WaitForExit();
                */
        }

        private static void _scenarioListView_OpenSelectedItem(EventArgs e)
        {
            if (_runningScenario is null)
            {
                _scenarioListViewItem = _scenarioListView.SelectedItem;
                var source = _scenarioListView.Source as ScenarioListDataSource;
                /*
                        System.Console.WriteLine("======================");
                        System.Console.WriteLine(_scenarioListViewItem);
                        System.Console.WriteLine(source.Scenarios[_scenarioListView.SelectedItem]);
                        System.Console.WriteLine("=======================");
                    */
                //AsyncProcTest();
                // SyncProcTest();

                /*
               //var procStIfo = new ProcessStartInfo("cmd", "/c " + variableContainingUninstallerPath);
               var procStIfo = new ProcessStartInfo("vim", "/etc/passwd" );
               procStIfo.RedirectStandardOutput = true;
               procStIfo.UseShellExecute = false;
               procStIfo.CreateNoWindow = true;

               var proc = new Process();
               proc.StartInfo = procStIfo;
               proc.Start();
                       proc.BeginOutputReadLine();
               proc.WaitForExit();
               */

                /*
                            //Process.Start("vim /etc/passwd");
                           // Process.Start("bash - vim/etc/passwd");
                       string[] args = [];
                var app = ConsoleApp.Create(args);
                //app.AddCommand("vi", ([Option("/etc/passwd", "Message to display.")] string message) => Console.WriteLine($"Hello {message}"));
                app.AddCommand("vi", ([Option("/etc/passwd")] ));

                app.Run();
                */
                //_runningScenario = (Scenario)Activator.CreateInstance(source.Scenarios[_scenarioListView.SelectedItem]);
                Application.Shutdown();
                process();
                System.Console.WriteLine("*****");
                Init2();
                //Application.Run();

                //dotnet run - -t {accesstoken}
                //    Application.RequestStop();
            }
        }

        internal class ScenarioListDataSource : IListDataSource
        {
            //public List<Type> Scenarios { get; set; }
            public List<string> Scenarios { get; set; }

            public bool IsMarked(int item) => false;

            public int Count => Scenarios.Count;

            public int Length => Scenarios.Count;

            //public ScenarioListDataSource(List<Type> itemList) => Scenarios = itemList;
            public ScenarioListDataSource(List<string> itemList) => Scenarios = itemList;

            public void Render(ListView container, ConsoleDriver driver, bool selected, int item, int col, int line, int width, int start = 0)
            {
                container.Move(col, line);
                // Equivalent to an interpolated string like $"{Scenarios[item].Name, -widtestname}"; if such a thing were possible
                //var s = String.Format(String.Format("{{0,{0}}}", -_nameColumnWidth), Scenario.ScenarioMetadata.GetName(Scenarios[item]));
                var s = String.Format(String.Format("{{0,{0}}}", -_nameColumnWidth), Scenarios[item]);
                //RenderUstr(driver, $"{s}  {Scenario.ScenarioMetadata.GetDescription(Scenarios[item])}", col, line, width);
                RenderUstr(driver, $"{s}  dscription ---", col, line, width);
            }

            public void SetMark(int item, bool value)
            {
            }

            // A slightly adapted method from: https://github.com/migueldeicaza/gui.cs/blob/fc1faba7452ccbdf49028ac49f0c9f0f42bbae91/Terminal.Gui/Views/ListView.cs#L433-L461
            private void RenderUstr(ConsoleDriver driver, ustring ustr, int col, int line, int width)
            {
                int used = 0;
                int index = 0;
                while (index < ustr.Length)
                {
                    (var rune, var size) = Utf8.DecodeRune(ustr, index, index - ustr.Length);
                    var count = Rune.ColumnWidth(rune);
                    if (used + count >= width) break;
                    driver.AddRune(rune);
                    used += count;
                    index += size;
                }

                while (used < width)
                {
                    driver.AddRune(' ');
                    used++;
                }
            }

            public IList ToList()
            {
                return Scenarios;
            }
        }

        /// <summary>
        /// When Scenarios are running we need to override the behavior of the Menu 
        /// and Statusbar to enable Scenarios that use those (or related key input)
        /// to not be impacted. Same as for tabs.
        /// </summary>
        /// <param name="ke"></param>
        private static void KeyDownHandler(View.KeyEventEventArgs a)
        {
            //if (a.KeyEvent.Key == Key.Tab || a.KeyEvent.Key == Key.BackTab) {
            //	// BUGBUG: Work around Issue #434 by implementing our own TAB navigation
            //	if (_top.MostFocused == _categoryListView)
            //		_top.SetFocus (_rightPane);
            //	else
            //		_top.SetFocus (_leftPane);
            //}

            if (a.KeyEvent.IsCapslock)
            {
                _capslock.Title = "Caps: On";
                _statusBar.SetNeedsDisplay();
            }
            else
            {
                _capslock.Title = "Caps: Off";
                _statusBar.SetNeedsDisplay();
            }

            if (a.KeyEvent.IsNumlock)
            {
                _numlock.Title = "Num: On";
                _statusBar.SetNeedsDisplay();
            }
            else
            {
                _numlock.Title = "Num: Off";
                _statusBar.SetNeedsDisplay();
            }

            if (a.KeyEvent.IsScrolllock)
            {
                _scrolllock.Title = "Scroll: On";
                _statusBar.SetNeedsDisplay();
            }
            else
            {
                _scrolllock.Title = "Scroll: Off";
                _statusBar.SetNeedsDisplay();
            }
        }

        private static void CategoryListView_SelectedChanged(ListViewItemEventArgs e)
        {
            if (_categoryListViewItem != _categoryListView.SelectedItem)
            {
                _scenarioListViewItem = 0;
            }
            _categoryListViewItem = _categoryListView.SelectedItem;
            var item = _categories[_categoryListView.SelectedItem];
            //List<Type> newlist;
            List<string> newlist;
            //newlist = _scenarios.Where(t => Scenario.ScenarioCategory.GetCategories(t).Contains(item)).ToList();
            newlist = new List<string>()
            {
                "hoge",
                "row1",
                "row2",
                "row3",
                "row4",
                "row5",
        "hogehoge"
            };

            _scenarioListView.Source = new ScenarioListDataSource(newlist);
            _scenarioListView.SelectedItem = _scenarioListViewItem;
        }
    }
}
