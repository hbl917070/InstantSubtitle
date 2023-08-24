using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace InstantSubtitle {
    /// <summary>
    /// W_播放模式.xaml 的互動邏輯
    /// </summary>
    public partial class W_播放模式 : Window {


        MainWindow M;

        public bool bool_禁止換行 = true;

        public W_播放模式(MainWindow m) {

            this.M = m;

            InitializeComponent();


            text_1.Text = "Entet=換行；" + "Enter+Shift=送出；" + "ctrl+滑鼠滾輪上下=縮放文字";

            this.SourceInitialized += new System.EventHandler(MainWindow_SourceInitialized);//右下角拖曳



            event_視窗改變size();



            //視窗改變全螢幕或視窗化
            this.StateChanged += (sneder, e) => {

                // int screenWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
                // int screenHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;



                if (this.WindowState == WindowState.Maximized) {
                    grid_視窗控制.Visibility = Visibility.Collapsed;
                    but_視窗化.Visibility = Visibility.Visible;
                    but_最大化.Visibility = Visibility.Collapsed;


                } else {
                    grid_視窗控制.Visibility = Visibility.Visible;
                    but_視窗化.Visibility = Visibility.Collapsed;
                    but_最大化.Visibility = Visibility.Visible;

                }
            };
            but_視窗化.Visibility = Visibility.Collapsed;


            //滑鼠進入title的按鈕時才顯示
            StackPanel_視窗選項.Opacity = 0;
            StackPanel_視窗選項.MouseEnter += (semder, e) => {
                StackPanel_視窗選項.Opacity = 1;
            };
            StackPanel_視窗選項.MouseLeave += (semder, e) => {
                StackPanel_視窗選項.Opacity = 0;
            };


            //右上角的3個按鈕
            but_關閉.Click += (sneder, e) => {
                this.Close();
                M.w_播放模式 = null;
            };
            but_視窗化.Click += (sneder, e) => {
                //this.WindowState = WindowState.Normal;
                fun_視窗化();
            };
            but_最大化.Click += (sneder, e) => {
                //this.WindowState = WindowState.Maximized;
                fun_最大化();
            };



            //雙擊title列全熒幕或視窗化
            this.MouseDoubleClick += (sneder, e) => {
                var mou = System.Windows.Forms.Cursor.Position;
                var pos = this.PointToScreen(new Point(0, 0));
                if (mou.X > pos.X && mou.Y > pos.Y && mou.Y < pos.Y + 30) {
                    /*if (this.WindowState == WindowState.Maximized) {
                        this.WindowState = WindowState.Normal;
                    } else {
                        this.WindowState = WindowState.Maximized;
                    }*/
                    if (bool_最大化) {
                        fun_視窗化();
                    } else {
                        fun_最大化();
                    }
                }
            };


            //用滾輪縮放文字大小
            text_1.MouseWheel += (sender, e) => {

                //按下ctrl時才觸發
                bool bool_ctrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);//取得目前是否按下ctrl
                if (bool_ctrl == false) {
                    return;
                }

                e.Handled = true;

                double size = text_1.FontSize;

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
                text_1.FontSize = size;
            };



            //按enter送出
            text_1.KeyUp += Text_1_KeyUp;
            text_1.KeyDown += Text_1_KeyDown;

            if (bool_禁止換行) {
                text_1.AcceptsReturn = false;
            } else {
                text_1.AcceptsReturn = true;
            }


        }


        double this_left = 0;
        double this_top = 0;
        double this_width = 300;
        double this_height = 300;
        bool bool_最大化 = false;

        /// <summary>
        /// 
        /// </summary>
        private void fun_最大化() {


            bool_最大化 = true;

            //記錄
            this_left = this.Left;
            this_top = this.Top;
            this_width = this.ActualWidth;
            this_height = this.ActualHeight;


            var Work = System.Windows.Forms.Screen.GetBounds(new System.Drawing.Point((int)this.Left, (int)this.Top));


            /*foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens) {//列出所有螢幕資訊
                int xx = screen.Bounds.X + screen.Bounds.Width;
                if (xx > w)
                    w = xx;
            }*/

            int screenWidth = Work.Width;
            int screenHeight = Work.Height;

            this.Height = screenHeight;
            this.Width = screenWidth;



            this.Left = Work.Left;
            this.Top = Work.Top;

            //   
            grid_視窗控制.Visibility = Visibility.Collapsed;
            but_視窗化.Visibility = Visibility.Visible;
            but_最大化.Visibility = Visibility.Collapsed;
        }


        /// <summary>
        /// 
        /// </summary>
        private void fun_視窗化() {
            bool_最大化 = false;
            //恢復
            this.Left = this_left;
            this.Top = this_top;
            this.Width = this_width;
            this.Height = this_height;

            //
            grid_視窗控制.Visibility = Visibility.Visible;
            but_視窗化.Visibility = Visibility.Collapsed;
            but_最大化.Visibility = Visibility.Visible;
        }


        private void Text_1_KeyDown(object sender, KeyEventArgs e) {

            if (M.checkBox_播放模式_enter送出.IsChecked.Value)
                if (e.Key == Key.Enter && text_1.Text.Replace("\r\n", "").Length > 0) {
                    fun_送出();
                    return;
                }


            //如果shift是按下的狀態，就不要處理enter事件（enter無法攔截）
            if (bool_禁止換行 == false) {
                bool bool_shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);//取得目前是否按下shift
                text_1.AcceptsReturn = !bool_shift;
            }
        }



        private void Text_1_KeyUp(object sender, KeyEventArgs e) {


            bool bool_shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            if (bool_shift && e.Key == Key.Enter && text_1.Text.Replace("\r\n", "").Length > 0) {
                fun_送出();
            }
            if (bool_禁止換行 == false) {
                text_1.AcceptsReturn = true;//避免下次個輸入的enter失效
            }
        }



        /// <summary>
        /// 
        /// </summary>
        public void fun_送出() {

            //設定文字上限數量
            String out_t = text_1.Text.Replace("\r\n", "<br>");

            float int_累計 = 0;
            int int_max = 10;
            String sum = "";

            try {

                int_max = Int32.Parse(M.textBox_播放_字數上限.Text.Trim());

                for (int i = 0; i < out_t.Length; i++) {
                    String sss = out_t.Substring(i, 1);
                    if (System.Text.Encoding.Default.GetBytes(sss).Length >= 2) {//判斷是否為全型
                        int_累計 += 1;
                    } else {
                        int_累計 += 0.5f;
                    }
                    sum += sss;
                    if (int_累計 >= int_max) {
                        break;
                    }
                }

            } catch  {//錯誤就直接輸出原文字

                sum = out_t;
            }
           

       

            M.QuickInsert(sum);
            text_1.Text = "";
            M.fun_播放模式_倒數重置();
        }



       



        //------------------


        #region 改變視窗大小


        /// <summary>
        /// 
        /// </summary>
        private void event_視窗改變size() {


            b_視窗_上.MouseLeftButtonDown += ((object sender, MouseButtonEventArgs e) => {
                try {
                    ResizeWindow(ResizeDirection.Top);//拖曳視窗
                } catch { }
            });
            b_視窗_下.MouseLeftButtonDown += ((object sender, MouseButtonEventArgs e) => {
                try {
                    ResizeWindow(ResizeDirection.Bottom);//拖曳視窗
                } catch { }
            });
            b_視窗_左.MouseLeftButtonDown += ((object sender, MouseButtonEventArgs e) => {
                try {
                    ResizeWindow(ResizeDirection.Left);//拖曳視窗
                } catch { }
            });
            b_視窗_右.MouseLeftButtonDown += ((object sender, MouseButtonEventArgs e) => {
                try {
                    ResizeWindow(ResizeDirection.Right);//拖曳視窗
                } catch { }
            });
            b_視窗_右上.MouseLeftButtonDown += ((object sender, MouseButtonEventArgs e) => {
                try {
                    ResizeWindow(ResizeDirection.TopRight);//拖曳視窗
                } catch { }
            });
            b_視窗_右下.MouseLeftButtonDown += ((object sender, MouseButtonEventArgs e) => {
                try {
                    ResizeWindow(ResizeDirection.BottomRight);//拖曳視窗
                } catch { }
            });
            b_視窗_左上.MouseLeftButtonDown += ((object sender, MouseButtonEventArgs e) => {
                try {
                    ResizeWindow(ResizeDirection.TopLeft);//拖曳視窗
                } catch { }
            });
            b_視窗_左下.MouseLeftButtonDown += ((object sender, MouseButtonEventArgs e) => {
                try {
                    ResizeWindow(ResizeDirection.BottomLeft);//拖曳視窗
                } catch { }
            });
            b_視窗_title.MouseLeftButtonDown += ((object sender, MouseButtonEventArgs e) => {
                try {
                    ResizeWindow(ResizeDirection.Move);//拖曳視窗
                } catch { }
            });
        }


        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            Debug.WriteLine("WndProc messages: " + msg.ToString());

            if (msg == WM_SYSCOMMAND) {
                Debug.WriteLine("WndProc messages: " + msg.ToString());
            }

            return IntPtr.Zero;
        }

        void MainWindow_SourceInitialized(object sender, System.EventArgs e) {
            hwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;
            hwndSource.AddHook(new HwndSourceHook(WndProc));
        }

        private const int WM_SYSCOMMAND = 0x112;
        private HwndSource hwndSource;
        IntPtr retInt = IntPtr.Zero;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        public enum ResizeDirection {
            Left = 1,
            Right = 2,
            Top = 3,
            TopLeft = 4,
            TopRight = 5,
            Bottom = 6,
            BottomLeft = 7,
            BottomRight = 8,
            Move = 9
        }

        private void ResizeWindow(ResizeDirection direction) {
            try {
                SendMessage(hwndSource.Handle, WM_SYSCOMMAND, (IntPtr)(61440 + direction), IntPtr.Zero);
            } catch (Exception) {


            }


        }


        private void ResetCursor(object sender, MouseEventArgs e) {
            if (Mouse.LeftButton != MouseButtonState.Pressed) {
                this.Cursor = Cursors.Arrow;
            }
        }


        #endregion





    }
}
