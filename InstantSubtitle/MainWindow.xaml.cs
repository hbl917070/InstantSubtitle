using Microsoft.Win32;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Media;
using System.Net;
using System.Speech.Synthesis;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace InstantSubtitle {
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();

            Adapter.Initialize();//多執行緒初始化
        }


        W_print w;
        List<String> ar = new List<string>();
        int nub = -1;
        private SpeechSynthesizer ssyer = new SpeechSynthesizer();
        private Form1 f;
        private bool boo_需要重新讀取資料 = true;
        private Setting C_set;

        public W_播放模式 w_播放模式;
        int int_播放模式倒數 = 60;


        private void load_Loaded(object sender3, RoutedEventArgs e3) {

            //讓視窗可以拖動
            this.MouseLeftButtonDown += new MouseButtonEventHandler((object sender, MouseButtonEventArgs e) => {
                try {
                    this.DragMove();
                } catch { }
            });

            w = new W_print(this);
            f = new Form1(this);

            C_set = new Setting(this);
            C_set.fun_開啟程式時讀取上次設定(0);

            f.Show();
            w.Show();
            w.Visibility = System.Windows.Visibility.Visible;
            textBox_上一句快速鍵.IsReadOnly = true;
            textBox_下一句快速鍵.IsReadOnly = true;

            event_選取顏色事件();
            event_限制數字();
            event_文字對齊事件();
            event_更換字體();
            event_插入空白行();
            event_重新整理();
            fun_套用設定();
            event_右鍵選單();

            try {//更換字體
                w.lable_print.FontFamily = new FontFamily(comboBox_字體.Text);
            } catch { }

            button_下一句.Click += new RoutedEventHandler((object sss, RoutedEventArgs e) => {
                Play(1);
            });
            button_上一句.Click += new RoutedEventHandler((object sss, RoutedEventArgs e) => {
                Play(0);
            });

            //讓使用者設定完【目前位置】後點擊enter就能直接播放
            textBox_分子.KeyDown += new System.Windows.Input.KeyEventHandler((object sender, System.Windows.Input.KeyEventArgs e) => { //載入網址
                if (e.Key == (Key.Enter)) {
                    try {
                        textBox_分子.Text = (C_set.TextBoxGetInt(textBox_分子, 1, 30000) - 1) + "";//音量     
                        //textBox_分子.Text = (Int32.Parse(textBox_分子.Text) - 1) + "";
                        Play(1);
                    } catch { }
                }
            });



            textBox_快速插入.KeyDown += new System.Windows.Input.KeyEventHandler((object sender, System.Windows.Input.KeyEventArgs e) => { //載入網址
                if (e.Key == (Key.Enter)) {
                    try {
                        QuickInsert(textBox_快速插入.Text);
                        textBox_快速插入.Text = "";
                    } catch { }
                }
            });


            //避免每次都重新讀取內容而浪費記憶體
            textbox_文字框.TextChanged += new EventHandler((object sender, EventArgs e) => {
                boo_需要重新讀取資料 = true;
            });


            //離開文字框、在文字框點擊enter時，儲存套用設定
            List<TextBox> lis = new List<TextBox>();
            lis.Add(textBox_文字大小);
            lis.Add(textBox_朗讀速度);
            lis.Add(textBox_聲音大小);
            lis.Add(textBox_背景圖);
            lis.Add(textBox_外框寬度);
            lis.Add(textBox_外框_羽化);
            lis.Add(textBox_字幕最大寬度);
            for (int i = 0; i < lis.Count; i++) {
                lis[i].KeyDown += new System.Windows.Input.KeyEventHandler((object sender, System.Windows.Input.KeyEventArgs e) => { //載入網址
                    if (e.Key == (Key.Enter))
                        fun_套用設定();
                });

                lis[i].LostKeyboardFocus += new KeyboardFocusChangedEventHandler((object sender, KeyboardFocusChangedEventArgs e) => {
                    fun_套用設定();
                });
            }


            checkBox_顯示字幕.Click += new RoutedEventHandler((object sss, RoutedEventArgs e) => {
                if (checkBox_顯示字幕.IsChecked.Value == false)
                    w.Visibility = System.Windows.Visibility.Collapsed;
                else
                    w.Visibility = System.Windows.Visibility.Visible;
            });


            button_選擇背景.Click += new RoutedEventHandler((object sss, RoutedEventArgs e) => {
                OpenFileDialog saveFileDialog1 = new OpenFileDialog();
                saveFileDialog1.Filter = "Image" + "|" + "*.BMP;*.GIF;*.JPG;*.JPGE;*.PNG" + "|" + "All types" + "|" + "*.*";
                saveFileDialog1.Title = "Setting Background";
                saveFileDialog1.ShowDialog();

                if (saveFileDialog1.FileName != "") {

                    textBox_背景圖.Text = saveFileDialog1.FileName;
                    fun_修改背景(this, textBox_背景圖.Text);
                }
            });


            button_恢復原廠設定.Click += new RoutedEventHandler((object sss, RoutedEventArgs e) => {
                w.Topmost = false;//暫時取消視窗頂置
                if (System.Windows.Forms.MessageBox.Show("確定要恢復所有設定？", "恢復設定",
                    System.Windows.Forms.MessageBoxButtons.YesNo) == System.Windows.Forms.DialogResult.Yes) {
                    C_set.fun_開啟程式時讀取上次設定(1);
                    fun_套用設定();
                }
                w.Topmost = true;
            });


            button_文字歸位.Click += (object sender, RoutedEventArgs e) => {
                var workerarea_width = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
                w.Left = 0;
                w.Top = (workerarea_width - w.Height) / 2;
            };



            var time_定時播放 = new System.Windows.Forms.Timer();
            time_定時播放.Interval = 1000;
            time_定時播放.Tick += (sender, e) => {
                if (w_播放模式 != null && checkBox_播放模式_定時送出.IsChecked.Value) {
                    lab_播放模式_倒數.Visibility = Visibility.Visible;

                    if (int_播放模式倒數 < 0) {
                        w_播放模式.fun_送出();
                    }
                    lab_播放模式_倒數.Content = "倒數：" + int_播放模式倒數;
                    w_播放模式.lab_秒.Content = int_播放模式倒數 + "";
                     int_播放模式倒數--;

                } else {
                    lab_播放模式_倒數.Visibility = Visibility.Collapsed;
                }

            };
            time_定時播放.Start();


            checkBox_播放模式_定時送出.Checked += (sender, e) => {
                fun_播放模式_倒數重置();
            };


            but_開啟播放模式.Click += (sender, e) => {

                if (w_播放模式 != null) {
                    w_播放模式.Close();
                }
                w_播放模式 = new W_播放模式(this);
                w_播放模式.Owner = this;
                w_播放模式.Show();

                //判斷是否可以換行
                if (checkBox_播放模式_禁止換行.IsChecked.Value) {          
                    w_播放模式.text_1.AcceptsReturn = false;
                    w_播放模式.bool_禁止換行 = true;
                } else {
                    w_播放模式.text_1.AcceptsReturn = true;
                    w_播放模式.bool_禁止換行 = false;
                }

                fun_播放模式_倒數重置();
            };


            //不使用快速鍵
            but_取消快速鍵_上一句.Click += (sender, e) => {
                textBox_上一句快速鍵.Text = "";
            };
            but_取消快速鍵_下一句.Click += (sender, e) => {
                textBox_下一句快速鍵.Text = "";
            };


            //用滾輪縮放文字大小
            textbox_文字框.PreviewMouseWheel += (sender, e) => {

                //按下ctrl時才觸發
                bool bool_ctrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);//取得目前是否按下ctrl
                if (bool_ctrl == false) {
                    return;
                }

                e.Handled = true;

                double size = textbox_文字框.FontSize;

                if (e.Delta > 0) {
                    size += 3;
                } else {
                    size -= 3;
                }

                if (size > 500) {
                    size = 500;
                }
                if (size < 10) {
                    size = 10;
                }
                textbox_文字框.FontSize = size;
            };

            ///設定動畫
            tabControl_1.SelectionChanged += (sender, e) => {
                try {
                    TabControl tabc = (TabControl)e.OriginalSource;
                    var tabItem = (TabItem)tabc.Items[tabc.SelectedIndex];
                    fun_動畫((FrameworkElement)(tabItem.Content), 20, 0, "Y");

                } catch { }

            };

            checkBox_播放模式_禁止換行.Checked += (sender, e) => {
                if (w_播放模式 != null) {
                    w_播放模式. text_1.AcceptsReturn = false;
                    w_播放模式.bool_禁止換行 = true;
                }          
            };
             checkBox_播放模式_禁止換行.Unchecked += (sender, e) => {
                if (w_播放模式 != null) {
                    w_播放模式. text_1.AcceptsReturn = true;
                    w_播放模式.bool_禁止換行 = false;
                }          
            };
        }



        /// <summary>
        /// 
        /// </summary>
        public void fun_播放模式_倒數重置() {
            int xx = C_set.TextBoxGetInt(textBox_播放模式_定時送出, 1, 999);
            int_播放模式倒數 = xx;
        }


        /// <summary>
        /// 
        /// </summary>
        private void event_右鍵選單() {

            MenuItem propertyMenu = new MenuItem();
            propertyMenu.Header = "插入斷行符號";
            propertyMenu.Click += new RoutedEventHandler((object sender, RoutedEventArgs e) => {
                int a = textbox_文字框.SelectionStart;
                textbox_文字框.Text = textbox_文字框.Text.Insert(a, "<br>");
            });

            MenuItem propertyMenu2 = new MenuItem();
            propertyMenu2.Header = "全選 （Ctrl+A）";
            propertyMenu2.Click += new RoutedEventHandler((object sender, RoutedEventArgs e) => {
                textbox_文字框.SelectAll();
            });

            MenuItem propertyMenu3 = new MenuItem();
            propertyMenu3.Header = "剪下 （Ctrl+X）";
            propertyMenu3.Click += new RoutedEventHandler((object sender, RoutedEventArgs e) => {
                textbox_文字框.Cut();
            });

            MenuItem propertyMenu4 = new MenuItem();
            propertyMenu4.Header = "複製 （Ctrl+C）";
            propertyMenu4.Click += new RoutedEventHandler((object sender, RoutedEventArgs e) => {
                textbox_文字框.Copy();
            });

            MenuItem propertyMenu5 = new MenuItem();
            propertyMenu5.Header = "貼上 （Ctrl+V）";
            propertyMenu5.Click += new RoutedEventHandler((object sender, RoutedEventArgs e) => {
                textbox_文字框.Paste();
            });


            Separator se = new Separator();//分割線
            se.Margin = new Thickness(0, 10, 0, 10);

            textbox_文字框.ContextMenu = new ContextMenu();
            textbox_文字框.ContextMenu.Items.Add(propertyMenu);
            textbox_文字框.ContextMenu.Items.Add(se);
            textbox_文字框.ContextMenu.Items.Add(propertyMenu2);
            textbox_文字框.ContextMenu.Items.Add(propertyMenu3);
            textbox_文字框.ContextMenu.Items.Add(propertyMenu4);
            textbox_文字框.ContextMenu.Items.Add(propertyMenu5);
        }


        /// <summary>
        /// 更換字體
        /// </summary>
        private void event_更換字體() {

            //選取變更時。延遲一段時間在變更字體，不然會沒有效果
            comboBox_字體.SelectionChanged += (object sender, SelectionChangedEventArgs e) => {
                try {
                    new Thread(() => {
                        Thread.Sleep(10);
                        Adapter.UIThread(() => {
                            try {
                                w.lable_print.FontFamily = new FontFamily(comboBox_字體.Text);
                            } catch { }
                        });
                    }).Start();
                } catch { }
            };

            //按下enter
            comboBox_字體.KeyDown += (object sender, KeyEventArgs e) => {
                try {
                    if (e.Key == Key.Enter)
                        w.lable_print.FontFamily = new FontFamily(comboBox_字體.Text);
                } catch { }
            };

            //意思焦點
            comboBox_字體.LostFocus += ( sender,  e) => {
                try {
                        w.lable_print.FontFamily = new FontFamily(comboBox_字體.Text);
                } catch { }
            };
        }


        /// <summary>
        /// 
        /// </summary>
        private void event_重新整理() {

            button_重新整理.Click += (object sender, RoutedEventArgs e) => {
                String[] st = textbox_文字框.Text.Split('\n');
                ar = new List<string>();
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < st.Length; i++) {
                    st[i] = st[i].Replace("\r", "");

                    if (st[i].Replace(" ", "").Equals("") == false) {
                        String x = st[i].Replace("<br>", "\r\n");
                        ar.Add(x);
                        sb.Append(st[i] + "\n");
                    }
                }

                textbox_文字框.Text = sb.ToString();

                label_分母.Content = ar.Count;
            };
        }


        /// <summary>
        /// 
        /// </summary>
        private void event_插入空白行() {

            button_插入空白行.Click += (object sender, RoutedEventArgs e) => {
                String[] st = textbox_文字框.Text.Split('\n');
                ar = new List<string>();
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < st.Length; i++) {
                    st[i] = st[i].Replace("\r", "");

                    if (st[i].Replace(" ", "").Equals("") == false) {
                        String x = st[i].Replace("<br>", "\r\n");
                        ar.Add(x);
                        ar.Add("");//多增加一個空白行
                        sb.Append(st[i] + "\n\n");//多增加一個空白行
                    }
                }


                textbox_文字框.Text = sb.ToString();

                label_分母.Content = ar.Count;
            };
        }


        /// <summary>
        /// 
        /// </summary>
        private void fun_套用設定() {

            ssyer.Volume = C_set.TextBoxGetInt(textBox_聲音大小, 0, 100);//音量      
            ssyer.Rate = C_set.TextBoxGetInt(textBox_朗讀速度, -10, 10);//速度      
            w.lable_print.FontSize = C_set.TextBoxGetInt(textBox_文字大小, 1, 500);//文字大小
            w.lable_print.StrokeThickness = C_set.TextBoxGetFloat(textBox_外框寬度, 0, 50);//寬度
            w.BlurEffect_模糊.Radius = C_set.TextBoxGetFloat(textBox_外框_羽化, 0, 100);//寬度

            w.Width = C_set.TextBoxGetInt(textBox_字幕最大寬度, 200, 4000);//寬度
   
            Color cc = color_文字顏色.SelectedColor;
            w.lable_print.Fill = new SolidColorBrush(Color.FromArgb(cc.A, cc.R, cc.G, cc.B));//文字顏色

            Color cc2 = color_底色.SelectedColor;
            w.bc.Background = new SolidColorBrush(Color.FromArgb(cc2.A, cc2.R, cc2.G, cc2.B));//文字顏色

            Color cc3 = color_前景色.SelectedColor;
            tabControl_前景色.Background = new SolidColorBrush(Color.FromArgb(cc3.A, cc3.R, cc3.G, cc3.B));//文字顏色

            Color cc4 = color_外框顏色.SelectedColor;
            w.lable_print.Stroke = new SolidColorBrush(Color.FromArgb(cc4.A, cc4.R, cc4.G, cc4.B));//文字顏色

            fun_修改背景(this, textBox_背景圖.Text);

            if (checkBox_顯示字幕.IsChecked.Value == false)
                w.Visibility = System.Windows.Visibility.Collapsed;
            else
                w.Visibility = System.Windows.Visibility.Visible;

            if (radio_左.IsChecked.Value) {
                w.bc.HorizontalAlignment = HorizontalAlignment.Left;
                w.lable_print.TextAlignment = TextAlignment.Left;
            } else if (radio_中.IsChecked.Value) {
                w.bc.HorizontalAlignment = HorizontalAlignment.Center;
                w.lable_print.TextAlignment = TextAlignment.Center;
            } else if (radio_右.IsChecked.Value) {
                w.bc.HorizontalAlignment = HorizontalAlignment.Right;
                w.lable_print.TextAlignment = TextAlignment.Right;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        private void event_文字對齊事件() {

            List<RadioButton> ar = new List<RadioButton>();
            ar.Add(radio_左);
            ar.Add(radio_中);
            ar.Add(radio_右);

            for (int i = 0; i < ar.Count; i++) {
                ar[i].Checked += (object sender, RoutedEventArgs e) => {

                    if (radio_左.IsChecked.Value) {
                        w.bc.HorizontalAlignment = HorizontalAlignment.Left;
                        w.lable_print.TextAlignment = TextAlignment.Left;
                    } else if (radio_中.IsChecked.Value) {
                        w.bc.HorizontalAlignment = HorizontalAlignment.Center;
                        w.lable_print.TextAlignment = TextAlignment.Center;
                    } else if (radio_右.IsChecked.Value) {
                        w.bc.HorizontalAlignment = HorizontalAlignment.Right;
                        w.lable_print.TextAlignment = TextAlignment.Right;
                    }
                };
            }

        }


        /// <summary>
        /// 
        /// </summary>
        private void event_選取顏色事件() {
            //
            //
            color_文字顏色.MouseUp += new MouseButtonEventHandler((object sender2, MouseButtonEventArgs e2) => {
                color_文字顏色.IsDropDownOpen = true;
            });
            //
            color_文字顏色.IsMouseCaptureWithinChanged += new DependencyPropertyChangedEventHandler((object sender2, DependencyPropertyChangedEventArgs e2) => {
                if (e2.OldValue.Equals(true)) {
                    Color cc = color_文字顏色.SelectedColor;
                    w.lable_print.Fill = new SolidColorBrush(Color.FromArgb(cc.A, cc.R, cc.G, cc.B));//文字顏色
                }
            });
            //
            //----------------------------------
            //
            color_底色.MouseUp += new MouseButtonEventHandler((object sender2, MouseButtonEventArgs e2) => {
                color_底色.IsDropDownOpen = true;
            });
            //
            color_底色.IsMouseCaptureWithinChanged += new DependencyPropertyChangedEventHandler((object sender2, DependencyPropertyChangedEventArgs e2) => {
                if (e2.OldValue.Equals(true)) {
                    Color cc = color_底色.SelectedColor;
                    w.bc.Background = new SolidColorBrush(Color.FromArgb(cc.A, cc.R, cc.G, cc.B));//背景色
                }
            });
            //
            //----------------------------------
            //
            color_前景色.MouseUp += new MouseButtonEventHandler((object sender2, MouseButtonEventArgs e2) => {
                color_前景色.IsDropDownOpen = true;
            });
            //
            color_前景色.IsMouseCaptureWithinChanged += new DependencyPropertyChangedEventHandler((object sender2, DependencyPropertyChangedEventArgs e2) => {
                if (e2.OldValue.Equals(true)) {
                    Color cc = color_前景色.SelectedColor;
                    tabControl_前景色.Background = new SolidColorBrush(Color.FromArgb(cc.A, cc.R, cc.G, cc.B));//背景色
                }
            });
            //
            //----------------------------------
            //
            color_外框顏色.MouseUp += new MouseButtonEventHandler((object sender2, MouseButtonEventArgs e2) => {
                color_外框顏色.IsDropDownOpen = true;
            });
            //
            color_外框顏色.IsMouseCaptureWithinChanged += new DependencyPropertyChangedEventHandler((object sender2, DependencyPropertyChangedEventArgs e2) => {
                if (e2.OldValue.Equals(true)) {
                    Color cc = color_外框顏色.SelectedColor;
                    w.lable_print.Stroke = new SolidColorBrush(Color.FromArgb(cc.A, cc.R, cc.G, cc.B));//背景色
                }
            });
        }


        /// <summary>
        /// 快速插入
        /// </summary>
        public void QuickInsert(String ttt) {

            /*
            if (textBox_快速插入.Text.Replace(" ", "").Equals("") == true) {
                textBox_快速插入.Text = "";
                return;
            }
            */

            String[] st = textbox_文字框.Text.Split('\n');
            ar = new List<string>();
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < st.Length; i++) {
                st[i] = st[i].Replace("\r", "");

                if (st[i].Replace(" ", "").Equals("") == false) {//拿掉所有斷行
                    ar.Add(st[i].Replace("<br>", "\r\n"));
                }
            }

            int int_分子 = C_set.TextBoxGetInt(textBox_分子, 1, 30000);

            if (int_分子 >= ar.Count) {
                int_分子 = ar.Count;
                ar.Add(ttt);
            } else {
                ar.Insert(int_分子, ttt);
            }

            for (int i = 0; i < ar.Count; i++) {
                sb.Append(ar[i] + "\n");
            }

            nub = int_分子;
            w.lable_print.Text = ar[nub].Replace("<br>", "\r\n");//顯示字幕

            textBox_分子.Text = (int_分子 + 1) + "";
            label_分母.Content = ar.Count;
            textbox_文字框.Text = sb.ToString();

            boo_需要重新讀取資料 = false;

            if (checkBox_播放聲音.IsChecked.Value) {
                func_朗讀(ar[nub]);
            }

        }


        /// <summary>
        /// 重新讀取內容
        /// </summary>
        private void fun_整理() {

            String[] st = textbox_文字框.Text.Split('\n');
            ar = new List<string>();
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < st.Length; i++) {
                st[i] = st[i].Replace("\r", "");

                if (st[i].Replace(" ", "").Equals("") == false) {//拿掉所有斷行
                    String x = st[i].Replace("<br>", "\r\n");
                    ar.Add(x);
                    sb.Append(st[i] + "\n");
                }
            }

            textbox_文字框.Text = sb.ToString();

            label_分母.Content = ar.Count;
        }



        /// <summary>
        /// 播放（0=上一句、1=下一句）
        /// </summary>
        public void Play(int type) {

            if (boo_需要重新讀取資料 == true) {
                fun_整理();
                boo_需要重新讀取資料 = false;
            }

            //-------------------
            try {
                nub = Int32.Parse(textBox_分子.Text) - 1;
            } catch {
                textBox_分子.Text = nub + "";
            }

            if (type == 1)
                nub++;
            else
                nub--;

            if (ar.Count == 0)
                return;
            else if (nub >= ar.Count)
                nub = 0;
            else if (nub <= -1)
                nub = ar.Count - 1;

            //-------------------------

            textBox_分子.Text = (nub + 1) + "";

            func_更新字幕(ar[nub]);//顯示字幕


            if (checkBox_播放聲音.IsChecked.Value) {
                func_朗讀(ar[nub]);
            }

            if (nub % 10 == 0) {
                GC.Collect();
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="input_t"></param>
        public void func_更新字幕(String input_t) {

            if (input_t.Trim().ToUpper() == "<NONE>") {
                w.lable_print.Text = ""; //顯示字幕
                this.Title = "";
                return;
            }

            w.lable_print.Text = input_t; //顯示字幕

            this.Title = (nub + 1) + " / " + ar.Count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="input_t"></param>
        public void func_朗讀(String input_t) {

            input_t = input_t.Replace("<br>", " , ");
            input_t = input_t.Replace("<BR>", " , ");
            input_t = input_t.Replace("<none>", " ");
            input_t = input_t.Replace("<NONE>", " ");

            if (radio_Windows內建.IsChecked.Value) {
                TTS_windows(input_t);
            }

            if (radio_Google.IsChecked.Value) {
                TTS_google(input_t);
            }

        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="input_t"></param>
        public void TTS_windows(String input_t) {
            ssyer.SpeakAsyncCancelAll();//暫停目前的聲音
            ssyer.SpeakAsync(input_t);//播放音效
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="input_t"></param>
        private void TTS_google(String input_t) {

            if (File.Exists(System.AppDomain.CurrentDomain.BaseDirectory + "2.wav")) {
                File.Delete(System.AppDomain.CurrentDomain.BaseDirectory + "2.wav");
            }

            input_t = Uri.EscapeDataString(input_t);//編碼成網址
            String url = "http://translate.google.com/translate_tts?ie=UTF-8&total=1&idx=0&textlen=32&client=tw-ob&q=" + input_t + "&tl=zh";

            WebClient myWebClient = new WebClient();
            myWebClient.DownloadFile(url, "1.mp3");

            ConvertMp3ToWav("1.mp3", "1.wav");

            System.Diagnostics.Process Info = new System.Diagnostics.Process();
            Info.StartInfo.FileName = System.AppDomain.CurrentDomain.BaseDirectory + "soundstretch.exe"; //要啟動的應用程式
            Info.StartInfo.Arguments = $"\"{System.AppDomain.CurrentDomain.BaseDirectory + "1.wav"}\" " +
                                       $"\"{System.AppDomain.CurrentDomain.BaseDirectory + "2.wav"}\" " +
                                       "-tempo=80"; //該應用程式的指令

            Info.Start();
            Info.WaitForExit();

            SoundPlayer sp = new SoundPlayer(System.AppDomain.CurrentDomain.BaseDirectory + "1.mp3");
            sp.Play();
            sp.Dispose();

            Info.Dispose();
        }


        /// <summary>
        /// MP3轉WAV
        /// </summary>
        /// <param name="_inPath_"></param>
        /// <param name="_outPath_"></param>
        private void ConvertMp3ToWav(string _inPath_, string _outPath_) {
            using (Mp3FileReader mp3 = new Mp3FileReader(_inPath_)) {
                using (WaveStream pcm = WaveFormatConversionStream.CreatePcmStream(mp3)) {
                    WaveFileWriter.CreateWaveFile(_outPath_, pcm);
                }
            }
        }





        /// <summary>
        /// 修改背景
        /// </summary>
        public void fun_修改背景(Window w, String u) {

            if (u.ToUpper().Equals("DEFAULT")) {
                u = System.Windows.Forms.Application.StartupPath + "/data/Background.jpg";
            }

            try {
                w.Background = new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(u)),
                    Stretch = Stretch.UniformToFill,
                    TileMode = TileMode.None,

                };
            } catch { }

        }



        /// <summary>
        /// 限制文字框只能輸入數字。要註冊新的物件在這裡修改
        /// </summary>
        private void event_限制數字(List<System.Windows.Controls.TextBox> tex) {

            var fun_限制數字_down = new Action<object, System.Windows.Input.KeyEventArgs>(
            (object sender, System.Windows.Input.KeyEventArgs e) => {

                if ((e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) ||
                (e.Key >= Key.D0 && e.Key <= Key.D9) ||
                e.Key == Key.Back ||
                e.Key == Key.Left || e.Key == Key.Right || e.Key == Key.Enter) {
                    if (e.KeyboardDevice.Modifiers != ModifierKeys.None) {
                        e.Handled = true;
                    }
                } else {
                    e.Handled = true;
                }

            });

            var fun_限制數字_Changed = new Action<object, TextChangedEventArgs>(
            (object sender, TextChangedEventArgs e) => {

                //屏蔽中文輸入和非法字符粘貼輸入
                System.Windows.Controls.TextBox textBox = sender as System.Windows.Controls.TextBox;
                TextChange[] change = new TextChange[e.Changes.Count];
                e.Changes.CopyTo(change, 0);

                int offset = change[0].Offset;
                if (change[0].AddedLength > 0) {
                    double num = 0;
                    if (!Double.TryParse(textBox.Text, out num)) {
                        textBox.Text = textBox.Text.Remove(offset, change[0].AddedLength);
                        textBox.Select(offset, 0);
                    }
                }

            });



            for (int i = 0; i < tex.Count; i++) {
                tex[i].KeyDown += new System.Windows.Input.KeyEventHandler(fun_限制數字_down);
                tex[i].TextChanged += new System.Windows.Controls.TextChangedEventHandler(fun_限制數字_Changed);
            }

        }
        private void event_限制數字() {
            List<System.Windows.Controls.TextBox> tex = new List<System.Windows.Controls.TextBox>();

            tex.Add(textBox_分子);
            tex.Add(textBox_聲音大小);
            tex.Add(textBox_文字大小);
            tex.Add(textBox_播放模式_定時送出);

            event_限制數字(tex);
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="f"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="s_移動方式"></param>
        private void fun_動畫(FrameworkElement f, double from, double to, String s_移動方式) {

            if (f == null)
                return;

            s_移動方式 = s_移動方式.ToUpper();


            //位移
            Storyboard storyboard2 = new Storyboard();
            DoubleAnimation growAnimation2 = new DoubleAnimation();
            growAnimation2.Duration = (Duration)TimeSpan.FromSeconds(0.2f);

            growAnimation2.Completed += (sender, e) => {//完成時執行

                //f.Visibility = Visibility.Collapsed;
            };

            f.RenderTransform = new TranslateTransform();

            growAnimation2.From = from;
            growAnimation2.To = to;

            Storyboard.SetTargetProperty(growAnimation2, new PropertyPath("(FrameworkElement.RenderTransform).(TranslateTransform." + s_移動方式 + ")"));
            Storyboard.SetTarget(growAnimation2, f);

            storyboard2.Children.Add(growAnimation2);
            storyboard2.Begin();

            //----------------------

            Storyboard storyboard3 = new Storyboard();
            DoubleAnimation growAnimation3 = new DoubleAnimation();
            growAnimation3.Duration = (Duration)TimeSpan.FromSeconds(0.25f);

            growAnimation3.Completed += (sender, e) => {//完成時執行

                //f.Visibility = Visibility.Collapsed;
            };

            //f.RenderTransform = new TranslateTransform();

            growAnimation3.From = 0;
            growAnimation3.To = 1;

            Storyboard.SetTargetProperty(growAnimation3, new PropertyPath("Opacity"));
            Storyboard.SetTarget(growAnimation3, f);

            storyboard3.Children.Add(growAnimation3);
            storyboard3.Begin();
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void load_Closed(object sender, EventArgs e) {
            C_set.SaveSetting();
            try {
                w.Close();
                f.Close();
                ssyer.Dispose();
            } catch { }

        }




    }
}
