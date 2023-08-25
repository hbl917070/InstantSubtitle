using System;
using System.Collections.Generic;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;


namespace InstantSubtitle {
    /// <summary>
    /// W_print.xaml 的互動邏輯
    /// </summary>
    public partial class SubtitleWindow : Window {



        //http://stackoverflow.com/questions/4334704/outer-bevel-effect-on-text-in-wpf




        MainWindow m;

        public SubtitleWindow(MainWindow m) {
            InitializeComponent();

            this.m = m;

            //var workarea_Hight = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
            //var workerarea_width = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
            //this.Height = workarea_Hight;
            //this.Width = workerarea_width;

            this.Width = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;

            //讓視窗可以拖曳
            this.MouseLeftButtonDown += new MouseButtonEventHandler((object sender, MouseButtonEventArgs e) => {
                try {
                    this.DragMove();
                } catch {
                }


            });



            event_右鍵選單();

            //雙擊字幕 = 水平之中
            this.MouseDoubleClick += W_print_MouseDoubleClick;


        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void W_print_MouseDoubleClick(object sender, MouseButtonEventArgs e) {

            if (m.checkBox_雙擊置中.IsChecked.Value == false) {
                return;
            }

            foreach (System.Windows.Forms.Screen screen in System.Windows.Forms.Screen.AllScreens) {//列出所有螢幕資訊

                var l = screen.Bounds.X;
                var t = screen.Bounds.Y;
                var w = screen.Bounds.Width;
                var h = screen.Bounds.Height;
                var thisL = this.Left + this.ActualWidth / 2;
                var thisT = this.Top + this.ActualHeight / 2;

                if (thisL > l && thisL < l + w && thisT > t && thisT < t + h) {

                    this.Left = l + ((w - this.ActualWidth) / 2);

                    break;
                }


            }


        }









        /// <summary>
        /// 
        /// </summary>
        private void event_右鍵選單() {

            MenuItem propertyMenu = new MenuItem();
            propertyMenu.Header = "隱藏字幕";
            propertyMenu.Click += new RoutedEventHandler((object sender, RoutedEventArgs e) => {
                m.checkBox_顯示字幕.IsChecked = false;
                this.Visibility = System.Windows.Visibility.Collapsed;
            });

            MenuItem propertyMenu2 = new MenuItem();
            propertyMenu2.Header = "上一句";
            propertyMenu2.Click += new RoutedEventHandler((object sender, RoutedEventArgs e) => {
                m.Play(0);
            });

            MenuItem propertyMenu3 = new MenuItem();
            propertyMenu3.Header = "下一句";
            propertyMenu3.Click += new RoutedEventHandler((object sender, RoutedEventArgs e) => {
                m.Play(1);
            });

            Separator se = new Separator();//分割線
            se.Margin = new Thickness(0, 10, 0, 10);

            lable_print.ContextMenu = new ContextMenu();
            lable_print.ContextMenu.Items.Add(propertyMenu);
            lable_print.ContextMenu.Items.Add(se);
            lable_print.ContextMenu.Items.Add(propertyMenu2);
            lable_print.ContextMenu.Items.Add(propertyMenu3);

            bc.ContextMenu = lable_print.ContextMenu;

            /*bc.ContextMenu = new ContextMenu();
            bc.ContextMenu.Items.Add(propertyMenu);
            bc.ContextMenu.Items.Add(se);
            bc.ContextMenu.Items.Add(propertyMenu2);
            bc.ContextMenu.Items.Add(propertyMenu3);*/
        }



    }
}
