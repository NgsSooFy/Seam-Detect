using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics;
using MongoDB.Bson;
using MongoDB.Driver;

namespace QCDS_TestDemo
{
    public struct SeamInformation
    {
        public string Date;
        public string Time;
        public string InputSteelCode;
        public string OutputSteelCode;
        public string InputMaterial;
        public string OutputMaterial;
        public double InputWidth;
        public double OutputWidth;
        public double InputThickness;
        public double OutputThickness;

        public SeamInformation(string SDate,string STime, string InputCode,string OutputCode,string IMaterial,string OMaterial,double IWidth,double OWidth,double IThickness,double OThickness)
        {
            Date = SDate;
            Time = STime;
            InputSteelCode = InputCode;
            OutputSteelCode = OutputCode;
            InputMaterial = IMaterial;
            OutputMaterial = OMaterial;
            InputWidth = IWidth;
            OutputWidth = OWidth;
            InputThickness = IThickness;
            OutputThickness = OThickness;
        }
    }

    public struct CalculationParameter
    {
        public double Center;
        public double R;
        public int Factor;
   
        public CalculationParameter(double CenterPosition, double Radius, int factors)
        {
            Center = Math.Round(CenterPosition,2);
            R = Math.Round(Radius,2);
            Factor = factors;
        }
    }

    public struct MaxPoints                         //结构体用来表征2个特征点
    {
        public List<double> x;                      //返回特征点序列的x坐标值，使用中x序列个数为2
        public List<double> y;                      //返回特征点序列的y坐标值，使用中y序列个数为2
    }

    //单个轮廓类
    public class ProfileData
    {
        public List<int> OriginProfileData;                                     //从控制器中读到的单个摄像头的数据
 
        private int pointCount;                                                 //轮廓点数
        private List<double> ProfileData_float;                                 //轮廓数值转为double
        private List<double> AxisX;                                             //轮廓对应的X坐标轴

        //构造函数
        public ProfileData(List<int> profile)
        {
            OriginProfileData = OriginDataFix(profile);
            OriginProfileData.Reverse();

            pointCount = OriginProfileData.Count;
            ProfileData_float = Int2Double(OriginProfileData);
            AxisX = Figure_Xscale();
        }

        public ProfileData(int[] profile)
        {
            List<int> temp = new List<int>();
            temp.AddRange(profile);
            OriginProfileData = OriginDataFix(temp);
            OriginProfileData.Reverse();

            pointCount = OriginProfileData.Count;
            ProfileData_float = Int2Double(OriginProfileData);
            AxisX = Figure_Xscale();
        }

        //返回生成的X轴坐标
        public List<double> Figure_Xscale()
        {
            List<double> xscale = new List<double>();
            for (int i = 0; i < pointCount; i++)
            {
                double x = (i - pointCount / 2) * 0.02;
                xscale.Add(x);
            }
            return xscale;
        }

        //返回Y坐标的浮点值
        public List<double> Float_Yvalue()
        {
            return ProfileData_float;
        }

        //返回序列的特征点，必须有定位中心，计算半径值，拟合点数
        public MaxPoints CharacterPoint(double MiddlePosition, double R,int factor)
        {
            MaxPoints res = new MaxPoints();

            double x_Left = Math.Round(MiddlePosition - R, 2);
            double x_Right = Math.Round(MiddlePosition + R, 2);
            int IndexLeft = Location(AxisX, x_Left);
            int IndexRight = Location(AxisX, x_Right);
            List<double> K1 = QCDSDataFitWithDirection(factor, Define.LEFT_TO_RIGHT);
            List<double> K2 = QCDSDataFitWithDirection(factor, Define.RIGHT_TO_LEFT);
            List<double> X_Area = new List<double>();
            List<double> Y_Area = new List<double>();
            List<double> K1_Area = new List<double>();
            List<double> K2_Area = new List<double>();
            for(int i = IndexLeft;i<=IndexRight;i++)
            {
                X_Area.Add(AxisX[i]);
                Y_Area.Add(ProfileData_float[i]);
                K1_Area.Add(K1[i]);
                K2_Area.Add(K2[i]);
            }
            res = findMax(X_Area, Y_Area, K1_Area, K2_Area);
            return res;
        }

        public MaxPoints CharacterPoint(CalculationParameter CaculateParamater)
        {
            double MiddlePosition = CaculateParamater.Center;
            double R = CaculateParamater.R;
            int factor = CaculateParamater.Factor;
            MaxPoints res = new MaxPoints();

            double x_Left = Math.Round(MiddlePosition - R, 2);
            double x_Right = Math.Round(MiddlePosition + R, 2);
            int IndexLeft = Location(AxisX, x_Left);
            int IndexRight = Location(AxisX, x_Right);
            List<double> K1 = QCDSDataFitWithDirection(factor, Define.LEFT_TO_RIGHT);
            List<double> K2 = QCDSDataFitWithDirection(factor, Define.RIGHT_TO_LEFT);
            List<double> X_Area = new List<double>();
            List<double> Y_Area = new List<double>();
            List<double> K1_Area = new List<double>();
            List<double> K2_Area = new List<double>();
            for (int i = IndexLeft; i <= IndexRight; i++)
            {
                X_Area.Add(AxisX[i]);
                Y_Area.Add(ProfileData_float[i]);
                K1_Area.Add(K1[i]);
                K2_Area.Add(K2[i]);
            }
            res = findMax(X_Area, Y_Area, K1_Area, K2_Area);
            return res;
        }

        //返回轮廓特征点间隙，如为间隙则返回间隙宽度，如为焊缝则返回焊缝宽度
        public double GetWidth(double MiddlePosition, double R, int factor)
        {
            MaxPoints CP = CharacterPoint(MiddlePosition, R, factor);
            return Math.Round(CP.x[1] - CP.x[0], 2);
        }

        public double GetWidth(CalculationParameter CalculateParameter)
        {
            double MiddlePosition = CalculateParameter.Center;
            double R = CalculateParameter.R;
            int factor = CalculateParameter.Factor;
            MaxPoints CP = CharacterPoint(MiddlePosition, R, factor);
            return Math.Round(CP.x[1] - CP.x[0], 2);
        }

        //返回轮廓特征点位置与中心位置的差，返回间隙位置，焊缝位置
        public double GetPos(double MiddlePosition, double R, int factor)
        {
            MaxPoints CP = CharacterPoint(MiddlePosition, R, factor);
            return Math.Round((CP.x[0] + CP.x[1]) - MiddlePosition, 2);
        }

        public double GetPos(CalculationParameter CalculateParameter)
        {
            double MiddlePosition = CalculateParameter.Center;
            double R = CalculateParameter.R;
            int factor = CalculateParameter.Factor;
            MaxPoints CP = CharacterPoint(MiddlePosition, R, factor);
            return Math.Round((CP.x[0] + CP.x[1]) - MiddlePosition, 2);
        }

        //返回轮廓的高度差,返回间隙高度差，焊缝高度差
        public double GetHeightDifference(double MiddlePosition, double R, int factor)
        {
            double gapHeightDiff = 0;
            MaxPoints CP = CharacterPoint(MiddlePosition, R, factor);
            double x_Left = Math.Round(MiddlePosition - R, 2);
            double x_Right = Math.Round(MiddlePosition + R, 2);
            int IndexLeft = Location(AxisX, x_Left);                    //left start
            int IndexRight = Location(AxisX, x_Right);                  //right end
            int IndexLeftPoint = Location(AxisX, CP.x[0]);              //left end
            int IndexRightPoint = Location(AxisX, CP.x[1]);             //right start

            gapHeightDiff = gapHeightDifference(AxisX, ProfileData_float, IndexLeft, IndexLeftPoint, IndexRightPoint, IndexRight);
            return gapHeightDiff;
        }

        public double GetHeightDifference(CalculationParameter CalculateParameter)
        {
            double MiddlePosition = CalculateParameter.Center;
            double R = CalculateParameter.R;
            int factor = CalculateParameter.Factor;
            double gapHeightDiff = 0;
            MaxPoints CP = CharacterPoint(MiddlePosition, R, factor);
            double x_Left = Math.Round(MiddlePosition - R, 2);
            double x_Right = Math.Round(MiddlePosition + R, 2);
            int IndexLeft = Location(AxisX, x_Left);                    //left start
            int IndexRight = Location(AxisX, x_Right);                  //right end
            int IndexLeftPoint = Location(AxisX, CP.x[0]);              //left end
            int IndexRightPoint = Location(AxisX, CP.x[1]);             //right start

            gapHeightDiff = gapHeightDifference(AxisX, ProfileData_float, IndexLeft, IndexLeftPoint, IndexRightPoint, IndexRight);
            return gapHeightDiff;
        }

        //返回焊缝轮廓的凸起
        public double GetSeamUp(double MiddlePosition, double R, int factor)
        {
            double SeamUp = 0;
            MaxPoints CP = CharacterPoint(MiddlePosition, R, factor);
            int IndexLeftPoint = Location(AxisX, CP.x[0]);              //left end
            int IndexRightPoint = Location(AxisX, CP.x[1]);             //right start
            List<double> Seam = new List<double>();
            for(int i = IndexLeftPoint;i<=IndexRightPoint;i++)
            {
                Seam.Add(ProfileData_float[i]);
            }
            SeamUp = Seam.Max() - CP.y.Max();
            return SeamUp;
        }

        public double GetSeamUp(CalculationParameter CalculateParameter)
        {
            double MiddlePosition = CalculateParameter.Center;
            double R = CalculateParameter.R;
            int factor = CalculateParameter.Factor;
            double SeamUp = 0;
            MaxPoints CP = CharacterPoint(MiddlePosition, R, factor);
            int IndexLeftPoint = Location(AxisX, CP.x[0]);              //left end
            int IndexRightPoint = Location(AxisX, CP.x[1]);             //right start
            List<double> Seam = new List<double>();
            for (int i = IndexLeftPoint; i <= IndexRightPoint; i++)
            {
                Seam.Add(ProfileData_float[i]);
            }
            SeamUp = Seam.Max() - CP.y.Max();
            return SeamUp;
        }

        //返回焊缝轮廓的凸起
        public double GetSeamDown(double MiddlePosition, double R, int factor)
        {
            double SeamDown = 0;
            MaxPoints CP = CharacterPoint(MiddlePosition, R, factor);
            int IndexLeftPoint = Location(AxisX, CP.x[0]);              //left end
            int IndexRightPoint = Location(AxisX, CP.x[1]);             //right start
            List<double> Seam = new List<double>();
            for (int i = IndexLeftPoint; i <= IndexRightPoint; i++)
            {
                Seam.Add(ProfileData_float[i]);
            }
            SeamDown = Seam.Min() - CP.y.Min();
            return SeamDown;
        }

        public double GetSeamDown(CalculationParameter CalculateParameter)
        {
            double MiddlePosition = CalculateParameter.Center;
            double R = CalculateParameter.R;
            int factor = CalculateParameter.Factor;
            double SeamDown = 0;
            MaxPoints CP = CharacterPoint(MiddlePosition, R, factor);
            int IndexLeftPoint = Location(AxisX, CP.x[0]);              //left end
            int IndexRightPoint = Location(AxisX, CP.x[1]);             //right start
            List<double> Seam = new List<double>();
            for (int i = IndexLeftPoint; i <= IndexRightPoint; i++)
            {
                Seam.Add(ProfileData_float[i]);
            }
            SeamDown = Seam.Min() - CP.y.Min();
            return SeamDown;
        }


        #region Internal Function
        //返回拟合后求出的K值，direction标识方向
        private List<double> QCDSDataFitWithDirection(int factor, int direction)
        {
            //k为拟合的斜率曲线Y轴数组,factor 为拟合的点数,listX为轮廓横坐标数组，listY为轮廓纵坐标数组
            List<double> listX = this.Figure_Xscale();
            List<double> listY = ProfileData_float;

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
            if (direction == 0)
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


            for (; begin < end;)
            {

                for (int j = 0; j < factor; j++)
                {
                    if (direction == 0)
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
                if (direction == 0)
                    begin++;
                else
                {
                    pos++;
                    end--;
                }
                s = Fit.Line(x, y);
                if (direction == 0)
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
            if (direction == 0)
                last = k.Last<double>();//填充边界空出的点数
            else
                last = k.First<double>();
            for (int j = 0; j < factor; j++)
            {
                if (direction == 0)
                    k.Add(last);
                else
                    k.Insert(0, last);
            }
            return k;
        }
        
        private List<double> Int2Double(List<int> profile)
        {
            List<double> d = new List<double>();
            foreach (int q in profile)
            {
                d.Add((double)q / 100000);
            }
            return d;
        }

        //返回一个值在一个序列中的索引值，只能用于X轴的定位
        private int Location(List<double> listX, double valueP)
        {
            int index = -1;
            for (int i = 0; i < listX.Count; i++)
            {
                if (listX[i] == Math.Round(valueP, 2))
                {
                    index = i;
                    return index;
                }
            }
            return index;
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
        private double gapHeightDifference(List<double> listX, List<double> listY, int gapLeftStart, int gapLeftEnd, int gapRightStart, int gapRightEnd)
        {
            if (gapLeftStart > gapLeftEnd || gapRightStart > gapRightEnd || gapLeftEnd > gapRightStart)
                return 0;
            if (listY.Count != listX.Count)
                return 0;
            if (gapRightEnd - gapLeftStart + 1 > listX.Count)
                return 0;

            int i = 0;
            double sum = 0;
            for (i = gapLeftStart; i <= gapLeftEnd; i++)
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
       
        /* 
        * MaxPoints.x contains the horizontal value of most left and most right,i.e [x1,x2],x1 is tht most left value
        * MaxPoints.y contains the vertical value of most left and most right,i.e [y1,y2],y1 is tht most left value
        * listK1, right to left
        * listK2, left to right
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

        //对原始数据进行处理
        private List<int> OriginDataFix(List<int> OriginDatas)
        {
            List<int> res = new List<int>();
            int current = 0;
            for (int i = 0; i < OriginDatas.Count(); i++)
            {
                if ((OriginDatas[i] != -2147483648) & (OriginDatas[i] != -2147483647) & (OriginDatas[i] != -2147483646) & (OriginDatas[i] != -2147483645))
                {
                    res.Add(OriginDatas[i]);
                    current = OriginDatas[i];
                }
                else
                {
                    res.Add(current); 
                }
            }
            return res;
        }
        #endregion
    }

    //一条焊缝的数据，即控制器的一个批次
    public class BatchData
    {
        public SeamInformation BatchInformation; 
        public List<ProfileData> Gap;
        public List<ProfileData> Seam;

        private CalculationParameter GapCalculationParameters;
        private CalculationParameter SeamCalculationParameters;

        public BatchData()
        {
            BatchInformation = new SeamInformation();
            BatchInformation.Date = DateTime.Today.ToString("yyyy-MM-dd");
            BatchInformation.Time = DateTime.Now.ToLongTimeString();
            BatchInformation.InputSteelCode = "None";
            BatchInformation.OutputSteelCode = "None";
            BatchInformation.InputMaterial = "Default";
            BatchInformation.OutputMaterial = "Default";
            BatchInformation.InputThickness = 0;
            BatchInformation.OutputThickness = 0;
            BatchInformation.InputWidth = 0;
            BatchInformation.OutputWidth = 0;

            Gap = new List<ProfileData>();
            Seam = new List<ProfileData>();
            GapCalculationParameters = new CalculationParameter(0, 2, 10);
            SeamCalculationParameters = new CalculationParameter(0, 2, 10);
        }

        public BatchData(SeamInformation SeamInformation)
        {
            BatchInformation = SeamInformation;
            Gap = new List<ProfileData>();
            Seam = new List<ProfileData>();
            GapCalculationParameters = new CalculationParameter(0, 2, 10);
            SeamCalculationParameters = new CalculationParameter(0, 2, 10);
        }

        public void AddGap(ProfileData gap)
        {
            Gap.Add(gap);
        }

        public void AddSeam(ProfileData seam)
        {
            Seam.Add(seam);
        }

        public int GetGapCount()
        {
            return Gap.Count();
        }

        public int GetSeamCount()
        {
            return Seam.Count();
        }

        public int GetCount()
        {
            return Math.Max(Gap.Count(), Seam.Count());
        }

        public List<double> GetGapWidth()
        {
            List<double> res = new List<double>();
            for(int i = 0; i<Gap.Count(); i++)
            {
                res.Add(Gap[i].GetWidth(GapCalculationParameters.Center,GapCalculationParameters.R,GapCalculationParameters.Factor));
            }
            return res;
        }

        public List<double> GetGapPos()
        {
            List<double> res = new List<double>();
            for (int i = 0; i < Gap.Count(); i++)
            {
                res.Add(Gap[i].GetPos(GapCalculationParameters.Center, GapCalculationParameters.R, GapCalculationParameters.Factor));
            }
            return res;
        }

        public List<double> GetGapHeightDifference()
        {
            List<double> res = new List<double>();
            for (int i = 0; i < Gap.Count(); i++)
            {
                res.Add(Gap[i].GetHeightDifference(GapCalculationParameters.Center, GapCalculationParameters.R, GapCalculationParameters.Factor));
            }
            return res;
        }

        public List<double> GetSeamWidth()
        {
            List<double> res = new List<double>();
            for (int i = 0; i < Seam.Count(); i++)
            {
                res.Add(Seam[i].GetWidth(SeamCalculationParameters.Center, SeamCalculationParameters.R, SeamCalculationParameters.Factor));
            }
            return res;
        }

        public List<double> GetSeamPos()
        {
            List<double> res = new List<double>();
            for (int i = 0; i < Seam.Count(); i++)
            {
                res.Add(Seam[i].GetPos(SeamCalculationParameters.Center, SeamCalculationParameters.R, SeamCalculationParameters.Factor));
            }
            return res;
        }

        public List<double> GetSeamHeightDifference()
        {
            List<double> res = new List<double>();
            for (int i = 0; i < Seam.Count(); i++)
            {
                res.Add(Seam[i].GetHeightDifference(SeamCalculationParameters.Center, SeamCalculationParameters.R, SeamCalculationParameters.Factor));
            }
            return res;
        }

        public List<double> GetSeamUp()
        {
            List<double> res = new List<double>();
            for (int i = 0; i < Seam.Count(); i++)
            {
                res.Add(Seam[i].GetSeamUp(SeamCalculationParameters.Center, SeamCalculationParameters.R, SeamCalculationParameters.Factor));
            }
            return res;
        }

        public List<double> GetSeamDown()
        {
            List<double> res = new List<double>();
            for (int i = 0; i < Seam.Count(); i++)
            {
                res.Add(Seam[i].GetSeamDown(SeamCalculationParameters.Center, SeamCalculationParameters.R, SeamCalculationParameters.Factor));
            }
            return res;
        }

        public List<double> GetSeamGaoPosDifference()
        {
            List<double> res = new List<double>();
            if(Gap.Count() == Seam.Count())
            {
                List<double> GapPos = GetGapPos();
                List<double> SeamPos = GetSeamPos();
                for(int i = 0; i< GetCount();i++)
                {
                    res.Add(SeamPos[i] - GapPos[i]);
                }
            }
            return res;
        }

        public void Clear()
        {
            Gap.Clear();
            Seam.Clear();
        }

    }
}
