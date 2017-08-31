using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace QCDS_TestDemo
{
    public partial class FormEthSetting : Form
    {
        IniFileRW iniR;

        public FormEthSetting()
        {
            InitializeComponent();
            iniR = new IniFileRW(Application.StartupPath+"\\settings.ini");
            readSetting();
        }

        private void readSetting()
        {
            string ip = iniR.Read("SensorControllerEthernet","IP");
            string[] ips = ip.Split('.');
            _txtIpFirstSegment.Text = ips[0];
            _txtIpSecondSegment.Text = ips[1];
            _txtIpThirdSegment.Text = ips[2];
            _txtIpFourthSegment.Text = ips[3];

            _txtCommandPort.Text = iniR.Read("SensorControllerEthernet", "Port");
            _txtHighSpeedPort.Text = iniR.Read("SensorControllerEthernet", "HighSpeedPort");
        }

        private void btEthSetting_Click(object sender, EventArgs e)
        {
            string ip = _txtIpFirstSegment.Text + "." + _txtIpSecondSegment.Text + "." + _txtIpThirdSegment.Text + "." + _txtIpFourthSegment.Text;
            string port = _txtCommandPort.Text;
            string hport = _txtHighSpeedPort.Text;
            iniR.Write("SensorControllerEthernet", "IP", ip);
            iniR.Write("SensorControllerEthernet", "Port", port);
            iniR.Write("SensorControllerEthernet", "HighSpeedPort", hport);
        }
    }
}
