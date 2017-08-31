using System.Text;
using System.Runtime.InteropServices;
using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.OleDb;

namespace QCDS_TestDemo
{
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

    //参数判断条件
    public class JudgementPara
    {
        public string InputMaterial = "Default";
        public string OutputMaterial = "Default";
        public double InputThick = 1.0;
        public double OutputThick = 1.0;

        public double GapWidthMin;
        public double GapWidthMax;
        public double GapPosMin;
        public double GapPosMax;
        public double GapHDiffMin;
        public double GapHDiffMax;

        public double SeamWidthMin;
        public double SeamWidthMax;
        public double SeamPosMin;
        public double SeamPosMax;
        public double SeamHDiffMin;
        public double SeamHDiffMax;

        public double SeamUpMax;
        public double SeamDownMax;

        public double SGDiffMin;
        public double SGDiffMax;

        public JudgementPara()
        {
            loadDefaultPara();
        }

        public JudgementPara(string InMaterial,string OutMaterial,double InThick,double OutThick)
        {
           if(!loadPara(InMaterial,OutMaterial,InThick,OutThick))
           {
                loadDefaultPara();
           }
        }

        public void Show(System.Windows.Forms.Label[] lbls)
        {
            lbls[Define.GAP_WIDTH_MIN].Text = GapWidthMin.ToString();
            lbls[Define.GAP_WIDTH_MAX].Text = GapWidthMax.ToString();
            lbls[Define.GAP_POS_MIN].Text = GapPosMin.ToString();
            lbls[Define.GAP_POS_MAX].Text = GapPosMax.ToString();
            lbls[Define.GAP_HEIGHT_DIFFERENCE_MIN].Text = GapHDiffMin.ToString();
            lbls[Define.GAP_HEIGHT_DIFFERENCE_MAX].Text = GapHDiffMax.ToString();

            lbls[Define.SEAM_WIDTH_MIN].Text = SeamWidthMin.ToString();
            lbls[Define.SEAM_WIDTH_MAX].Text = SeamWidthMax.ToString();
            lbls[Define.SEAM_POS_MIN].Text = SeamPosMin.ToString();
            lbls[Define.SEAM_POS_MAX].Text = SeamPosMax.ToString();
            lbls[Define.SEAM_HEIGHT_DIFFERENCE_MIN].Text = SeamHDiffMin.ToString();
            lbls[Define.SEAM_HEIGHT_DIFFERENCE_MAX].Text = SeamHDiffMax.ToString();

            lbls[Define.SEAM_UP_MAX].Text = SeamUpMax.ToString();
            lbls[Define.SEAM_DOWN_MAX].Text = SeamDownMax.ToString();

            lbls[Define.SEAM_GAP_DIFFERENCE_MIN].Text = SGDiffMin.ToString();
            lbls[Define.SEAM_GAP_DIFFERENCE_MAX].Text = SGDiffMax.ToString();
        }

        private bool loadDefaultPara()
        {
            SqlHandle sqlhand = new SqlHandle();
            string sqls = "select * from QParaJudgeList where ID=1";
            DataTable dt = sqlhand.ExecuteRead(sqls);
            if(dt.Rows.Count<=0)
            {
                return false;
            }
            DataRow dr = dt.Rows[0];
            InputMaterial = dr[1].ToString();
            OutputMaterial = dr[2].ToString();
            InputThick = (double)dr[3];
            OutputThick = (double)dr[4];
            GapWidthMin = (double)dr[5];
            GapWidthMax = (double)dr[6];
            GapPosMin = (double)dr[7];
            GapPosMax = (double)dr[8];
            GapHDiffMin = (double)dr[9];
            GapHDiffMax = (double)dr[10];

            SeamWidthMin = (double)dr[11];
            SeamWidthMax = (double)dr[12];
            SeamPosMin = (double)dr[13];
            SeamPosMax = (double)dr[14];
            SeamHDiffMin = (double)dr[15];
            SeamHDiffMax = (double)dr[16];

            SeamUpMax = (double)dr[17];
            SeamDownMax = (double)dr[18];

            SGDiffMin = (double)dr[19];
            SGDiffMax = (double)dr[20];
            return true;
        }

        private bool loadPara(string InMaterial, string OutMaterial, double InThick, double OutThick)
        {
            SqlHandle sqlhand = new SqlHandle();
            string sqls = "select * from QParaJudgeList where InputMaterial = '" + InMaterial + "' and OutputMaterial ='" + OutMaterial + "' and InputThick =" + InThick.ToString() + "and OutputThick =" + OutThick.ToString();
            DataTable dt = sqlhand.ExecuteRead(sqls);
            if (dt.Rows.Count <= 0)
            {
                return false;
            }
            DataRow dr = dt.Rows[0];
            InputMaterial = dr[1].ToString();
            OutputMaterial = dr[2].ToString();
            InputThick = (double)dr[3];
            OutputThick = (double)dr[4];
            GapWidthMin = (double)dr[5];
            GapWidthMax = (double)dr[6];
            GapPosMin = (double)dr[7];
            GapPosMax = (double)dr[8];
            GapHDiffMin = (double)dr[9];
            GapHDiffMax = (double)dr[10];

            SeamWidthMin = (double)dr[11];
            SeamWidthMax = (double)dr[12];
            SeamPosMin = (double)dr[13];
            SeamPosMax = (double)dr[14];
            SeamHDiffMin = (double)dr[15];
            SeamHDiffMax = (double)dr[16];

            SeamUpMax = (double)dr[17];
            SeamDownMax = (double)dr[18];

            SGDiffMin = (double)dr[19];
            SGDiffMax = (double)dr[20];
            return true;
        }

    }

    public class IniFileRW
    {
        [DllImport("kernel32")]
        private static extern ulong WritePrivateProfileString(string section, string key, string val, string filepath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filepath);

        private string iniFileName;

        public string ErrorText;

        public IniFileRW(string iniFileDirection)
        {
            iniFileName = iniFileDirection;
            ErrorText = "None";
        }       //定义ini读写类，需要指明文件路径

        public string INIFileName
        {
            get
            {
                return iniFileName;
            }
            set
            {
                iniFileName = value;
            }
        }

        public string Read(string sect, string key)
        {
            StringBuilder temp = new StringBuilder();
            GetPrivateProfileString(sect, key, "NULL", temp, 255, iniFileName);
            return temp.ToString();
        }  //ini文件读取，SECT为分段，KEY为键值

        public bool Write(string sect, string key, string txt)
        {
            try
            {
                WritePrivateProfileString(sect, key, txt, iniFileName);
                return true;
            }
            catch (System.Exception ex)
            {
                ErrorText = ex.ToString();
                return false;
            }
        }       //ini文件写入，sect为分段，key为键位，txt为需要写入文本

    }

    public class SqlHandle
    {
        public int ExecuteNoneReturn(string s)
        {
            CDBInfo dbinfo = new CDBInfo();
            SqlConnection cnn = new SqlConnection();
            cnn.ConnectionString = dbinfo.ConnectString();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cnn;
            cmd.CommandText = s;

            try
            {
                cnn.Open();
                int i = cmd.ExecuteNonQuery();
                return i;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString() + "\r" + s);
                return -1;
            }
        }

        public DataTable ExecuteRead(string s)
        {
            CDBInfo dbinfo = new CDBInfo();
            SqlConnection cnn = new SqlConnection();
            cnn.ConnectionString = dbinfo.ConnectString();
            //SqlCommand cmd = new SqlCommand();
            //cmd.Connection = cnn;
            //cmd.CommandText = s;
            DataTable dt = new DataTable();

            DataSet ds = new DataSet();
            try
            {
                SqlDataAdapter sda = new SqlDataAdapter(s, cnn);
                sda.Fill(ds);
                dt = ds.Tables[0];
            }
            catch
            {
                ds = null;
            }
            return dt;
        }

        public bool MultiExecuteNoneReturn(string[] sqls)
        {
            CDBInfo dbinfo = new CDBInfo();
            SqlConnection cnn = new SqlConnection();
            cnn.ConnectionString = dbinfo.ConnectString();
            cnn.Open();
            SqlTransaction st = cnn.BeginTransaction();
            SqlCommand cmd = new SqlCommand();
            cmd.Connection = cnn;
            cmd.Transaction = st;
            try
            {
                foreach (string item in sqls)
                {
                    cmd.CommandText = item;
                    cmd.ExecuteNonQuery();
                }
                st.Commit();
            }
            catch
            {
                st.Rollback();
                return false;
            }
            return true;
        }
    }

    public class CDBInfo
    {
        public string db_ip;
        public string db_name;
        public string db_username;
        public string db_pwd;
        public string db_portNo;
        public string iniFileName;

        [DllImport("kernel32")]
        private static extern ulong WritePrivateProfileString(string section, string key, string val, string filepath);
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filepath);

        public CDBInfo()
        {
            StringBuilder temp = new StringBuilder(255);
            string Sect = "DataBaseConnection";
            string iniFilePath = Application.StartupPath;
            iniFileName = iniFilePath + "\\settings.ini";
            GetPrivateProfileString(Sect, "db_ip", "无法读取相应值！", temp, 255, iniFileName);
            db_ip = temp.ToString();
            GetPrivateProfileString(Sect, "db_name", "无法读取相应值！", temp, 255, iniFileName);
            db_name = temp.ToString();
            GetPrivateProfileString(Sect, "db_username", "无法读取相应值！", temp, 255, iniFileName);
            db_username = temp.ToString();
            GetPrivateProfileString(Sect, "db_pwd", "无法读取相应值！", temp, 255, iniFileName);
            db_pwd = temp.ToString();
            GetPrivateProfileString(Sect, "db_portNo", "无法读取相应值！", temp, 255, iniFileName);
            db_portNo = temp.ToString();
        }

        public string ConnectString()
        {
            return "server=" + db_ip + "," + db_portNo + ";database=" + db_name + ";user=" + db_username + ";pwd=" + db_pwd;
        }

        public void writeInfo()
        {
            WritePrivateProfileString("DataBaseConnection", "db_ip", db_ip, iniFileName);
            WritePrivateProfileString("DataBaseConnection", "db_name", db_name, iniFileName);
            WritePrivateProfileString("DataBaseConnection", "db_username", db_username, iniFileName);
            WritePrivateProfileString("DataBaseConnection", "db_pwd", db_pwd, iniFileName);
            WritePrivateProfileString("DataBaseConnection", "db_portNo", db_portNo, iniFileName);
        }
    }

}
