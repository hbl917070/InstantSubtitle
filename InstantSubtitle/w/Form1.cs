using System.Windows.Forms;

namespace InstantSubtitle {
    public partial class Form1 : Form {

        public Form1(MainWindow m) {
            InitializeComponent();
            keyboardHook1.InstallHook(); //偵測鍵盤
            this.m = m;
        }


        MainWindow m;


        private void keyboardHook1_KeyDown(object sender, WindowsHookLib.KeyboardEventArgs e) {

            //MessageBox.Show(e.KeyValue+"");


            //避免在播放模式啟動
            if (m.w_播放模式 != null) {
                return;
            }


            if (m.IsActive == true) {

                //設定快速鍵
                if (m.textBox_下一句快速鍵.IsFocused == true) {
                    m.textBox_下一句快速鍵.Text = e.KeyCode.ToString();
                    return;
                }
                if (m.textBox_上一句快速鍵.IsFocused == true) {
                    m.textBox_上一句快速鍵.Text = e.KeyCode.ToString();
                    return;
                }

                //避免在輸入文字時啟用
                if (m.textBox_分子.IsFocused == true || m.textbox_文字框.IsKeyboardFocusWithin == true || m.textBox_快速插入.IsFocused == true ||
                    m.textBox_文字大小.IsFocused == true || m.textBox_朗讀速度.IsFocused == true || m.textBox_聲音大小.IsFocused == true ||
                    m.textBox_外框寬度.IsFocused == true || m.textBox_播放模式_定時送出.IsFocused == true || m.textBox_外框_羽化.IsFocused == true ||
                  m.textBox_背景圖.IsFocused == true
                    ) {

                    return;
                }

            }


            if (e.KeyCode.ToString().Equals(m.textBox_下一句快速鍵.Text)) {
                m.Play(1);
            } else if (e.KeyCode.ToString().Equals(m.textBox_上一句快速鍵.Text)) {
                m.Play(0);
            }


        }



    }
}
