using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QCDS_TestDemo.Properties;

namespace QCDS_TestDemo
{
    public partial class FormControllerSetting : Form
    {

        public FormControllerSetting()
        {
            InitializeComponent();
        }

        private void FormControllerSetting_Load(object sender, EventArgs e)
        {
            ReadAllSettings();
        }

        #region Read
        private void ReadBatchPoints()
        {
            byte[] data = new byte[2];
            byte depth;
            LJV7IF_TARGET_SETTING targetSetting = new LJV7IF_TARGET_SETTING();

            data = new byte[NativeMethods.ProgramSettingSize];
            data[0] = (byte)0x3;

            depth = Convert.ToByte("01", 16);
            targetSetting.byType = Convert.ToByte("10", 16);
            targetSetting.byCategory = Convert.ToByte("00", 16);
            targetSetting.byItem = Convert.ToByte("0A", 16);
            targetSetting.byTarget1 = Convert.ToByte("00", 16);
            targetSetting.byTarget2 = Convert.ToByte("00", 16);
            targetSetting.byTarget3 = Convert.ToByte("00", 16);
            targetSetting.byTarget4 = Convert.ToByte("00", 16);

            //string tempWord = ((int)numericUpDown2.Value).ToString("X4");
            //string trimStr = tempWord.Remove(0, 2) + "," + tempWord.Remove(2);

            //if (trimStr.Length > 0)
            //{
            //    string[] aSrc = trimStr.Split(',');
            //    if (aSrc.Length > 0)
            //    {
            //        data = Array.ConvertAll<string, byte>(aSrc,
            //            delegate (string s) { return Convert.ToByte(s, 16); });
            //    }
            //}
            Array.Resize(ref data, Convert.ToInt32("2"));

            using (PinnedObject pin = new PinnedObject(data))
            {
                int rc = NativeMethods.LJV7IF_GetSetting(Define._currentDeviceId, depth, targetSetting,
                    pin.Pointer, (uint)data.Length);
                Formmain.pMainWin.AddLogResult(rc, Resources.SID_GET_SETTING);
                if (rc == (int)Rc.Ok)
                {
                    Formmain.pMainWin.AddLog("\t    0  1  2  3  4  5  6  7");
                    StringBuilder sb = new StringBuilder();
                    // Get data display
                    for (int i = 0; i < 2; i++)
                    {
                        if ((i % 8) == 0) sb.Append(string.Format("  [0x{0:x4}] ", i));

                        sb.Append(string.Format("{0:x2} ", data[i]));
                        if (((i % 8) == 7) || (i == 2 - 1))
                        {
                            Formmain.pMainWin.AddLog(sb.ToString());
                            sb.Remove(0, sb.Length);
                        }
                    }
                }
                numericUpDown2.Value = data[1] * 256 + data[0];
                Formmain.pMainWin.AddLog(string.Format("批处理点数 = {0}", numericUpDown2.Value));
            }
        }

        private void ReadTriggerInterval()
        {
            byte[] data;
            byte depth;
            LJV7IF_TARGET_SETTING targetSetting = new LJV7IF_TARGET_SETTING();

            data = new byte[NativeMethods.ProgramSettingSize];
            data[0] = (byte)0x3;

            depth = Convert.ToByte("01", 16);
            targetSetting.byType = Convert.ToByte("10", 16);
            targetSetting.byCategory = Convert.ToByte("00", 16);
            targetSetting.byItem = Convert.ToByte("05", 16);
            targetSetting.byTarget1 = Convert.ToByte("00", 16);
            targetSetting.byTarget2 = Convert.ToByte("00", 16);
            targetSetting.byTarget3 = Convert.ToByte("00", 16);
            targetSetting.byTarget4 = Convert.ToByte("00", 16);

            //int t = (int)((float)numericUpDown1.Value / 0.001);
            //string tempWord = t.ToString("X4");
            //string trimStr = tempWord.Remove(0, 2) + "," + tempWord.Remove(2);

            //if (trimStr.Length > 0)
            //{
            //    string[] aSrc = trimStr.Split(',');
            //    if (aSrc.Length > 0)
            //    {
            //        data = Array.ConvertAll<string, byte>(aSrc,
            //            delegate (string s) { return Convert.ToByte(s, 16); });
            //    }
            //}
            Array.Resize(ref data, Convert.ToInt32("2"));

            using (PinnedObject pin = new PinnedObject(data))
            {
                int rc = NativeMethods.LJV7IF_GetSetting(Define._currentDeviceId, depth, targetSetting,
                    pin.Pointer, (uint)data.Length);
                Formmain.pMainWin.AddLogResult(rc, Resources.SID_GET_SETTING);
                if (rc == (int)Rc.Ok)
                {
                    Formmain.pMainWin.AddLog("\t    0  1  2  3  4  5  6  7");
                    StringBuilder sb = new StringBuilder();
                    // Get data display
                    for (int i = 0; i < 2; i++)
                    {
                        if ((i % 8) == 0) sb.Append(string.Format("  [0x{0:x4}] ", i));

                        sb.Append(string.Format("{0:x2} ", data[i]));
                        if (((i % 8) == 7) || (i == 2 - 1))
                        {
                            Formmain.pMainWin.AddLog(sb.ToString());
                            sb.Remove(0, sb.Length);
                        }
                    }
                }
                numericUpDown1.Value = (decimal)0.001 * (data[1] * 256 + data[0]);
                Formmain.pMainWin.AddLog(string.Format("触发间距 = {0} mm", numericUpDown1.Value));
            }
        }

        private void ReadSampleFrequency()
        {
            byte[] data;
            byte depth;
            LJV7IF_TARGET_SETTING targetSetting = new LJV7IF_TARGET_SETTING();

            data = new byte[NativeMethods.ProgramSettingSize];
            data[0] = (byte)0x3;

            depth = Convert.ToByte("01", 16);
            targetSetting.byType = Convert.ToByte("10", 16);
            targetSetting.byCategory = Convert.ToByte("00", 16);
            targetSetting.byItem = Convert.ToByte("02", 16);
            targetSetting.byTarget1 = Convert.ToByte("00", 16);
            targetSetting.byTarget2 = Convert.ToByte("00", 16);
            targetSetting.byTarget3 = Convert.ToByte("00", 16);
            targetSetting.byTarget4 = Convert.ToByte("00", 16);

            using (PinnedObject pin = new PinnedObject(data))
            {
                int rc = NativeMethods.LJV7IF_GetSetting(Define._currentDeviceId, depth, targetSetting,
                    pin.Pointer, (uint)data.Length);
                Formmain.pMainWin.AddLogResult(rc, Resources.SID_GET_SETTING);
                if (rc == (int)Rc.Ok)
                {
                    Formmain.pMainWin.AddLog("\t    0  1  2  3  4  5  6  7");
                    StringBuilder sb = new StringBuilder();
                    // Get data display
                    for (int i = 0; i < 2; i++)
                    {
                        if ((i % 8) == 0) sb.Append(string.Format("  [0x{0:x4}] ", i));

                        sb.Append(string.Format("{0:x2} ", data[i]));
                        if (((i % 8) == 7) || (i == 2 - 1))
                        {
                            Formmain.pMainWin.AddLog(sb.ToString());
                            sb.Remove(0, sb.Length);
                        }
                    }
                }
            }

            comboBox3.SelectedIndex = data[0];
            Formmain.pMainWin.AddLog(string.Format("采样频率 = {0}", comboBox3.Text));

        }

        private void ReadBatchSwitch()
        {
            byte[] data;
            byte depth;
            LJV7IF_TARGET_SETTING targetSetting = new LJV7IF_TARGET_SETTING();

            data = new byte[NativeMethods.ProgramSettingSize];
            data[0] = (byte)0x3;

            depth = Convert.ToByte("01", 16);
            targetSetting.byType = Convert.ToByte("10", 16);
            targetSetting.byCategory = Convert.ToByte("00", 16);
            targetSetting.byItem = Convert.ToByte("03", 16);
            targetSetting.byTarget1 = Convert.ToByte("00", 16);
            targetSetting.byTarget2 = Convert.ToByte("00", 16);
            targetSetting.byTarget3 = Convert.ToByte("00", 16);
            targetSetting.byTarget4 = Convert.ToByte("00", 16);

            using (PinnedObject pin = new PinnedObject(data))
            {
                int rc = NativeMethods.LJV7IF_GetSetting(Define._currentDeviceId, depth, targetSetting,
                    pin.Pointer, (uint)data.Length);
                Formmain.pMainWin.AddLogResult(rc, Resources.SID_GET_SETTING);
                if (rc == (int)Rc.Ok)
                {
                    Formmain.pMainWin.AddLog("\t    0  1  2  3  4  5  6  7");
                    StringBuilder sb = new StringBuilder();
                    // Get data display
                    for (int i = 0; i < 2; i++)
                    {
                        if ((i % 8) == 0) sb.Append(string.Format("  [0x{0:x4}] ", i));

                        sb.Append(string.Format("{0:x2} ", data[i]));
                        if (((i % 8) == 7) || (i == 2 - 1))
                        {
                            Formmain.pMainWin.AddLog(sb.ToString());
                            sb.Remove(0, sb.Length);
                        }
                    }
                }
            }
            radioButton1.Checked = (data[0] == 1);
            radioButton2.Checked = !radioButton1.Checked;
            //comboBox3.SelectedIndex = data[0];
            Formmain.pMainWin.AddLog(string.Format("批处理状态 = {0}", data[0]));

        }

        private void ReadTriggerType()
        {
            byte[] data;
            byte depth;
            LJV7IF_TARGET_SETTING targetSetting = new LJV7IF_TARGET_SETTING();

            data = new byte[NativeMethods.ProgramSettingSize];
            data[0] = (byte)0x3;

            depth = Convert.ToByte("01", 16);
            targetSetting.byType = Convert.ToByte("10", 16);
            targetSetting.byCategory = Convert.ToByte("00", 16);
            targetSetting.byItem = Convert.ToByte("01", 16);
            targetSetting.byTarget1 = Convert.ToByte("00", 16);
            targetSetting.byTarget2 = Convert.ToByte("00", 16);
            targetSetting.byTarget3 = Convert.ToByte("00", 16);
            targetSetting.byTarget4 = Convert.ToByte("00", 16);

            using (PinnedObject pin = new PinnedObject(data))
            {
                int rc = NativeMethods.LJV7IF_GetSetting(Define._currentDeviceId, depth, targetSetting,
                    pin.Pointer, (uint)data.Length);
                Formmain.pMainWin.AddLogResult(rc, Resources.SID_GET_SETTING);
                if (rc == (int)Rc.Ok)
                {
                    Formmain.pMainWin.AddLog("\t    0  1  2  3  4  5  6  7");
                    StringBuilder sb = new StringBuilder();
                    // Get data display
                    for (int i = 0; i < 2; i++)
                    {
                        if ((i % 8) == 0) sb.Append(string.Format("  [0x{0:x4}] ", i));

                        sb.Append(string.Format("{0:x2} ", data[i]));
                        if (((i % 8) == 7) || (i == 2 - 1))
                        {
                            Formmain.pMainWin.AddLog(sb.ToString());
                            sb.Remove(0, sb.Length);
                        }
                    }
                }
            }

            comboBox1.SelectedIndex = data[0];
            Formmain.pMainWin.AddLog(string.Format("触发方式为 {0}", comboBox1.Text));


        }

        private void ReadXRange()
        {
            byte[] data;
            byte depth;
            LJV7IF_TARGET_SETTING targetSetting = new LJV7IF_TARGET_SETTING();

            data = new byte[NativeMethods.ProgramSettingSize];
            data[0] = (byte)0x3;

            depth = Convert.ToByte("01", 16);
            targetSetting.byType = Convert.ToByte("10", 16);
            targetSetting.byCategory = Convert.ToByte("01", 16);
            targetSetting.byItem = Convert.ToByte("02", 16);
            targetSetting.byTarget1 = Convert.ToByte("00", 16);
            targetSetting.byTarget2 = Convert.ToByte("00", 16);
            targetSetting.byTarget3 = Convert.ToByte("00", 16);
            targetSetting.byTarget4 = Convert.ToByte("00", 16);

            using (PinnedObject pin = new PinnedObject(data))
            {
                int rc = NativeMethods.LJV7IF_GetSetting(Define._currentDeviceId, depth, targetSetting,
                    pin.Pointer, (uint)data.Length);
                Formmain.pMainWin.AddLogResult(rc, Resources.SID_GET_SETTING);
                if (rc == (int)Rc.Ok)
                {
                    Formmain.pMainWin.AddLog("\t    0  1  2  3  4  5  6  7");
                    StringBuilder sb = new StringBuilder();
                    // Get data display
                    for (int i = 0; i < 2; i++)
                    {
                        if ((i % 8) == 0) sb.Append(string.Format("  [0x{0:x4}] ", i));

                        sb.Append(string.Format("{0:x2} ", data[i]));
                        if (((i % 8) == 7) || (i == 2 - 1))
                        {
                            Formmain.pMainWin.AddLog(sb.ToString());
                            sb.Remove(0, sb.Length);
                        }
                    }
                }
            }
            comboBox2.SelectedIndex = data[0];
            int singleProfilePointCount = 400;
            switch (data[0])
            {
                case 0:
                    {
                        singleProfilePointCount = 800;
                        break;
                    }
                case 1:
                    {
                        singleProfilePointCount = 600;
                        break;
                    }
                case 2:
                    {
                        singleProfilePointCount = 400;
                        break;
                    }
            }
            Formmain.pMainWin.AddLog(string.Format("X轴方向采样范围为:{0}\t轮廓点数为{1}", comboBox2.Text, singleProfilePointCount));
        }

        private void ReadAllSettings()
        {
            ReadBatchPoints();
            ReadTriggerInterval();
            ReadSampleFrequency();
            ReadTriggerType();
            ReadXRange();
            ReadBatchSwitch();
        }
        #endregion

        #region Write
        private void WriteBatchPoints()
        {
            byte[] data;
            byte depth;
            LJV7IF_TARGET_SETTING targetSetting = new LJV7IF_TARGET_SETTING();

            data = new byte[NativeMethods.ProgramSettingSize];
            data[0] = (byte)0x3;

            depth = Convert.ToByte("01", 16);
            targetSetting.byType = Convert.ToByte("10", 16);
            targetSetting.byCategory = Convert.ToByte("00", 16);
            targetSetting.byItem = Convert.ToByte("0A", 16);
            targetSetting.byTarget1 = Convert.ToByte("00", 16);
            targetSetting.byTarget2 = Convert.ToByte("00", 16);
            targetSetting.byTarget3 = Convert.ToByte("00", 16);
            targetSetting.byTarget4 = Convert.ToByte("00", 16);


            string tempWord = ((int)numericUpDown2.Value).ToString("X4");
            string trimStr = tempWord.Remove(0, 2) + "," + tempWord.Remove(2);

            if (trimStr.Length > 0)
            {
                string[] aSrc = trimStr.Split(',');
                if (aSrc.Length > 0)
                {
                    data = Array.ConvertAll<string, byte>(aSrc,
                        delegate (string s) { return Convert.ToByte(s, 16); });
                }
            }
            Array.Resize(ref data, Convert.ToInt32("2"));

            using (PinnedObject pin = new PinnedObject(data))
            {
                uint dwError = 0;
                int rc = NativeMethods.LJV7IF_SetSetting(Define._currentDeviceId, depth, targetSetting,
                    pin.Pointer, (uint)data.Length, ref dwError);
                Formmain.pMainWin.AddLogResult(rc, Resources.SID_SET_SETTING);
                if (rc != (int)Rc.Ok)
                {
                    Formmain.pMainWin.AddError(dwError);
                }
            }
        }

        private void WriteTriggerInterval()
        {
            byte[] data;
            byte depth;
            LJV7IF_TARGET_SETTING targetSetting = new LJV7IF_TARGET_SETTING();

            data = new byte[NativeMethods.ProgramSettingSize];
            data[0] = (byte)0x3;

            depth = Convert.ToByte("01", 16);
            targetSetting.byType = Convert.ToByte("10", 16);
            targetSetting.byCategory = Convert.ToByte("00", 16);
            targetSetting.byItem = Convert.ToByte("05", 16);
            targetSetting.byTarget1 = Convert.ToByte("00", 16);
            targetSetting.byTarget2 = Convert.ToByte("00", 16);
            targetSetting.byTarget3 = Convert.ToByte("00", 16);
            targetSetting.byTarget4 = Convert.ToByte("00", 16);

            int t = (int)((float)numericUpDown1.Value / 0.001);
            string tempWord = t.ToString("X4");
            string trimStr = tempWord.Remove(0, 2) + "," + tempWord.Remove(2);

            if (trimStr.Length > 0)
            {
                string[] aSrc = trimStr.Split(',');
                if (aSrc.Length > 0)
                {
                    data = Array.ConvertAll<string, byte>(aSrc,
                        delegate (string s) { return Convert.ToByte(s, 16); });
                }
            }
            Array.Resize(ref data, Convert.ToInt32("2"));

            using (PinnedObject pin = new PinnedObject(data))
            {
                uint dwError = 0;
                int rc = NativeMethods.LJV7IF_SetSetting(Define._currentDeviceId, depth, targetSetting,
                    pin.Pointer, (uint)data.Length, ref dwError);
                Formmain.pMainWin.AddLogResult(rc, Resources.SID_SET_SETTING);
                if (rc != (int)Rc.Ok)
                {
                    Formmain.pMainWin.AddError(dwError);
                }
            }
        }

        private void WriteSampleFrequency()
        {
            byte[] data;
            byte depth;
            LJV7IF_TARGET_SETTING targetSetting = new LJV7IF_TARGET_SETTING();

            data = new byte[NativeMethods.ProgramSettingSize];
            data[0] = (byte)0x3;

            depth = Convert.ToByte("01", 16);
            targetSetting.byType = Convert.ToByte("10", 16);
            targetSetting.byCategory = Convert.ToByte("00", 16);
            targetSetting.byItem = Convert.ToByte("02", 16);
            targetSetting.byTarget1 = Convert.ToByte("00", 16);
            targetSetting.byTarget2 = Convert.ToByte("00", 16);
            targetSetting.byTarget3 = Convert.ToByte("00", 16);
            targetSetting.byTarget4 = Convert.ToByte("00", 16);

            string trimStr = "1";
            switch (comboBox3.Text)
            {
                case "10Hz":
                    {
                        trimStr = "0";
                        break;
                    }
                case "20Hz":
                    {
                        trimStr = "1";
                        break;
                    }
                case "50Hz":
                    {
                        trimStr = "2";
                        break;
                    }
                case "100Hz":
                    {
                        trimStr = "3";
                        break;
                    }
                case "200Hz":
                    {
                        trimStr = "4";
                        break;
                    }
                case "500Hz":
                    {
                        trimStr = "5";
                        break;
                    }
                case "1KHz":
                    {
                        trimStr = "6";
                        break;
                    }
                case "2KHz":
                    {
                        trimStr = "7";
                        break;
                    }
                case "4KHz":
                    {
                        trimStr = "8";
                        break;
                    }

            }
            if (trimStr.Length > 0)
            {
                string[] aSrc = trimStr.Split(',');
                if (aSrc.Length > 0)
                {
                    data = Array.ConvertAll<string, byte>(aSrc,
                        delegate (string s) { return Convert.ToByte(s, 16); });
                }
            }
            Array.Resize(ref data, Convert.ToInt32("1"));

            using (PinnedObject pin = new PinnedObject(data))
            {
                uint dwError = 0;
                int rc = NativeMethods.LJV7IF_SetSetting(Define._currentDeviceId, depth, targetSetting,
                    pin.Pointer, (uint)data.Length, ref dwError);
                Formmain.pMainWin.AddLogResult(rc, Resources.SID_SET_SETTING);
                if (rc != (int)Rc.Ok)
                {
                    Formmain.pMainWin.AddError(dwError);
                }
            }
        }

        private void WriteBatchSwitch()
        {
            byte[] data;
            byte depth;
            LJV7IF_TARGET_SETTING targetSetting = new LJV7IF_TARGET_SETTING();

            data = new byte[NativeMethods.ProgramSettingSize];
            data[0] = (byte)0x3;

            depth = Convert.ToByte("01", 16);
            targetSetting.byType = Convert.ToByte("10", 16);
            targetSetting.byCategory = Convert.ToByte("00", 16);
            targetSetting.byItem = Convert.ToByte("03", 16);
            targetSetting.byTarget1 = Convert.ToByte("00", 16);
            targetSetting.byTarget2 = Convert.ToByte("00", 16);
            targetSetting.byTarget3 = Convert.ToByte("00", 16);
            targetSetting.byTarget4 = Convert.ToByte("00", 16);

            string trimStr = "1";
            if(!radioButton1.Checked)
            {
                trimStr = "0";
            }
            if (trimStr.Length > 0)
            {
                string[] aSrc = trimStr.Split(',');
                if (aSrc.Length > 0)
                {
                    data = Array.ConvertAll<string, byte>(aSrc,
                        delegate (string s) { return Convert.ToByte(s, 16); });
                }
            }
            Array.Resize(ref data, Convert.ToInt32("1"));

            using (PinnedObject pin = new PinnedObject(data))
            {
                uint dwError = 0;
                int rc = NativeMethods.LJV7IF_SetSetting(Define._currentDeviceId, depth, targetSetting,
                    pin.Pointer, (uint)data.Length, ref dwError);
                Formmain.pMainWin.AddLogResult(rc, Resources.SID_SET_SETTING);
                if (rc != (int)Rc.Ok)
                {
                    Formmain.pMainWin.AddError(dwError);
                }
            }
        }

        private void WriteTriggerType()
        {
            byte[] data;
            byte depth;
            LJV7IF_TARGET_SETTING targetSetting = new LJV7IF_TARGET_SETTING();

            data = new byte[NativeMethods.ProgramSettingSize];
            data[0] = (byte)0x3;

            depth = Convert.ToByte("01", 16);
            targetSetting.byType = Convert.ToByte("10", 16);
            targetSetting.byCategory = Convert.ToByte("00", 16);
            targetSetting.byItem = Convert.ToByte("01", 16);
            targetSetting.byTarget1 = Convert.ToByte("00", 16);
            targetSetting.byTarget2 = Convert.ToByte("00", 16);
            targetSetting.byTarget3 = Convert.ToByte("00", 16);
            targetSetting.byTarget4 = Convert.ToByte("00", 16);

            string trimStr = "1";
            if (comboBox1.Text == "连续触发")
            {
                trimStr = "0";
            }
            else if (comboBox1.Text == "外部触发")
            {
                trimStr = "1";
            }
            else
            {
                trimStr = "2";
            }
            if (trimStr.Length > 0)
            {
                string[] aSrc = trimStr.Split(',');
                if (aSrc.Length > 0)
                {
                    data = Array.ConvertAll<string, byte>(aSrc,
                        delegate (string s) { return Convert.ToByte(s, 16); });
                }
            }
            Array.Resize(ref data, Convert.ToInt32("1"));

            using (PinnedObject pin = new PinnedObject(data))
            {
                uint dwError = 0;
                int rc = NativeMethods.LJV7IF_SetSetting(Define._currentDeviceId, depth, targetSetting,
                    pin.Pointer, (uint)data.Length, ref dwError);
                Formmain.pMainWin.AddLogResult(rc, Resources.SID_SET_SETTING);
                if (rc != (int)Rc.Ok)
                {
                    Formmain.pMainWin.AddError(dwError);
                }
            }
        }

        private void WriteXRange()
        {
            byte[] data;
            byte depth;
            LJV7IF_TARGET_SETTING targetSetting = new LJV7IF_TARGET_SETTING();

            data = new byte[NativeMethods.ProgramSettingSize];
            data[0] = (byte)0x3;

            depth = Convert.ToByte("01", 16);
            targetSetting.byType = Convert.ToByte("10", 16);
            targetSetting.byCategory = Convert.ToByte("01", 16);
            targetSetting.byItem = Convert.ToByte("02", 16);
            targetSetting.byTarget1 = Convert.ToByte("00", 16);
            targetSetting.byTarget2 = Convert.ToByte("00", 16);
            targetSetting.byTarget3 = Convert.ToByte("00", 16);
            targetSetting.byTarget4 = Convert.ToByte("00", 16);

            string trimStr = "0";
            if (trimStr.Length > 0)
            {
                string[] aSrc = trimStr.Split(',');
                if (aSrc.Length > 0)
                {
                    data = Array.ConvertAll<string, byte>(aSrc,
                        delegate (string s) { return Convert.ToByte(s, 16); });
                }
            }
            Array.Resize(ref data, Convert.ToInt32("1"));

            using (PinnedObject pin = new PinnedObject(data))
            {
                uint dwError = 0;
                int rc = NativeMethods.LJV7IF_SetSetting(Define._currentDeviceId, depth, targetSetting,
                    pin.Pointer, (uint)data.Length, ref dwError);
                Formmain.pMainWin.AddLogResult(rc, Resources.SID_SET_SETTING);
                if (rc != (int)Rc.Ok)
                {
                    Formmain.pMainWin.AddError(dwError);
                }
            }
        }

        private void WriteAllSettings()
        {
            WriteBatchPoints();
            WriteTriggerInterval();
            WriteSampleFrequency();
            WriteTriggerType();
            WriteXRange();
            WriteBatchSwitch();
            ReadAllSettings();
        }
        #endregion

        private void btSensorSetting_Click(object sender, EventArgs e)
        {
            WriteAllSettings();
        }
    }
}
