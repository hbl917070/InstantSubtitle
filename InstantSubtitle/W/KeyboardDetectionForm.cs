using System.Windows.Forms;

namespace InstantSubtitle {

    /// <summary>
    /// 偵測鍵盤的視窗
    /// </summary>
    public partial class KeyboardDetectionForm : Form {

        public KeyboardDetectionForm(MainWindow m) {
            InitializeComponent();
            keyboardHook1.InstallHook(); //偵測鍵盤
            this.m = m;
        }


        MainWindow m;


        private void keyboardHook1_KeyDown(object sender, WindowsHookLib.KeyboardEventArgs e) {

            //MessageBox.Show(e.KeyValue+"");

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
                if (m.textBox_分子.IsFocused || m.textbox_文字框.IsKeyboardFocusWithin || m.textBox_快速插入.IsFocused ||
                    m.textBox_文字大小.IsFocused || m.textBox_朗讀速度.IsFocused || m.textBox_聲音大小.IsFocused ||
                    m.textBox_外框寬度.IsFocused || m.textBox_外框_羽化.IsFocused ||
                    m.textBox_背景圖.IsFocused
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
