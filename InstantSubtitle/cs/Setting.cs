using ColorPicker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;

namespace InstantSubtitle {
    class Setting {

        private String pathXml = "data/setting.xml"; //儲存的檔名
        private String pathXmlDefault = "data/setting-preset.xml"; //預設資料

        public MainWindow m;


        public Setting(MainWindow m) {
            this.m = m;
        }


        /// <summary>
        /// 儲存設定
        /// </summary>
        public void SaveSetting() {

            Func<Color, String> getColor = new Func<Color, String>((Color co) => {
                return co.A + "," + co.R + "," + co.G + "," + co.B;
            });

            Action<XmlTextWriter, String, String> writeItem = new Action<XmlTextWriter, String, String>(
                    (XmlTextWriter XTW, String key, String value) => {
                        XTW.WriteStartElement("item");
                        XTW.WriteAttributeString("name", key);
                        XTW.WriteString(value);
                        XTW.WriteEndElement();
                    });

            XmlTextWriter X = new XmlTextWriter(pathXml, Encoding.UTF8);


            X.WriteStartDocument(); //使用1.0版本
            X.Formatting = Formatting.Indented; //自動縮排
            X.Indentation = 2; //縮排距離

            X.WriteStartElement("settings");

            //
            writeItem(X, m.checkBox_播放聲音.Name, m.checkBox_播放聲音.IsChecked.Value.ToString());//
            writeItem(X, m.checkBox_顯示字幕.Name, m.checkBox_顯示字幕.IsChecked.Value.ToString());//
            writeItem(X, m.checkBox_雙擊置中.Name, m.checkBox_雙擊置中.IsChecked.Value.ToString());//
            writeItem(X, m.textBox_上一句快速鍵.Name, m.textBox_上一句快速鍵.Text);//
            writeItem(X, m.textBox_下一句快速鍵.Name, m.textBox_下一句快速鍵.Text);//
            writeItem(X, m.textBox_文字大小.Name, m.textBox_文字大小.Text);//
            writeItem(X, m.textBox_朗讀速度.Name, m.textBox_朗讀速度.Text);//
            writeItem(X, m.textBox_聲音大小.Name, m.textBox_聲音大小.Text);//
            writeItem(X, m.textBox_背景圖.Name, m.textBox_背景圖.Text);//
            writeItem(X, m.textBox_外框寬度.Name, m.textBox_外框寬度.Text);//
            writeItem(X, m.textBox_外框_羽化.Name, m.textBox_外框_羽化.Text);//
            writeItem(X, m.textBox_字幕最大寬度.Name, m.textBox_字幕最大寬度.Text);//
            //
            writeItem(X, m.color_文字顏色.Name, getColor(m.color_文字顏色.SelectedColor));//
            writeItem(X, m.color_底色.Name, getColor(m.color_底色.SelectedColor));//
            writeItem(X, m.color_前景色.Name, getColor(m.color_前景色.SelectedColor));//
            writeItem(X, m.color_外框顏色.Name, getColor(m.color_外框顏色.SelectedColor));//

            //
            if (m.radio_左.IsChecked.Value) {
                writeItem(X, "radio_text_alignment", "l");
            } else if (m.radio_中.IsChecked.Value) {
                writeItem(X, "radio_text_alignment", "c");
            } else if (m.radio_右.IsChecked.Value) {
                writeItem(X, "radio_text_alignment", "r");
            }
            //
            writeItem(X, m.comboBox_字體.Name, m.comboBox_字體.Text);//

            //
            /*
            X.WriteStartElement("item");//前景顏色
            X.WriteAttributeString("name", "Background_Color");
            X.WriteString(color_顏色);
            X.WriteEndElement();
            */

            X.WriteEndElement();

            X.Flush();     //寫這行才會寫入檔案
            X.Close();
        }


        /// <summary>
        /// 開啟程式時讀取上次設定 ( 0=讀取設定檔、1=恢復所有設定
        /// </summary>
        /// <param name="type"></param>
        public void fun_開啟程式時讀取上次設定(int type) {

            try {

                XmlDocument XmlDoc = new XmlDocument();

                if (type == 0)
                    XmlDoc.Load(pathXml);
                else if (type == 1)
                    XmlDoc.Load(pathXmlDefault);

                XmlNodeList NodeLists = XmlDoc.SelectNodes("settings/item");

                foreach (XmlNode item in NodeLists) {

                    if (item.Attributes["name"].Value.Equals(m.checkBox_播放聲音.Name))
                        m.checkBox_播放聲音.IsChecked = item.InnerText.Equals((true).ToString());
                    if (item.Attributes["name"].Value.Equals(m.checkBox_顯示字幕.Name))
                        m.checkBox_顯示字幕.IsChecked = item.InnerText.Equals((true).ToString());
                    if (item.Attributes["name"].Value.Equals(m.checkBox_雙擊置中.Name))
                        m.checkBox_雙擊置中.IsChecked = item.InnerText.Equals((true).ToString());

                    //
                    if (item.Attributes["name"].Value.Equals(m.color_文字顏色.Name))
                        SetColorComboBox(m.color_文字顏色, item.InnerText);
                    if (item.Attributes["name"].Value.Equals(m.color_底色.Name))
                        SetColorComboBox(m.color_底色, item.InnerText);
                    if (item.Attributes["name"].Value.Equals(m.color_前景色.Name))
                        SetColorComboBox(m.color_前景色, item.InnerText);
                    if (item.Attributes["name"].Value.Equals(m.color_外框顏色.Name))
                        SetColorComboBox(m.color_外框顏色, item.InnerText);
                    //
                    if (item.Attributes["name"].Value.Equals(m.textBox_上一句快速鍵.Name))
                        m.textBox_上一句快速鍵.Text = item.InnerText;
                    if (item.Attributes["name"].Value.Equals(m.textBox_下一句快速鍵.Name))
                        m.textBox_下一句快速鍵.Text = item.InnerText;
                    if (item.Attributes["name"].Value.Equals(m.textBox_文字大小.Name))
                        m.textBox_文字大小.Text = item.InnerText;
                    if (item.Attributes["name"].Value.Equals(m.textBox_背景圖.Name))
                        m.textBox_背景圖.Text = item.InnerText;
                    if (item.Attributes["name"].Value.Equals(m.textBox_朗讀速度.Name))
                        m.textBox_朗讀速度.Text = item.InnerText;
                    if (item.Attributes["name"].Value.Equals(m.textBox_聲音大小.Name))
                        m.textBox_聲音大小.Text = item.InnerText;
                    if (item.Attributes["name"].Value.Equals(m.textBox_外框寬度.Name))
                        m.textBox_外框寬度.Text = item.InnerText;
                    if (item.Attributes["name"].Value.Equals(m.textBox_外框_羽化.Name))
                        m.textBox_外框_羽化.Text = item.InnerText;
                    if (item.Attributes["name"].Value.Equals(m.textBox_字幕最大寬度.Name))
                        m.textBox_字幕最大寬度.Text = item.InnerText;

                    //
                    if (item.Attributes["name"].Value.Equals("radio_text_alignment"))
                        if (item.InnerText.Equals("l")) {
                            m.radio_左.IsChecked = true;
                        } else if (item.InnerText.Equals("c")) {
                            m.radio_中.IsChecked = true;
                        } else if (item.InnerText.Equals("r")) {
                            m.radio_右.IsChecked = true;
                        }
                    //
                    if (item.Attributes["name"].Value.Equals(m.comboBox_字體.Name))
                        m.comboBox_字體.Text = item.InnerText;

                }//for

            } catch { }

        }


        /// <summary>
        /// 設定 ColorComboBox 的顏色
        /// </summary>
        public void SetColorComboBox(ColorComboBox col, String color) {

            try {
                Byte[] x = new Byte[4];
                String[] s = color.Split(',');
                for (int i = 0; i < 4; i++) {
                    x[i] = Byte.Parse(s[i]);
                }
                Color c = new Color();
                c.A = x[0];
                c.R = x[1];
                c.G = x[2];
                c.B = x[3];
                col.SelectedColor = c;
            } catch { }
        }


        /// <summary>
        /// 從 TextBox 取得 float
        /// </summary>
        public float TextBoxGetFloat(TextBox t, float min, float max) {

            float i = 0;

            t.Text = t.Text.Replace(" ", "");//取代空白（因為textBox有BUG）

            if (t.Text == "")
                t.Text = min + "";

            try {

                bool result = float.TryParse(t.Text.ToString(), out i); //判斷是否符合int，並且把值丟給 i 

                if (result == false) {
                    i = max;
                }

                if (i < min) {//太小
                    i = min;
                    t.Text = min + "";
                } else if (i > max) {//太大
                    i = max;
                    t.Text = max + "";
                } else {//一般
                    t.Text = i + "";
                }

            } catch {
                i = min;
                t.Text = min + "";
            }

            return i;
        }


        /// <summary>
        /// 從 TextBox 取得 int
        /// </summary>
        public int TextBoxGetInt(TextBox t, int min, int max) {

            int i = 0;

            t.Text = t.Text.Replace(" ", ""); //取代空白（因為textBox有BUG）

            if (t.Text == "")
                t.Text = min + "";

            try {

                bool result = int.TryParse(t.Text.ToString(), out i); //判斷是否符合int，並且把值丟給 i 

                if (result == false) {
                    i = max;
                }

                if (i < min) {//太小
                    i = min;
                    t.Text = min + "";
                } else if (i > max) {//太大
                    i = max;
                    t.Text = max + "";
                } else {//一般
                    t.Text = i + "";
                }

            } catch {
                i = min;
                t.Text = min + "";
            }

            return i;
        }


    }

}
