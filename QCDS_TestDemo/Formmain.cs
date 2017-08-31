using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using QCDS_TestDemo;
using QCDS_TestDemo.Properties;
using QCDS_TestDemo.Datas;
using System.Windows.Forms.DataVisualization.Charting;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using MathNet.Numerics;

namespace QCDS_TestDemo
{
    public partial class Formmain : Form
    {
        private LJV7IF_ETHERNET_CONFIG _ethernetConfig;
        private SendCommand _sendCommand;                           //Send command        
        private HighSpeedDataCallBack _callback;                    //Callback function used during high-speed communication
        private DeviceData[] _deviceData;                           //Array of controller information
        private IniFileRW iniH;
        private LJV7IF_PROFILE_INFO[] _profileInfo;                 //Array of profile information structures
        private bool singleBatchFinish = true;                      //用于定时器判断单个批处理是否完成
        private int currentBatchProfileCount = 0;
        private int singleProfilePointCount = 400;
        private int SensorCount = 1;
        private BatchData currentBatch;
        private CalculationParameter CPGap;
        private CalculationParameter CPSeam;

        public Formmain()
        {
            InitializeComponent();
            pMainWin = this;
            iniH = new IniFileRW(Application.StartupPath + "\\settings.ini");
        }

        private void Formmain_Load(object sender, EventArgs e)
        {
            FormDesignSetting();
            loadDefaulePara();
            ParameterRefresh();
            SensorControllerStart();
        }

        //调试界面弹窗
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            Form1 fm = new Form1();
            fm.Show();
        }

        //控制器以太网设定弹窗
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            FormEthSetting fmEth = new FormEthSetting();
            if (fmEth.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    OpenEthernetConnection();
                }
                catch
                {
                    toolStripStatusLabel2.Text = "未连接";
                    AddLog("以太网通信连接失败！");
                }
            }
        }

        //控制器以太网设定弹窗
        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            FormControllerSetting fmCS = new FormControllerSetting();
            if(fmCS.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }
            //ParameterRefresh();
            SensorControllerStop();
            SensorControllerStart();
        }

        //定时器读取线程可靠BUFFER中的数据
        private void timer1_Tick(object sender, EventArgs e)
        {
            int count = 0;
            uint notify = 0;
            int batchNo = 0;

            List<int[]> data = ThreadSafeBuffer.Get(0, out notify, out batchNo);
            if (data.Count != 0)
            {
                if (singleBatchFinish)
                {
                    //currentDatas.Clear();
                    currentBatch.Clear();
                }
                singleBatchFinish = false;
                foreach (int[] profile in data)
                {
                    int[] tempGap = new int[singleProfilePointCount];
                    int[] tempSeam = new int[singleProfilePointCount];
                    if (SensorCount == 1)
                    {
                        Array.Copy(profile, 6, tempGap, 0, singleProfilePointCount);
                        ProfileData Gap = new ProfileData(tempGap);
                        currentBatch.AddGap(Gap);
                        DrawCurrentGap(Gap);
                        DrawCurrentParameter(Gap);
                    }
                    else if (SensorCount == 2)
                    {
                        Array.Copy(profile, 6, tempGap, 0, singleProfilePointCount);
                        Array.Copy(profile, 6+singleProfilePointCount, tempSeam, 0, singleProfilePointCount);
                        ProfileData Gap = new ProfileData(tempGap);
                        ProfileData Seam = new ProfileData(tempSeam);
                        currentBatch.AddGap(Gap);
                        currentBatch.AddSeam(Seam);
                    }
                    count++;
                    currentBatchProfileCount++;
                }
            }
            if (notify != 0)
            {
                AddLog(string.Format("当前批计数达到上限,或中断当前批处理"));
                AddLog(string.Format("  notify[{0}] = {1,0:x8}\tbatch[{0}] = {2}", 0, notify, batchNo));
                AddLog(string.Format("当前批次轮廓数量 = {0}", currentBatchProfileCount));
                currentBatchProfileCount = 0;
                singleBatchFinish = true;
            }
        }

        #region checkbox
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            chartCurrentParameter.Series["GapWidth"].Enabled = checkBox1.Checked;
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            chartCurrentParameter.Series["GapPos"].Enabled = checkBox2.Checked;
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            chartCurrentParameter.Series["GapHDiff"].Enabled = checkBox3.Checked;
        }

        private void checkBox4_CheckedChanged(object sender, EventArgs e)
        {
            chartCurrentParameter.Series["SeamWidth"].Enabled = checkBox4.Checked;
        }

        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            chartCurrentParameter.Series["SeamPos"].Enabled = checkBox5.Checked;
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            chartCurrentParameter.Series["SeamHDiff"].Enabled = checkBox6.Checked;
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            chartCurrentParameter.Series["SeamUp"].Enabled = checkBox7.Checked;
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            chartCurrentParameter.Series["SeamDown"].Enabled = checkBox8.Checked;
        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            chartCurrentParameter.Series["SGDiff"].Enabled = checkBox9.Checked;
        }
        #endregion

        #region Log output

        /// <summary>
        /// Log output
        /// </summary>
        /// <param name="strLog">Output log</param>
        public void AddLog(string strLog)
        {
            _txtboxLog.AppendText(DateTime.Now.ToString() + "\t" + strLog + Environment.NewLine);
            _txtboxLog.SelectionStart = _txtboxLog.Text.Length;
            _txtboxLog.Focus();
            _txtboxLog.ScrollToCaret();
        }

        /// <summary>
        /// Communication command result log output
        /// </summary>
        /// <param name="rc">Return code from the DLL</param>
        /// <param name="commandName">Command name to be output in the log</param>
        public void AddLogResult(int rc, string commandName)
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
        public void AddErrorLog(int rc)
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
        public void AddError(uint dwError)
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
        public void CommonErrorLog(int rc)
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
        public void IndividualErrorLog(int rc)
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

        public bool CheckReturnCode(Rc rc)
        {
            if (rc == Rc.Ok) return true;
            MessageBox.Show(this, string.Format("Error: 0x{0,8:x}", rc), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        #endregion

        #region SensorControllerCommunication
        private bool OpenEthernetConnection()
        {
            Rc rc = Rc.Ok;
            // Initialize the DLL
            rc = (Rc)NativeMethods.LJV7IF_Initialize();
            if (!CheckReturnCode(rc))
            {
                return false;
            }

            // Open the communication path
            // Generate the settings for Ethernet communication.
            string ip = iniH.Read("SensorControllerEthernet", "IP");
            string[] ips = ip.Split('.');
            string port = iniH.Read("SensorControllerEthernet", "Port");
            try
            {
                _ethernetConfig.abyIpAddress = new byte[] {
                        Convert.ToByte(ips[0]),
                        Convert.ToByte(ips[1]),
                        Convert.ToByte(ips[2]),
                        Convert.ToByte(ips[3])
                    };
                _ethernetConfig.wPortNo = Convert.ToUInt16(port);

            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
                //toolStripStatusLabel2.Text = "未连接";
                AddLog("以太网通信连接失败！");
                return false;
            }

            rc = (Rc)NativeMethods.LJV7IF_EthernetOpen(Define.DEVICE_ID, ref _ethernetConfig);

            if (!CheckReturnCode(rc))
            {
                AddLog("以太网通信连接失败！");
                return false;
            }
            AddLog("以太网通信开启！");
            //toolStripStatusLabel2.Text = "已连接";
            return true;
        }

        private bool HighSpeedDataEthernetCommunicationInitalize()
        {
            _sendCommand = SendCommand.HighSpeedDataEthernetCommunicationInitalize;
            _deviceData[Define._currentDeviceId].ProfileData.Clear();  //Clear the retained profile data.
            _deviceData[Define._currentDeviceId].MeasureData.Clear();
            LJV7IF_ETHERNET_CONFIG ethernetConfig = _ethernetConfig;
            int rc = NativeMethods.LJV7IF_HighSpeedDataEthernetCommunicationInitalize(Define._currentDeviceId, ref ethernetConfig,
                UInt16.Parse(iniH.Read("SensorControllerEthernet", "HighSpeedPort")), _callback,
                1, (uint)Define._currentDeviceId);

            //AddLogResult(rc, Resources.SID_HIGH_SPEED_DATA_ETHERNET_COMMUNICATION_INITALIZE);

            if (rc == (int)Rc.Ok)
            {
                _deviceData[Define._currentDeviceId].Status = DeviceStatus.EthernetFast;
                _deviceData[Define._currentDeviceId].EthernetConfig = ethernetConfig;
                AddLog("高速连接初始化 OK！");
                return true;
            }
            else
            {
                AddLog("高速连接初始化 Fail！");
                return false;
            }
        }

        private bool PreStartHighSpeedDataCommunication()
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

            int rc = NativeMethods.LJV7IF_PreStartHighSpeedDataCommunication(Define._currentDeviceId, ref req, ref profileInfo);
            //AddLogResult(rc, Resources.SID_PRE_START_HIGH_SPEED_DATA_COMMUNICATION);
            if (rc == (int)Rc.Ok)
            {
                // Response data display
                //AddLog(Utility.ConvertToLogString(profileInfo).ToString());
                _profileInfo[Define._currentDeviceId] = profileInfo;
                AddLog("高速通信预开启 OK！");
                return true;
            }
            else
            {
                AddLog("高速通信预开启 Fail！");
                return false;
            }
        }

        private bool StartHighSpeedDataCommunication()
        {
            _sendCommand = SendCommand.StartHighSpeedDataCommunication;

            ThreadSafeBuffer.ClearBuffer(Define._currentDeviceId);
            int rc = NativeMethods.LJV7IF_StartHighSpeedDataCommunication(Define._currentDeviceId);
            // @Point
            //  If the LJ-V does not measure the profile for 30 seconds from the start of transmission,
            //  "0x00000008" is stored in dwNotify of the callback function.
            timer1.Start();
            //ReadingThread.Start();
            //AddLogResult(rc, Resources.SID_START_HIGH_SPEED_DATA_COMMUNICATION);
            if (rc == (int)Rc.Ok)
            {
                AddLog("高速通信启动 OK！");
                return true;
            }
            else
            {
                AddLog("高速通信启动 Fail！");
                return false;
            }
        }

        private bool StopHighSpeedDataCommunication()
        {
            int rc = NativeMethods.LJV7IF_StopHighSpeedDataCommunication(Define._currentDeviceId);
            timer1.Stop();
            //AddLogResult(rc, Resources.SID_STOP_HIGH_SPEED_DATA_COMMUNICATION);
            if (rc == (int)Rc.Ok)
            {
                AddLog("高速通信停止 OK！");
                return true;
            }
            else
            {
                AddLog("高速通信停止 Fail！");
                return false;
            }
        }

        private bool HighSpeedDataCommunicationFinalize()
        {
            int rc = NativeMethods.LJV7IF_HighSpeedDataCommunicationFinalize(Define._currentDeviceId);
            //AddLogResult(rc, Resources.SID_HIGH_SPEED_DATA_COMMUNICATION_FINALIZE);

            LJV7IF_ETHERNET_CONFIG config = _deviceData[Define._currentDeviceId].EthernetConfig;
            _deviceData[Define._currentDeviceId].Status = DeviceStatus.Ethernet;
            _deviceData[Define._currentDeviceId].EthernetConfig = config;

            timer1.Stop();
            if (rc == (int)Rc.Ok)
            {
                AddLog("高速连接断开 OK！");
                return true;
            }
            else
            {
                AddLog("高速连接断开 Fail！");
                return false;
            }
            //_deviceStatusLabels[_currentDeviceId].Text = _deviceData[_currentDeviceId].GetStatusString();
        }

        private bool SensorControllerStart()
        {
            if(!OpenEthernetConnection())
            {
                toolStripStatusLabel2.Text = "未连接";
                return false;
            }
            if (!HighSpeedDataEthernetCommunicationInitalize())
            {
                toolStripStatusLabel2.Text = "未连接";
                return false;
            }
            if (!PreStartHighSpeedDataCommunication())
            {
                toolStripStatusLabel2.Text = "未连接";
                return false;
            }
            if (!StartHighSpeedDataCommunication())
            {
                toolStripStatusLabel2.Text = "未连接";
                return false;
            }
            toolStripStatusLabel2.Text = "已连接";
            return true;
        }

        private bool SensorControllerStop()
        {
            if(!StopHighSpeedDataCommunication())
            {
                return false;
            }
            if (!HighSpeedDataCommunicationFinalize())
            {
                return false;
            }
            toolStripStatusLabel2.Text = "未连接";
            return true;
        }
        #endregion


        private void FormDesignSetting()
        {
            lblQC = new Label[16];
            lblQC[Define.GAP_WIDTH_MIN] = label43;
            lblQC[Define.GAP_WIDTH_MAX] = label45;
            lblQC[Define.GAP_POS_MIN] = label46;
            lblQC[Define.GAP_POS_MAX] = label48;
            lblQC[Define.GAP_HEIGHT_DIFFERENCE_MIN] = label49;
            lblQC[Define.GAP_HEIGHT_DIFFERENCE_MAX] = label51;

            lblQC[Define.SEAM_WIDTH_MIN] = label52;
            lblQC[Define.SEAM_WIDTH_MAX] = label54;
            lblQC[Define.SEAM_POS_MIN] = label55;
            lblQC[Define.SEAM_POS_MAX] = label57;
            lblQC[Define.SEAM_HEIGHT_DIFFERENCE_MIN] = label58;
            lblQC[Define.SEAM_HEIGHT_DIFFERENCE_MAX] = label60;

            lblQC[Define.SEAM_UP_MAX] = label63;
            lblQC[Define.SEAM_DOWN_MAX] = label66;
            lblQC[Define.SEAM_GAP_DIFFERENCE_MIN] = label67;
            lblQC[Define.SEAM_GAP_DIFFERENCE_MAX] = label69;
        }
        
        private void loadDefaulePara()
        {
            JudgementPara jp = new JudgementPara();
            jp.Show(lblQC);

        }

        private void ParameterRefresh()
        {
            _sendCommand = SendCommand.None;
            _deviceData = new DeviceData[NativeMethods.DeviceCount];
            _callback = new HighSpeedDataCallBack(ReceiveHighSpeedData);
            
            for (int i = 0; i < NativeMethods.DeviceCount; i++)
            {
                _deviceData[i] = new DeviceData();
            }
            _profileInfo = new LJV7IF_PROFILE_INFO[NativeMethods.DeviceCount];
            
            currentBatch = new BatchData();
            CPGap = new CalculationParameter(0, 2, 10);
            CPSeam = new CalculationParameter(0, 2, 10);
        }

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

        private void CurrentChartClear()
        {
            foreach(Series s in chartCurrentGap.Series)
            {
                s.Points.Clear();
            }
            foreach (Series s in chartCurrentSeam.Series)
            {
                s.Points.Clear();
            }
            foreach (Series s in chartCurrentParameter.Series)
            {
                s.Points.Clear();
            }
        }

        private void DrawCurrentGap(ProfileData Gap)
        {
            chartCurrentGap.Series["Profile"].Points.DataBindXY(Gap.Figure_Xscale(), Gap.Float_Yvalue());
            chartCurrentGap.Series["CPoints"].Points.DataBindXY(Gap.CharacterPoint(CPGap).x, Gap.CharacterPoint(CPGap).y);
        }

        private void DrawCurrentSeam(ProfileData Seam)
        {
            chartCurrentSeam.Series["Profile"].Points.DataBindXY(Seam.Figure_Xscale(), Seam.Float_Yvalue());
            chartCurrentSeam.Series["CPoints"].Points.DataBindXY(Seam.CharacterPoint(CPSeam).x, Seam.CharacterPoint(CPSeam).y);
        }

        private void DrawCurrentParameter(ProfileData Gap)
        {
            chartCurrentParameter.Series["GapWidth"].Points.AddY(Gap.GetWidth(CPGap));
            chartCurrentParameter.Series["GapPos"].Points.AddY(Gap.GetPos(CPGap));
            chartCurrentParameter.Series["GapHDiff"].Points.AddY(Gap.GetHeightDifference(CPGap));
        }



        private List<double> figureXR(int count, double interval)
        {
            List<double> xRscale = new List<double>();
            for (int i = 0; i < count; i++)
            {
                double x = (i - count / 2) * 0.02;
                xRscale.Add(Math.Round(x, 2));
            }

            return xRscale;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            CurrentChartClear();
        }
    }
}
