using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using QCDS_TestDemo;
using QCDS_TestDemo.Properties;
using QCDS_TestDemo.Datas;
using System.Windows.Forms.DataVisualization.Charting;
using MongoDB.Bson;
using MongoDB.Driver;

namespace QCDS_TestDemo
{
    public partial class Form1 : Form
    {
        #region Enum

        /// <summary>
        /// Send command definition
        /// </summary>
        /// <remark>Defined for separate return code distinction</remark>
        public enum SendCommand
        {
            /// <summary>None</summary>
            None,
            /// <summary>Restart</summary>
            RebootController,
            /// <summary>Trigger</summary>
            Trigger,
            /// <summary>Start measurement</summary>
            StartMeasure,
            /// <summary>Stop measurement</summary>
            StopMeasure,
            /// <summary>Auto zero</summary>
            AutoZero,
            /// <summary>Timing</summary>
            Timing,
            /// <summary>Reset</summary>
            Reset,
            /// <summary>Program switch</summary>
            ChangeActiveProgram,
            /// <summary>Get measurement results</summary>
            GetMeasurementValue,

            /// <summary>Get profiles</summary>
            GetProfile,
            /// <summary>Get batch profiles (operation mode "high-speed (profile only)")</summary>
            GetBatchProfile,
            /// <summary>Get profiles (operation mode "advanced (with OUT measurement)")</summary>
            GetProfileAdvance,
            /// <summary>Get batch profiles (operation mode "advanced (with OUT measurement)").</summary>
            GetBatchProfileAdvance,

            /// <summary>Start storage</summary>
            StartStorage,
            /// <summary>Stop storage</summary>
            StopStorage,
            /// <summary>Get storage status</summary>
            GetStorageStatus,
            /// <summary>Manual storage request</summary>
            RequestStorage,
            /// <summary>Get storage data</summary>
            GetStorageData,
            /// <summary>Get profile storage data</summary>
            GetStorageProfile,
            /// <summary>Get batch profile storage data.</summary>
            GetStorageBatchProfile,

            /// <summary>Initialize USB high-speed data communication</summary>
            HighSpeedDataUsbCommunicationInitalize,
            /// <summary>Initialize Ethernet high-speed data communication</summary>
            HighSpeedDataEthernetCommunicationInitalize,
            /// <summary>Request preparation before starting high-speed data communication</summary>
            PreStartHighSpeedDataCommunication,
            /// <summary>Start high-speed data communication</summary>
            StartHighSpeedDataCommunication,
        }

        #endregion

        #region Struct
        private struct BatchRecord
        {
            public string date;
            public string time;
            public int profileCount;
            public int pointCount;
            public double interval;
            public List<int[]> datas;
        }
        #endregion

        #region Field

        /// <summary>Ethernet settings structure </summary>
        private LJV7IF_ETHERNET_CONFIG _ethernetConfig;
        /// <summary>Measurement data list</summary>
        private List<MeasureData> _measureDatas;
        /// <summary>Current device ID</summary>
        private int _currentDeviceId = 0;
        /// <summary>Send command</summary>
        private SendCommand _sendCommand;
        /// <summary>Callback function used during high-speed communication</summary>
        private HighSpeedDataCallBack _callback;
        /// <summary>Callback function used during high-speed communication (count only)</summary>
        //private HighSpeedDataCallBack _callbackOnlyCount;

        /// The following are maintained in arrays to support multiple controllers.
        /// <summary>Array of profile information structures</summary>
        private LJV7IF_PROFILE_INFO[] _profileInfo;
        /// <summary>Array of controller information</summary>
        private DeviceData[] _deviceData;

        private int currentBatchProfileCount = 0;

        private int singleProfilePointCount = 400;

        private bool singleBatchFinish = true;

        private List<int[]> currentDatas;

        private List<double> xscale = new List<double>();
        #endregion



        public Form1()
        {
            InitializeComponent();
            _sendCommand = SendCommand.None;
            _deviceData = new DeviceData[NativeMethods.DeviceCount];
            _measureDatas = new List<MeasureData>();
            _callback = new HighSpeedDataCallBack(ReceiveHighSpeedData);
            //_callbackOnlyCount = new HighSpeedDataCallBack(CountProfileReceive);
            

            for (int i = 0; i < NativeMethods.DeviceCount; i++)
            {
                _deviceData[i] = new DeviceData();
                //_deviceStatusLabels[i].Text = _deviceData[i].GetStatusString();
            }
            _profileInfo = new LJV7IF_PROFILE_INFO[NativeMethods.DeviceCount];
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            OpenEthernetConnection();
            ReadAllSettings();
        }

        private void _btnHighSpeedDataEthernetCommunicationInitalize_Click(object sender, EventArgs e)
        {
            _sendCommand = SendCommand.HighSpeedDataEthernetCommunicationInitalize;


            _deviceData[_currentDeviceId].ProfileData.Clear();  //Clear the retained profile data.
            _deviceData[_currentDeviceId].MeasureData.Clear();

            LJV7IF_ETHERNET_CONFIG ethernetConfig = _ethernetConfig;
            
            int rc = NativeMethods.LJV7IF_HighSpeedDataEthernetCommunicationInitalize(_currentDeviceId, ref ethernetConfig,
                UInt16.Parse(_txtHighSpeedPort.Text), _callback,
                1, (uint)_currentDeviceId);
           
            AddLogResult(rc, Resources.SID_HIGH_SPEED_DATA_ETHERNET_COMMUNICATION_INITALIZE);

            if (rc == (int)Rc.Ok)
            {
                _deviceData[_currentDeviceId].Status = DeviceStatus.EthernetFast;
                _deviceData[_currentDeviceId].EthernetConfig = ethernetConfig;
            }
            //_deviceStatusLabels[_currentDeviceId].Text = _deviceData[_currentDeviceId].GetStatusString();

            
        }

        private void _btnPreStartHighSpeedDataCommunication_Click(object sender, EventArgs e)
        {
            _sendCommand = SendCommand.PreStartHighSpeedDataCommunication;


            LJV7IF_HIGH_SPEED_PRE_START_REQ req = new LJV7IF_HIGH_SPEED_PRE_START_REQ();
            req.bySendPos = Convert.ToByte("2");
            // @Point
            // # SendPos is used to specify which profile to start sending data from during high-speed communication.
            // # When "Overwrite" is specified for the operation when memory full and 
            //   "0: From previous send complete position" is specified for the send start position,
            //    if the LJ-V continues to accumulate profiles, the LJ-V memory will become full,
            //    and the profile at the previous send complete position will be overwritten with a new profile.
            //    In this situation, because the profile at the previous send complete position is not saved, an error will occur.

            LJV7IF_PROFILE_INFO profileInfo = new LJV7IF_PROFILE_INFO();

            int rc = NativeMethods.LJV7IF_PreStartHighSpeedDataCommunication(_currentDeviceId, ref req, ref profileInfo);
            AddLogResult(rc, Resources.SID_PRE_START_HIGH_SPEED_DATA_COMMUNICATION);
            if (rc == (int)Rc.Ok)
            {
                // Response data display
                AddLog(Utility.ConvertToLogString(profileInfo).ToString());
                _profileInfo[_currentDeviceId] = profileInfo;
            }
        }

        private void _btnStartHighSpeedDataCommunication_Click(object sender, EventArgs e)
        {
            _sendCommand = SendCommand.StartHighSpeedDataCommunication;

            ThreadSafeBuffer.ClearBuffer(_currentDeviceId);
            _profileCount.Text = "0";
            currentProfileCount.Text = "0";
            int rc = NativeMethods.LJV7IF_StartHighSpeedDataCommunication(_currentDeviceId);
            // @Point
            //  If the LJ-V does not measure the profile for 30 seconds from the start of transmission,
            //  "0x00000008" is stored in dwNotify of the callback function.
            currentBatchProfileCount = 0;
            timer1.Start();
            AddLogResult(rc, Resources.SID_START_HIGH_SPEED_DATA_COMMUNICATION);
        }

        private void _btnStopHighSpeedDataCommunication_Click(object sender, EventArgs e)
        {
            int rc = NativeMethods.LJV7IF_StopHighSpeedDataCommunication(_currentDeviceId);
            timer1.Stop();
            AddLogResult(rc, Resources.SID_STOP_HIGH_SPEED_DATA_COMMUNICATION);
        }

        private void _btnHighSpeedDataCommunicationFinalize_Click(object sender, EventArgs e)
        {
            int rc = NativeMethods.LJV7IF_HighSpeedDataCommunicationFinalize(_currentDeviceId);
            AddLogResult(rc, Resources.SID_HIGH_SPEED_DATA_COMMUNICATION_FINALIZE);

            LJV7IF_ETHERNET_CONFIG config = _deviceData[_currentDeviceId].EthernetConfig;
            _deviceData[_currentDeviceId].Status = DeviceStatus.Ethernet;
            _deviceData[_currentDeviceId].EthernetConfig = config;

            timer1.Stop();
            profileClear();
            //_deviceStatusLabels[_currentDeviceId].Text = _deviceData[_currentDeviceId].GetStatusString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenEthernetConnection();
        }

        private void button2_Click(object sender, EventArgs e)
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
            if(comboBox1.Text == "连续触发")
            {
                trimStr = "0";
            }
            else
            {
                trimStr = "1";
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
                int rc = NativeMethods.LJV7IF_SetSetting(_currentDeviceId, depth, targetSetting,
                    pin.Pointer, (uint)data.Length, ref dwError);
                AddLogResult(rc, Resources.SID_SET_SETTING);
                if (rc != (int)Rc.Ok)
                {
                    AddError(dwError);
                }
            }
            ReadTriggerType();
        }

        private void button3_Click(object sender, EventArgs e)
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
                int rc = NativeMethods.LJV7IF_SetSetting(_currentDeviceId, depth, targetSetting,
                    pin.Pointer, (uint)data.Length, ref dwError);
                AddLogResult(rc, Resources.SID_SET_SETTING);
                if (rc != (int)Rc.Ok)
                {
                    AddError(dwError);
                }
            }
            ReadTriggerInterval();
        }

        private void button4_Click(object sender, EventArgs e)
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
            string trimStr = tempWord.Remove(0,2) + "," + tempWord.Remove(2);
            
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
                int rc = NativeMethods.LJV7IF_SetSetting(_currentDeviceId, depth, targetSetting,
                    pin.Pointer, (uint)data.Length, ref dwError);
                AddLogResult(rc, Resources.SID_SET_SETTING);
                if (rc != (int)Rc.Ok)
                {
                    AddError(dwError);
                }
            }
            ReadBatchPoints();
        }

        private void button5_Click(object sender, EventArgs e)
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
            switch (comboBox2.Text)
            {
                case "FULL":
                    {
                        trimStr = "0";
                        singleProfilePointCount = 800;
                        break;
                    }
                case "MIDDLE":
                    {
                        trimStr = "1";
                        singleProfilePointCount = 600;
                        break;
                    }
                case "SMALL":
                    {
                        trimStr = "2";
                        singleProfilePointCount = 400;
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
                int rc = NativeMethods.LJV7IF_SetSetting(_currentDeviceId, depth, targetSetting,
                    pin.Pointer, (uint)data.Length, ref dwError);
                AddLogResult(rc, Resources.SID_SET_SETTING);
                if (rc != (int)Rc.Ok)
                {
                    AddError(dwError);
                }
            }
            ReadXRange();
        }

        private void button6_Click(object sender, EventArgs e)
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
                int rc = NativeMethods.LJV7IF_SetSetting(_currentDeviceId, depth, targetSetting,
                    pin.Pointer, (uint)data.Length, ref dwError);
                AddLogResult(rc, Resources.SID_SET_SETTING);
                if (rc != (int)Rc.Ok)
                {
                    AddError(dwError);
                }
            }
            ReadSampleFrequency();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            ReadAllSettings();
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                ChartFigure(currentDatas[(int)numericUpDown3.Value]);
            }
            catch { }
            
        }

        private void _btnInitialize_Click(object sender, EventArgs e)
        {
            int rc = NativeMethods.LJV7IF_Initialize();
            AddLogResult(rc, Resources.SID_INITIALIZE);

            for (int i = 0; i < _deviceData.Length; i++)
            {
                _deviceData[i].Status = DeviceStatus.NoConnection;
                //_deviceStatusLabels[i].Text = _deviceData[i].GetStatusString();
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int count = 0;
            uint notify = 0;
            int batchNo = 0;
            for (int i = 0; i < NativeMethods.DeviceCount; i++)
            {
                List<int[]> data = ThreadSafeBuffer.Get(i, out notify, out batchNo);
                
                if (data.Count != 0)
                {
                    if(singleBatchFinish)
                    {
                        currentProfileCount.Text = "0";
                        currentDatas = new List<int[]>();
                    }
                    singleBatchFinish = false;
                    foreach (int[] profile in data)
                    {
                        if (_deviceData[i].ProfileData.Count < Define.WRITE_DATA_SIZE)
                        {
                            _deviceData[i].ProfileData.Add(new ProfileData(profile, _profileInfo[i]));
                            int[] temp = new int[singleProfilePointCount * 2];
                            Array.Copy(profile, 6, temp, 0, singleProfilePointCount * 2);
                            currentDatas.Add(temp);
                            //ChartFigure(temp);
                        }
                        count++;
                        currentBatchProfileCount++;
                        numericUpDown3.Maximum = currentBatchProfileCount - 1;
                        numericUpDown3.Value = currentBatchProfileCount - 1;
                    }
                    _profileCount.Text = (Convert.ToInt32(_profileCount.Text) + count).ToString();
                    currentProfileCount.Text = (Convert.ToInt32(currentProfileCount.Text) + count).ToString();
                }
                if (notify != 0)
                {
                    AddLog(string.Format("当前批计数达到上限,或中断当前批处理"));
                    AddLog(string.Format("  notify[{0}] = {1,0:x8}\tbatch[{0}] = {2}", i, notify, batchNo));
                    AddLog(string.Format("当前批次轮廓数量 = {0}\t总计获得轮廓数量 = {1}", currentBatchProfileCount,_profileCount.Text));
                    currentBatchProfileCount = 0;
                    singleBatchFinish = true;
                    numericUpDown3.Maximum = decimal.Parse(currentProfileCount.Text) - 1;
                    numericUpDown3.Value = decimal.Parse(currentProfileCount.Text) - 1;
                }

            }
            
        }

        private void chart1_GetToolTipText(object sender, System.Windows.Forms.DataVisualization.Charting.ToolTipEventArgs e)
        {
            if (e.HitTestResult.ChartElementType == ChartElementType.DataPoint)
            {
                int i = e.HitTestResult.PointIndex;
                DataPoint dp = e.HitTestResult.Series.Points[i];
                //分别显示x轴和y轴的数值，其中{1:F3},表示显示的是float类型，精确到小数点后3位。  
                e.Text = string.Format("X:{0:F2}\tY:{1:F6} ", dp.XValue, dp.YValues[0]);
            }


        }

        private void chart2_GetToolTipText(object sender, ToolTipEventArgs e)
        {
            if (e.HitTestResult.ChartElementType == ChartElementType.DataPoint)
            {
                int i = e.HitTestResult.PointIndex;
                DataPoint dp = e.HitTestResult.Series.Points[i];
                //分别显示x轴和y轴的数值，其中{1:F3},表示显示的是float类型，精确到小数点后3位。  
                e.Text = string.Format("X:{0:F2}\tY:{1:F6} ", dp.XValue, dp.YValues[0]);
            }
        }






        #region Log output

        /// <summary>
        /// Log output
        /// </summary>
        /// <param name="strLog">Output log</param>
        private void AddLog(string strLog)
        {
            _txtboxLog.AppendText(strLog + Environment.NewLine);
            _txtboxLog.SelectionStart = _txtboxLog.Text.Length;
            _txtboxLog.Focus();
            _txtboxLog.ScrollToCaret();
        }

        /// <summary>
        /// Communication command result log output
        /// </summary>
        /// <param name="rc">Return code from the DLL</param>
        /// <param name="commandName">Command name to be output in the log</param>
        private void AddLogResult(int rc, string commandName)
        {
            if (rc == (int)Rc.Ok)
            {
                AddLog(string.Format(Resources.SID_LOG_FORMAT, commandName, Resources.SID_RESULT_OK, rc));
            }
            else
            {
                AddLog(string.Format(Resources.SID_LOG_FORMAT, commandName, Resources.SID_RESULT_NG, rc));
                AddErrorLog(rc);
            }
        }

        /// <summary>
        /// Error log output
        /// </summary>
        /// <param name="rc">Return code</param>
        private void AddErrorLog(int rc)
        {
            if (rc < 0x8000)
            {
                // Common return code
                CommonErrorLog(rc);
            }
            else
            {
                // Individual return code
                IndividualErrorLog(rc);
            }
        }

        /// <summary>
        /// Add Error
        /// </summary>
        /// <param name="dwError"></param>
        private void AddError(uint dwError)
        {
            _txtboxLog.AppendText("  ErrorCode : 0x" + dwError.ToString("x8") + Environment.NewLine);
            _txtboxLog.SelectionStart = _txtboxLog.Text.Length;
            _txtboxLog.Focus();
            _txtboxLog.ScrollToCaret();
        }

        /// <summary>
        /// Common return code log output
        /// </summary>
        /// <param name="rc">Return code</param>
        private void CommonErrorLog(int rc)
        {
            switch (rc)
            {
                case (int)Rc.Ok:
                    AddLog(string.Format(Resources.SID_RC_FORMAT, Resources.SID_RC_OK));
                    break;
                case (int)Rc.ErrOpenDevice:
                    AddLog(string.Format(Resources.SID_RC_FORMAT, Resources.SID_RC_ERR_OPEN_DEVICE));
                    break;
                case (int)Rc.ErrNoDevice:
                    AddLog(string.Format(Resources.SID_RC_FORMAT, Resources.SID_RC_ERR_NO_DEVICE));
                    break;
                case (int)Rc.ErrSend:
                    AddLog(string.Format(Resources.SID_RC_FORMAT, Resources.SID_RC_ERR_SEND));
                    break;
                case (int)Rc.ErrReceive:
                    AddLog(string.Format(Resources.SID_RC_FORMAT, Resources.SID_RC_ERR_RECEIVE));
                    break;
                case (int)Rc.ErrTimeout:
                    AddLog(string.Format(Resources.SID_RC_FORMAT, Resources.SID_RC_ERR_TIMEOUT));
                    break;
                case (int)Rc.ErrParameter:
                    AddLog(string.Format(Resources.SID_RC_FORMAT, Resources.SID_RC_ERR_PARAMETER));
                    break;
                case (int)Rc.ErrNomemory:
                    AddLog(string.Format(Resources.SID_RC_FORMAT, Resources.SID_RC_ERR_NOMEMORY));
                    break;
                default:
                    AddLog(string.Format(Resources.SID_NOT_DEFINE_FROMAT, rc));
                    break;
            }
        }

        /// <summary>
        /// Individual return code log output
        /// </summary>
        /// <param name="rc">Return code</param>
        private void IndividualErrorLog(int rc)
        {
            switch (_sendCommand)
            {
                case SendCommand.RebootController:
                    {
                        switch (rc)
                        {
                            case 0x80A0:
                                AddLog(string.Format(Resources.SID_RC_FORMAT, @"Accessing the save area"));
                                break;
                            default:
                                AddLog(string.Format(Resources.SID_NOT_DEFINE_FROMAT, rc));
                                break;
                        }
                    }
                    break;
                case SendCommand.Trigger:
                    {
                        switch (rc)
                        {
                            case 0x8080:
                                AddLog(string.Format(Resources.SID_RC_FORMAT, @"The trigger mode is not [external trigger]"));
                                break;
                            default:
                                AddLog(string.Format(Resources.SID_NOT_DEFINE_FROMAT, rc));
                                break;
                        }
                    }
                    break;
                case SendCommand.StartMeasure:
                case SendCommand.StopMeasure:
                    {
                        switch (rc)
                        {
                            case 0x8080:
                                AddLog(string.Format(Resources.SID_RC_FORMAT, @"Batch measurements are off"));
                                break;
                            case 0x80A0:
                                AddLog(string.Format(Resources.SID_RC_FORMAT, @"Batch measurement start processing could not be performed because the REMOTE terminal is off or the LASER_OFF terminal is on"));
                                break;
                            default:
                                AddLog(string.Format(Resources.SID_NOT_DEFINE_FROMAT, rc));
                                break;
                        }
                    }
                    break;
                case SendCommand.AutoZero:
                case SendCommand.Timing:
                case SendCommand.Reset:
                case SendCommand.GetMeasurementValue:
                    {
                        switch (rc)
                        {
                            case 0x8080:
                                AddLog(string.Format(Resources.SID_RC_FORMAT, @"The operation mode is [high-speed (profile only)]"));
                                break;
                            default:
                                AddLog(string.Format(Resources.SID_NOT_DEFINE_FROMAT, rc));
                                break;
                        }
                    }
                    break;
                case SendCommand.ChangeActiveProgram:
                    {
                        switch (rc)
                        {
                            case 0x8080:
                                AddLog(string.Format(Resources.SID_RC_FORMAT, @"The change program setting is [terminal]"));
                                break;
                            default:
                                AddLog(string.Format(Resources.SID_NOT_DEFINE_FROMAT, rc));
                                break;
                        }
                    }
                    break;
                case SendCommand.GetProfile:
                case SendCommand.GetProfileAdvance:
                    {
                        switch (rc)
                        {
                            case 0x8080:
                                AddLog(string.Format(Resources.SID_RC_FORMAT, @"The operation mode is [advanced (with OUT measurement)]"));
                                break;
                            case 0x8081:
                                AddLog(string.Format(Resources.SID_RC_FORMAT, @"Batch measurements on and profile compression (time axis) off"));
                                break;
                            case 0x80A0:
                                AddLog(string.Format(Resources.SID_RC_FORMAT, @"No profile data"));
                                break;
                            default:
                                AddLog(string.Format(Resources.SID_NOT_DEFINE_FROMAT, rc));
                                break;
                        }
                    }
                    break;
                case SendCommand.GetBatchProfile:
                case SendCommand.GetBatchProfileAdvance:
                    {
                        switch (rc)
                        {
                            case 0x8080:
                                AddLog(string.Format(Resources.SID_RC_FORMAT, @"The operation mode is [advanced (with OUT measurement)]"));
                                break;
                            case 0x8081:
                                AddLog(string.Format(Resources.SID_RC_FORMAT, @"Not [batch measurements on and profile compression (time axis) off]"));
                                break;
                            case 0x80A0:
                                AddLog(string.Format(Resources.SID_RC_FORMAT, @"No batch data (batch measurements not run even once)"));
                                break;
                            default:
                                AddLog(string.Format(Resources.SID_NOT_DEFINE_FROMAT, rc));
                                break;
                        }
                    }
                    break;

                case SendCommand.StartStorage:
                case SendCommand.StopStorage:
                    {
                        switch (rc)
                        {
                            case 0x8080:
                                AddLog(string.Format(Resources.SID_RC_FORMAT, @"The operation mode is [high-speed (profile only)]"));
                                break;
                            case 0x8081:
                                AddLog(string.Format(Resources.SID_RC_FORMAT, @"Storage target setting is [OFF] (no storage)"));
                                break;
                            case 0x8082:
                                AddLog(string.Format(Resources.SID_RC_FORMAT, @"The storage condition setting is not [terminal/command]"));
                                break;
                            default:
                                AddLog(string.Format(Resources.SID_NOT_DEFINE_FROMAT, rc));
                                break;
                        }
                    }
                    break;
                case SendCommand.GetStorageStatus:
                    {
                        switch (rc)
                        {
                            case 0x8080:
                                AddLog(string.Format(Resources.SID_RC_FORMAT, @"The operation mode is [high-speed (profile only)]"));
                                break;
                            default:
                                AddLog(string.Format(Resources.SID_NOT_DEFINE_FROMAT, rc));
                                break;
                        }
                    }
                    break;
                case SendCommand.GetStorageData:
                    {
                        switch (rc)
                        {
                            case 0x8080:
                                AddLog(string.Format(Resources.SID_RC_FORMAT, @"The operation mode is [high-speed (profile only)]"));
                                break;
                            case 0x8081:
                                AddLog(string.Format(Resources.SID_RC_FORMAT, @"The storage target setting is not [OUT value]"));
                                break;
                            default:
                                AddLog(string.Format(Resources.SID_NOT_DEFINE_FROMAT, rc));
                                break;
                        }
                    }
                    break;
                case SendCommand.GetStorageProfile:
                    {
                        switch (rc)
                        {
                            case 0x8080:
                                AddLog(string.Format(Resources.SID_RC_FORMAT, @"The operation mode is [high-speed (profile only)]"));
                                break;
                            case 0x8081:
                                AddLog(string.Format(Resources.SID_RC_FORMAT, @"The storage target setting is not profile, or batch measurements on and profile compression (time axis) off"));
                                break;
                            case 0x8082:
                                AddLog(string.Format(Resources.SID_RC_FORMAT, @"Batch measurements on and profile compression (time axis) off"));
                                break;
                            default:
                                AddLog(string.Format(Resources.SID_NOT_DEFINE_FROMAT, rc));
                                break;
                        }
                    }
                    break;
                case SendCommand.GetStorageBatchProfile:
                    {
                        switch (rc)
                        {
                            case 0x8080:
                                AddLog(string.Format(Resources.SID_RC_FORMAT, @"The operation mode is [high-speed (profile only)]"));
                                break;
                            case 0x8081:
                                AddLog(string.Format(Resources.SID_RC_FORMAT, @"The storage target setting is not profile, or batch measurements on and profile compression (time axis) off"));
                                break;
                            case 0x8082:
                                AddLog(string.Format(Resources.SID_RC_FORMAT, @"Not [batch measurements on and profile compression (time axis) off]"));
                                break;
                            default:
                                AddLog(string.Format(Resources.SID_NOT_DEFINE_FROMAT, rc));
                                break;
                        }
                    }
                    break;
                case SendCommand.HighSpeedDataUsbCommunicationInitalize:
                case SendCommand.HighSpeedDataEthernetCommunicationInitalize:
                    {
                        switch (rc)
                        {
                            case 0x8080:
                                AddLog(string.Format(Resources.SID_RC_FORMAT, @"The operation mode is [advanced (with OUT measurement)]"));
                                break;
                            case 0x80A1:
                                AddLog(string.Format(Resources.SID_RC_FORMAT, @"Already performing high-speed communication error (for high-speed communication)"));
                                break;
                            default:
                                AddLog(string.Format(Resources.SID_NOT_DEFINE_FROMAT, rc));
                                break;
                        }
                    }
                    break;
                case SendCommand.PreStartHighSpeedDataCommunication:
                case SendCommand.StartHighSpeedDataCommunication:
                    {
                        switch (rc)
                        {
                            case 0x8080:
                                AddLog(string.Format(Resources.SID_RC_FORMAT, @"The operation mode is [advanced (with OUT measurement)]"));
                                break;
                            case 0x8081:
                                AddLog(string.Format(Resources.SID_RC_FORMAT, @"The data specified as the send start position does not exist"));
                                break;
                            case 0x80A0:
                                AddLog(string.Format(Resources.SID_RC_FORMAT, @"A high-speed data communication connection was not established"));
                                break;
                            case 0x80A1:
                                AddLog(string.Format(Resources.SID_RC_FORMAT, @"Already performing high-speed communication error (for high-speed communication)"));
                                break;
                            case 0x80A4:
                                AddLog(string.Format(Resources.SID_RC_FORMAT, @"The send target data was cleared"));
                                break;
                            default:
                                AddLog(string.Format(Resources.SID_NOT_DEFINE_FROMAT, rc));
                                break;
                        }
                    }
                    break;
                default:
                    AddLog(string.Format(Resources.SID_NOT_DEFINE_FROMAT, rc));
                    break;
            }
        }

        private bool CheckReturnCode(Rc rc)
        {
            if (rc == Rc.Ok) return true;
            MessageBox.Show(this, string.Format("Error: 0x{0,8:x}", rc), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        #endregion

        public static void ReceiveHighSpeedData(IntPtr buffer, uint size, uint count, uint notify, uint user)
        {
            // @Point
            // Take care to only implement storing profile data in a thread save buffer in the callback function.
            // As the thread used to call the callback function is the same as the thread used to receive data,
            // the processing time of the callback function affects the speed at which data is received,
            // and may stop communication from being performed properly in some environments.

            uint profileSize = (uint)(size / Marshal.SizeOf(typeof(int)));
            List<int[]> receiveBuffer = new List<int[]>();
            int[] bufferArray = new int[profileSize * count];
            Marshal.Copy(buffer, bufferArray, 0, (int)(profileSize * count));

            // Profile data retention
            for (int i = 0; i < count; i++)
            {
                int[] oneProfile = new int[profileSize];
                Array.Copy(bufferArray, i * profileSize, oneProfile, 0, profileSize);
                receiveBuffer.Add(oneProfile);
            }

            ThreadSafeBuffer.Add((int)user, receiveBuffer, notify);
        }

        private void ChartFigure(int[] profile)
        {
            
            float[] A = new float[singleProfilePointCount];
            float[] B = new float[singleProfilePointCount];
            List<float> p = new List<float>();
            foreach(int x in profile)
            {
                p.Add((float)x / 100000);
            }
            Array.Copy(p.ToArray(), 0, A, 0, singleProfilePointCount);
            Array.Copy(p.ToArray(), singleProfilePointCount, B, 0, singleProfilePointCount);
            //chart1.Series["Profile"].Points.DataBindY(A);
            //chart2.Series["Profile"].Points.DataBindY(B);
            chart1.Series["Profile"].Points.DataBindXY(xscale, A);
            chart2.Series["Profile"].Points.DataBindXY(xscale, B);
        }

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
                int rc = NativeMethods.LJV7IF_GetSetting(_currentDeviceId, depth, targetSetting,
                    pin.Pointer, (uint)data.Length);
                AddLogResult(rc, Resources.SID_GET_SETTING);
                if (rc == (int)Rc.Ok)
                {
                    AddLog("\t    0  1  2  3  4  5  6  7");
                    StringBuilder sb = new StringBuilder();
                    // Get data display
                    for (int i = 0; i < 2; i++)
                    {
                        if ((i % 8) == 0) sb.Append(string.Format("  [0x{0:x4}] ", i));

                        sb.Append(string.Format("{0:x2} ", data[i]));
                        if (((i % 8) == 7) || (i == 2 - 1))
                        {
                            AddLog(sb.ToString());
                            sb.Remove(0, sb.Length);
                        }
                    }
                }
                numericUpDown2.Value = data[1] * 256 + data[0];
                numericUpDown3.Maximum = data[1] * 256 + data[0] - 1;
                AddLog(string.Format("批处理点数 = {0}",numericUpDown2.Value));
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
                int rc = NativeMethods.LJV7IF_GetSetting(_currentDeviceId, depth, targetSetting,
                    pin.Pointer, (uint)data.Length);
                AddLogResult(rc, Resources.SID_GET_SETTING);
                if (rc == (int)Rc.Ok)
                {
                    AddLog("\t    0  1  2  3  4  5  6  7");
                    StringBuilder sb = new StringBuilder();
                    // Get data display
                    for (int i = 0; i < 2; i++)
                    {
                        if ((i % 8) == 0) sb.Append(string.Format("  [0x{0:x4}] ", i));

                        sb.Append(string.Format("{0:x2} ", data[i]));
                        if (((i % 8) == 7) || (i == 2 - 1))
                        {
                            AddLog(sb.ToString());
                            sb.Remove(0, sb.Length);
                        }
                    }
                }
                numericUpDown1.Value = (decimal)0.001*(data[1] * 256 + data[0]);
                AddLog(string.Format("触发间距 = {0} mm", numericUpDown1.Value));
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
                int rc = NativeMethods.LJV7IF_GetSetting(_currentDeviceId, depth, targetSetting,
                    pin.Pointer, (uint)data.Length);
                AddLogResult(rc, Resources.SID_GET_SETTING);
                if (rc == (int)Rc.Ok)
                {
                    AddLog("\t    0  1  2  3  4  5  6  7");
                    StringBuilder sb = new StringBuilder();
                    // Get data display
                    for (int i = 0; i < 2; i++)
                    {
                        if ((i % 8) == 0) sb.Append(string.Format("  [0x{0:x4}] ", i));

                        sb.Append(string.Format("{0:x2} ", data[i]));
                        if (((i % 8) == 7) || (i == 2 - 1))
                        {
                            AddLog(sb.ToString());
                            sb.Remove(0, sb.Length);
                        }
                    }
                }
            }

            comboBox3.SelectedIndex = data[0];
            AddLog(string.Format("采样频率 = {0}", comboBox3.Text));

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
                int rc = NativeMethods.LJV7IF_GetSetting(_currentDeviceId, depth, targetSetting,
                    pin.Pointer, (uint)data.Length);
                AddLogResult(rc, Resources.SID_GET_SETTING);
                if (rc == (int)Rc.Ok)
                {
                    AddLog("\t    0  1  2  3  4  5  6  7");
                    StringBuilder sb = new StringBuilder();
                    // Get data display
                    for (int i = 0; i < 2; i++)
                    {
                        if ((i % 8) == 0) sb.Append(string.Format("  [0x{0:x4}] ", i));

                        sb.Append(string.Format("{0:x2} ", data[i]));
                        if (((i % 8) == 7) || (i == 2 - 1))
                        {
                            AddLog(sb.ToString());
                            sb.Remove(0, sb.Length);
                        }
                    }
                }
            }

            comboBox1.SelectedIndex = data[0];
            AddLog(string.Format("触发方式为 {0}", comboBox1.Text));
            

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
                int rc = NativeMethods.LJV7IF_GetSetting(_currentDeviceId, depth, targetSetting,
                    pin.Pointer, (uint)data.Length);
                AddLogResult(rc, Resources.SID_GET_SETTING);
                if (rc == (int)Rc.Ok)
                {
                    AddLog("\t    0  1  2  3  4  5  6  7");
                    StringBuilder sb = new StringBuilder();
                    // Get data display
                    for (int i = 0; i < 2; i++)
                    {
                        if ((i % 8) == 0) sb.Append(string.Format("  [0x{0:x4}] ", i));

                        sb.Append(string.Format("{0:x2} ", data[i]));
                        if (((i % 8) == 7) || (i == 2 - 1))
                        {
                            AddLog(sb.ToString());
                            sb.Remove(0, sb.Length);
                        }
                    }
                }
            }

            comboBox2.SelectedIndex = data[0];
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
            figureX(singleProfilePointCount);
            AddLog(string.Format("X轴方向采样范围为:{0}\t轮廓点数为{1}", comboBox2.Text,singleProfilePointCount));
        }

        private void ReadAllSettings()
        {
            ReadBatchPoints();
            ReadTriggerInterval();
            ReadSampleFrequency();
            ReadTriggerType();
            ReadXRange();
        }

        private void OpenEthernetConnection()
        {
            Rc rc = Rc.Ok;
            // Initialize the DLL
            rc = (Rc)NativeMethods.LJV7IF_Initialize();
            if (!CheckReturnCode(rc)) return;

            // Open the communication path

            // Generate the settings for Ethernet communication.
            try
            {
                _ethernetConfig.abyIpAddress = new byte[] {
                        Convert.ToByte(_txtIpFirstSegment.Text),
                        Convert.ToByte(_txtIpSecondSegment.Text),
                        Convert.ToByte(_txtIpThirdSegment.Text),
                        Convert.ToByte(_txtIpFourthSegment.Text)
                    };
                _ethernetConfig.wPortNo = Convert.ToUInt16(_txtCommandPort.Text);

            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
                return;
            }

            rc = (Rc)NativeMethods.LJV7IF_EthernetOpen(Define.DEVICE_ID, ref _ethernetConfig);

            if (!CheckReturnCode(rc)) return;
            AddLog("以太网通信开启！");
        }

        private void figureX(int count)
        {
            xscale = new List<double>();
            for (int i = 0; i < count; i++)
            {
                double x = (i - count / 2) * 0.02;
                xscale.Add(x);
            }
        }

        private void profileClear()
        {
            currentProfileCount.Text = "0";
            numericUpDown3.Value = 0;
            _profileCount.Text = "0";
            chart1.Series["Profile"].Points.Clear();
            chart2.Series["Profile"].Points.Clear();
        }

        private void BuildBatchRecord(BatchRecord r)
        {
            string connectionStr = "mongodb://127.0.0.1:27017";
            MongoClient client = new MongoClient(connectionStr);
            var database = client.GetDatabase("Record");
            var collection = database.GetCollection<BsonDocument>(r.date.Replace("-",""));
            var document = new BsonDocument
            {
                { "Data",r.date},
                { "Time",r.time},
                { "ProfileCount",r.profileCount},
                { "PointCount",r.pointCount},
                { "Interval",r.interval}
            };
            for(int i =0; i<r.profileCount;i++)
            {
                Dictionary<string, int[]> p = new Dictionary<string, int[]>();
                p.Add("P" + i.ToString().PadLeft(4, '0'), r.datas[i]);
                document.AddRange(p);
            }
            collection.InsertOne(document);
            MessageBox.Show("Save Done!");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            BatchRecord res = new BatchRecord();
            res.date = DateTime.Today.ToString("yyyy-MM-dd");
            res.time = DateTime.Now.ToLongTimeString();
            res.profileCount = currentDatas.Count;
            res.pointCount = currentDatas[0].Count();
            res.interval = (double)decimal.Round(numericUpDown1.Value,3);
            res.datas = currentDatas;
            BuildBatchRecord(res);
        }
    }
}
