using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text;
using System.IO;
using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;
using MathNet.Numerics;
namespace SeamDetect
{
    partial class Form1
    {
     

        static List<double>[] GetDataFromFile(string datafilePath)
        {
            // 创建泛型列表
            List<double> listX = new List<double>();
            List<double> listY = new List<double>();
            List<double> list = new List<double>();

            // 打开数据文件 D:\data.txt逐行读入
            StreamReader rd1 = File.OpenText(datafilePath);
            string line;
            int lineNum = 0;
            while ((line = rd1.ReadLine()) != null)
            {
                if (lineNum % 2 == 0)
                    list = listX;
                else
                    list = listY;
                // 将读入的数据转换成double类型值
                string[] nums = line.Split(new char[2] { ' ', '\t' });
                //Console.WriteLine(nums.Length);
                int i;
                for (i = 0; i < nums.Length; i++)
                {
                    string input = nums[i];
                    double result;
                    if (double.TryParse(input, out result))
                    {
                        // 转换成功，加入到泛型列表
                        list.Add(result);
                    }
                    else
                    {
                        // 转换失败，显示错误提示
                        //Console.WriteLine(line);
                        //Console.WriteLine("数据格式错误！");
                        ;
                    }
                }
                lineNum++;
            }
            // 关闭文件
            rd1.Close();

            // 将泛型列表转换成数组
            //List<Point> points = new List<Point>(listX.Count);
            List<Point> points = new List<Point>();
            //Point[] points = Array.CreateInstance(Point, listX.Count);


            //int index = 0
            List<double>[] listXY = { listX,listY};
            if (listX.Count == listY.Count)
            {
                
                /*for (index = 0; index < listX.Count; index++)
                {
                  
                    
                    
                    int X = listX.IndexOf(index);
                    int Y = listY.IndexOf(index);
                    Point p = new Point(X, Y);
                    p.X = X;
                    p.Y = Y;
                    points.Add(p);
                }*/


            }

            
            return listXY;
        }



        /*
         * @param direction: LEFT_TO_RIGHT (0), RIGHT_TO_LEFT (not 0);
         */
        static List<double> QCDSDataFitWithDirection(List<double> listX, List<double> listY, int factor, int direction)
        {
            //k为拟合的斜率曲线Y轴数组,factor 为拟合的点数,listX为轮廓横坐标数组，listY为轮廓纵坐标数组
            double[] x = new double[factor];
            double[] y = new double[factor];
            Tuple<double, double> s = new Tuple<double, double>(0, 0);
            List<double> k = new List<double>();
            int count = listX.Count;
            if (count != listY.Count)//数组大小不一致，抛出异常？
            {
                return k;
            }
            int pos;
            int begin, end;
            if (direction == LEFT_TO_RIGHT)
            {
                pos = 0;
                begin = pos;
                end = count - factor;
            }
            else
            {

                pos = count - 1;
                end = pos;
                pos = 0;
                begin = factor - 1;
            }

            
            for (; begin < end; )
            {
                
                for (int j = 0; j < factor; j++)
                {
                    if (direction == LEFT_TO_RIGHT)
                    {
                        x[j] = listX.ElementAt(begin + j);
                        y[j] = listY.ElementAt(begin + j);
                        
                    }
                    else
                    {
                        x[j] = listX.ElementAt(pos + j);
                        y[j] = listY.ElementAt(end - j);
                        
                    }
                    
                }
                if (direction == LEFT_TO_RIGHT)
                    begin++;
                else
                {
                    pos++;
                    end--;
                }
                s = Fit.Line(x, y);
                if(direction == LEFT_TO_RIGHT)
                {
                    k.Add(s.Item2);
                }
                else
                {
                    k.Insert(0, s.Item2);
                    //k.Add(s.Item2);
                }
            }
            double last;
            if (direction == LEFT_TO_RIGHT)
                last = k.Last<double>();//填充边界空出的点数
            else
                last = k.First<double>(); 
            for (int j = 0; j < factor; j++)
            {
                if(direction == LEFT_TO_RIGHT)
                    k.Add(last);
                else
                    k.Insert(0, last);
            }
            return k;
        }

        /*左边补偿，拟合出的斜率给最右边的点*/
        static List<double> QCDSDataFit2(List<double> listX, List<double> listY, int factor)
        {
            //k为拟合的斜率曲线Y轴数组,factor 为拟合的点数,listX为轮廓横坐标数组，listY为轮廓纵坐标数组
            double[] x = new double[factor];
            double[] y = new double[factor];
            Tuple<double, double> s = new Tuple<double, double>(0, 0);
            List<double> k = new List<double>();
            int count = listX.Count;
            if (count != listY.Count)//数组大小不一致，抛出异常？
            {
                return k;
            }

            for (int pos = 0; pos < count - factor; pos++)//count - factor为迭代的右边界
            {
                for (int j = 0; j < factor; j++)
                {
                    x[j] = listX.ElementAt(pos + j);

                    y[j] = listY.ElementAt(pos + j);

                }
                s = Fit.Line(x, y);
                k.Add(s.Item2);
            }
            double first = k.First<double>();//填充边界空出的点数
            
            for (int j = 0; j < factor; j++)
                k.Insert(0,first);
            
            return k;
        }

		/* 
		 * MaxPoints.x contains the horizontal value of most left and most right,i.e [x1,x2],x1 is tht most left value
		 * MaxPoints.y contains the vertical value of most left and most right,i.e [y1,y2],y1 is tht most left value
		 * listK2, right to left
		 * listK1, left to right
		 */
		private MaxPoints findMax(List<double> listX, List<double> listY, List<double> listK1, List<double> listK2)
		{
			MaxPoints max = new MaxPoints();
			max.x = new List<double>();
			max.y = new List<double>();

			MaxPoints min = new MaxPoints();
			min.x = new List<double>();
			min.y = new List<double>();

			// var v1, v2;  max value
			//var v3, v4;  min value
			int cnt = 0;

			List<double> tmp;

			bool isSummit = true;
			var v1 = listK1.Select((m, index) => new { index, m }).OrderByDescending(n => n.m).Last();
			var v3 = listK1.Select((m, index) => new { index, m }).OrderByDescending(n => n.m).First();
			var v2 = listK1.Select((m, index) => new { index, m }).OrderByDescending(n => n.m).Last();
			var v4 = listK1.Select((m, index) => new { index, m }).OrderByDescending(n => n.m).First();
			if (v1.index < v3.index) //  summit,else valley 
			{
				isSummit = true;
			}
			else
			{
				isSummit = false;
			}
			int a, b;
			while (isSummit)
			{
				cnt++;
				v1 = listK1.Select((m, index) => new { index, m }).OrderByDescending(n => n.m).Last();
				v2 = listK2.Select((m, index) => new { index, m }).OrderByDescending(n => n.m).Last();

				if (isSummit)
				{
					a = v1.index;
					b = v2.index;
					tmp = listK2;
				}
				else
				{
					b = v1.index;
					a = v2.index;
					tmp = listK1;
				}
			    if (a > b && cnt < 2) //cnt < 2 to avoid infinate loop, it may happen
				{
					// set listK1[0-index] = 0
					for (int i = 0; i <= a; i++)
					{
						tmp[i] = 0;
						continue;
					}
				}
				break;
			}
			cnt = 0;
			while (!isSummit)
			{
				cnt++;
				v3 = listK1.Select((m, index) => new { index, m }).OrderByDescending(n => n.m).First();
				v4 = listK2.Select((m, index) => new { index, m }).OrderByDescending(n => n.m).First();

				if (v3.index > v4.index && cnt < 2) //cnt < 2 to avoid infinate loop, it may happen
				{
					// set listK1[0-index] = 0
					for (int i = 0; i <= v3.index; i++)
					{
						listK2[i] = 0;
						continue;
					}
				}
				break;
			}
			if (isSummit)
			{
				max.x.Add(listX[v1.index]);
				max.x.Add(listX[v2.index]);
				max.y.Add(listY[v1.index]);
				max.y.Add(listY[v2.index]);
               
				return max;
			}
			else
			{
				min.x.Add(listX[v3.index]);
				min.x.Add(listX[v4.index]);
				min.y.Add(listY[v3.index]);
				min.y.Add(listY[v4.index]);
                
                return min;
			}	
		}

        private GapIndex findGap(List<double> listX, List<double> listY, List<double> listK1, List<double> listK2)
        {
            GapIndex gap = new GapIndex();
            MaxPoints max = new MaxPoints();
            max.x = new List<double>();
            max.y = new List<double>();

            MaxPoints min = new MaxPoints();
            min.x = new List<double>();
            min.y = new List<double>();

            // var v1, v2;  max value
            //var v3, v4;  min value
            int cnt = 0;

            List<double> tmp;

            bool isSummit = true;
            var v1 = listK1.Select((m, index) => new { index, m }).OrderByDescending(n => n.m).Last();
            var v3 = listK1.Select((m, index) => new { index, m }).OrderByDescending(n => n.m).First();
            var v2 = listK1.Select((m, index) => new { index, m }).OrderByDescending(n => n.m).Last();
            var v4 = listK1.Select((m, index) => new { index, m }).OrderByDescending(n => n.m).First();
            if (v1.index < v3.index) //  summit,else valley 
            {
                isSummit = true;
            }
            else
            {
                isSummit = false;
            }
            int a, b;
            while (isSummit)
            {
                cnt++;
                v1 = listK1.Select((m, index) => new { index, m }).OrderByDescending(n => n.m).Last();
                v2 = listK2.Select((m, index) => new { index, m }).OrderByDescending(n => n.m).Last();

                if (isSummit)
                {
                    a = v1.index;
                    b = v2.index;
                    tmp = listK2;
                }
                else
                {
                    b = v1.index;
                    a = v2.index;
                    tmp = listK1;
                }
                if (a > b && cnt < 2) //cnt < 2 to avoid infinate loop, it may happen
                {
                    // set listK1[0-index] = 0
                    for (int i = 0; i <= a; i++)
                    {
                        tmp[i] = 0;
                        continue;
                    }
                }
                break;
            }
            cnt = 0;
            while (!isSummit)
            {
                cnt++;
                v3 = listK1.Select((m, index) => new { index, m }).OrderByDescending(n => n.m).First();
                v4 = listK2.Select((m, index) => new { index, m }).OrderByDescending(n => n.m).First();

                if (v3.index > v4.index && cnt < 2) //cnt < 2 to avoid infinate loop, it may happen
                {
                    // set listK1[0-index] = 0
                    for (int i = 0; i <= v3.index; i++)
                    {
                        listK2[i] = 0;
                        continue;
                    }
                }
                break;
            }
            if (isSummit)
            {
                
                gap.left = v1.index;
                gap.right = v2.index;
                return gap;
            }
            else
            {
                gap.left = v3.index;
                gap.right = v4.index;
                return gap;
            }
        }

        /* 
         * listX, x axis value list of the profile
         * listY, y axis value list of the profile
		 * gapLeftStart, x start index of the left-boundary of the gap
         * gapLeftEnd,x end index of the left-boundary of the gap
         * gapRightStart, x start index of the right-boundary of the gap
         * gapRightEnd,x end index of the right-boundary of the gap
         * 
         * return: the height difference, height.left - height.right
		 */
        private double gapHeightDifference(List<double> listX, List<double> listY, int gapLeftStart,int gapLeftEnd,int gapRightStart,int gapRightEnd)
        {
            if (gapLeftStart > gapRightEnd || gapRightStart > gapRightEnd || gapLeftEnd > gapRightStart)
                return 0;
            if (listY.Count != listX.Count)
                return 0;
            if (gapRightEnd - gapLeftStart + 1 > listX.Count)
                return 0;

            int i = 0;
            double sum = 0;
            for(i = gapLeftStart;i <= gapLeftEnd;i ++)
            {
                sum += listY.ElementAt(i);
            }
            double averLeft = sum / (gapLeftEnd - gapLeftStart + 1);

            sum = 0;
            for (i = gapRightStart; i <= gapRightEnd; i++)
            {
                sum += listY.ElementAt(i);
            }
            double averRight = sum / (gapRightEnd - gapRightStart + 1);

            return averLeft - averRight;
        }
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            

            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1000, 5000);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);

            string filePath = "profile-1.txt";

            // 从数据文件>读取数据
            List<double>[] values = GetDataFromFile(filePath);


            int w = this.Width;
            int h = this.Height;

            Chart chart1 = new Chart();
            ChartArea chartArea1 = new ChartArea();


            //定义一个绘图区域
            Series series1 = new Series();
            Series series2 = new Series();// for 斜率曲线
            Series series3 = new Series();// for 斜率曲线
            Series series4 = new Series();// for 斜率曲线
			Series series5 = new Series();

			//定义一个数据列
			chartArea1.Name = "ChartArea1";

            //其实没有必要，可以使用chart1。ChartAreas[0]就可以了
            chart1.ChartAreas.Add(chartArea1);

            //完成Chart和chartArea的关联
            //Legend legend1 = new Legend();
            //legend1.Name = "图标";
            //chart1.Legends.Add(legend1);
            chart1.Name = "chart1";
            series1.ChartType = SeriesChartType.Spline;
            series2.ChartType = SeriesChartType.Spline;
            series3.ChartType = SeriesChartType.Spline;
			series5.ChartType = SeriesChartType.Spline;
			series4.ChartType = SeriesChartType.Spline;

            //设置线性

            Random rd2 = new Random();
            double[] num = new double[20];

            /*for (int i = 0; i < 20; i++)
            {
                int valuey = rd2.Next(20, 100);
                DataPoint point = new DataPoint((i + 1), valuey);
                series1.Points.Add(point);
            }*/
            series1.Points.DataBindXY(values[0], values[1]);
            int fitFactor = 10; 
            // left to right, padding the most right points
            List<double> k = QCDSDataFitWithDirection(values[0], values[1], fitFactor,0);
            series2.Points.DataBindXY(values[0], k);
            
            List<double> k2 = QCDSDataFit2(values[0], values[1], fitFactor);
            //series3.Points.DataBindXY(values[0], k2);
            // right to left, padding the most last points
            List<double> k3 = QCDSDataFitWithDirection(values[0], values[1], fitFactor, 1);
            series4.Points.DataBindXY(values[0], k3);

            // find the boundary of the gap,(point1,point2)
            GapIndex gap = findGap(values[0], values[1], k, k3);
			MaxPoints result = findMax(values[0], values[1], k, k3);
			List<double> left_x = new List<double>();
			List<double> left_y = new List<double>();
			for (double ch = 0; (result.y[0] + ch <= 3); ch += 0.02)
			{
				left_x.Add(result.x[0]);
				left_y.Add(result.y[0] + ch);
			}
			series3.Points.DataBindXY(left_x, left_y);

            double heightDiff = gapHeightDifference(values[0], values[1], gap.left, gap.left, gap.right, gap.right);  
            chart1.Titles.Add("height difference:"+ heightDiff.ToString());

            List <double> right_x = new List<double>();
			List<double> right_y = new List<double>();
			for (double ch = 0; (result.y[0] + ch <= 3); ch += 0.02)
			{
				right_x.Add(result.x[1]);
				right_y.Add(result.y[1] + ch);
			}
			series5.Points.DataBindXY(right_x, right_y);

			//series1.Points.DataBindY(values);

			//产生点的坐标
			//chart1.Titles[0].Text = "";
			//chartArea1.AxisX.IntervalType = double;
			//chartArea1.AxisY.IntervalType = double;
			series1.ChartArea = "ChartArea1";
            chartArea1.AxisX.Title = "X";
            chartArea1.AxisY.Title = "Y";
            chartArea1.AxisX.Interval = 0.02;
            //chartArea1.AxisY.Interval = 0.0001;
            //chartArea1.AxisY.Minimum = 16;
            series1.Legend = "图标";
            series1.Name = "Series1";
            chart1.Text = "测试";
            chart1.Size = new System.Drawing.Size(w, h);
            //chart1.Location = new System.Drawing.Point(50,120);
            series1.Color = Color.Red;
            series2.Color = Color.Blue;
            series3.Color = Color.Green;
			series5.Color = Color.Pink;
			series4.Color = Color.Honeydew;
            chart1.Text = "ceshi";
            //chart1.Titles[0].Text = "fff";
            chart1.Series.Add(series1);
            chart1.Series.Add(series2);
            chart1.Series.Add(series3);
			chart1.Series.Add(series5);
			chart1.Series.Add(series4);
            //这一句很重要，缺少的话绘图区域将显示空白
            //chart1.SizeChanged += new System.EventHandler(DoSizeChanged);
            //chart1.AllowDrop = true;
            chart1.BackColor = Color.FromArgb(243, 223, 193);

            //设置chart背景颜色
            chartArea1.BackColor = Color.FromArgb(243, 223, 193);

            //设置c绘图区域背景颜色
            series1.BorderWidth = 2;
            series2.BorderWidth = 2;
            series3.BorderWidth = 2;
			series5.BorderWidth = 2;
			series4.BorderWidth = 2;
            //series1.IsValueShownAsLabel = true;
            //series2.IsValuseShownAsLabel = true;

            //是否显示Y的值

            //this.groupBox9.Controls.Add(chart1);.
            this.Controls.Add(chart1);
            //this.panel21.Controls.Add(chart1);
            chart1.Visible = true;
            //this.label10.Visible = true;
            //this.label10.Text = "【" + tn.Name + "】图";
            chart1.Titles.Add("profile");
            //为Chart1添加标题
            chartArea1.AxisX.IsMarginVisible = true;

            //是否显示X轴两端的空白

            //2、在上述chart的基础上添加一条线

            /*
            System.Windows.Forms.Control[] controls = this.Controls.Find("chart1", true);

            //找到已经有的Chart1
            Chart ch = (Chart)controls[0];
            Series series2 = new Series();
            Series series3 = new Series();
            Series series4 = new Series();

            //新定义一个数据项
            Random rd = new Random();
            for (int i = 0; i < ch.Series[0].Points.Count; i++)
            {
                int valuey = rd.Next(20, 100);
                DataPoint point = new DataPoint((i + 1), valuey);
                series2.Points.Add(point);
                series3.Points.Add(point);
                series4.Points.Add(point);
            }
            series2.Color = Color.FromArgb(rd.Next(100, 255), rd.Next(100, 150), rd.Next(0, 255));
            series2.BorderWidth = 2;
            series2.ChartType = ((Chart)this.Controls[0]).Series[0].ChartType;

            series3.Color = Color.FromArgb(rd.Next(100, 255), rd.Next(0, 150), rd.Next(0, 100));
            series3.BorderWidth = 2;
            series3.ChartType = ((Chart)this.Controls[0]).Series[0].ChartType;

            series4.Color = Color.FromArgb(rd.Next(100, 255), rd.Next(0, 150), rd.Next(0, 255));
            series4.BorderWidth = 2;
            series4.ChartType = ((Chart)this.Controls[0]).Series[0].ChartType;

            //定义线性和原有的线条形状一致
            series2.IsValueShownAsLabel = true;
            ch.Series.Add(series2);

            series3.IsValueShownAsLabel = true;
            ch.Series.Add(series3);

            series4.IsValueShownAsLabel = true;
            ch.Series.Add(series4);

            //添加数据列

            

            /*Graphics g = this.CreateGraphics();
            this.Show();
            Pen pen1 = new Pen(Color.Red, 3);
            Point[] p1 = new Point[]
            {
                new Point(10,10),
                new Point(60,40),
                new Point(100,80),
                new Point(60,100)
            };
            
            
            g.DrawCurve(pen1, values);
            g.Save();*/

        }

        #endregion
    }
}

