﻿using MySql.Data.MySqlClient;
using RDotNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RProject
{
    public partial class MainWindow : Window
    {
        private Hashtable ht = new Hashtable();
        private bool hasStatic = false;

        private void StartPageCbB1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int startIndex = Convert.ToInt32(StartPageCbB1.SelectedValue);
            EndPageCbB1.Items.Clear();
            for (int i = startIndex; i <= maxPage; i++) {
                EndPageCbB1.Items.Add(i);
            }
            StartSlider.Minimum = 1;
            EndSlider.Minimum = 1;
        }

        private void EndPageCbB1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int startPage = Convert.ToInt32(StartPageCbB1.SelectedValue);
            int endPage = Convert.ToInt32(EndPageCbB1.SelectedValue);

            StartSlider.Maximum = (endPage - startPage + 1) * rowPerPage;
            EndSlider.Maximum = (endPage - startPage + 1) * rowPerPage;
            EndSlider.Value = EndSlider.Maximum;
        }


        private void StatisticsBtn_Click(object sender, RoutedEventArgs e)
        {
            double avg = 0;
            double sd2 = 0;     //方差
            int max = 0;
            int min = 0;
            int median = 0;     //中位数
            int mode = 0;       //众数
            // 
            //             if (csvDt != null) {
            //                 int startIndex;
            //                 int endIndex;
            // 
            //                 if (StartPageCbB1.SelectedIndex >= 0 && EndPageCbB1.SelectedIndex >= 0) {
            //                     int startPage = Convert.ToInt32(StartPageCbB1.SelectedValue);
            //                     int endPage = Convert.ToInt32(EndPageCbB1.SelectedValue);
            // 
            //                     startIndex = (startPage - 1) * rowPerPage + 1;
            //                     endIndex = endPage * rowPerPage;
            //                     if (endIndex > length) {
            //                         endIndex = length;
            //                     }
            //                 } else {
            //                     startIndex = 1;
            //                     endIndex = length;
            //                 }
            // 
            // 
            //                 try {
            //                     avg = re.Evaluate(string.Format("mean(tb[{0}:{1},1])", startIndex, endIndex)).AsNumeric()[0];
            //                     avg = Math.Round(avg, 2);
            //                     AvgLabel.Content = avg.ToString();
            // 
            //                     sd2 = re.Evaluate(string.Format("var(tb[{0}:{1},1])", startIndex, endIndex)).AsNumeric()[0];
            //                     sd2 = Math.Round(sd2, 2);
            //                     Sd2Label.Content = sd2.ToString();
            // 
            //                     max = re.Evaluate(string.Format("max(tb[{0}:{1},1])", startIndex, endIndex)).AsInteger()[0];
            //                     MaxLabel.Content = max.ToString();
            // 
            //                     min = re.Evaluate(string.Format("min(tb[{0}:{1},1])", startIndex, endIndex)).AsInteger()[0];
            //                     MinLabel.Content = min.ToString();
            // 
            //                     median = re.Evaluate(string.Format("median(tb[{0}:{1},1])", startIndex, endIndex)).AsInteger()[0];
            //                     MedianLabel.Content = median.ToString();
            // 
            //                     mode = Convert.ToInt32(re.Evaluate(string.Format("names(which.max(table(tb[{0}:{1},1])))", startIndex, endIndex)).AsCharacter()[0]);
            //                     ModeLabel.Content = mode.ToString();
            //                 } catch {
            // 
            //                 }
            //             } else 
            if (myConn != null) {
                if (CowIdCbB.SelectedIndex == -1) {
                    MessageBox.Show("请选择奶牛ID");
                } else if (YuZhiCbB.SelectedIndex == -1) {
                    MessageBox.Show("请选择阈值");
                } else {
                    int id = Convert.ToInt32(CowIdCbB.SelectedValue);

                    if (ht.Contains(id) && ((SelectTime) ht[id]).isOK) {
                        SelectTime stWin = (SelectTime) ht[id];
                        List<int> l = new List<int>();
                        string startDate;
                        int startTime;
                        string endDate;
                        int endTime;

                        DateTime tempDate;

                        string sqlComm;
                        MySqlCommand comm;
                        MySqlDataReader dr;

                        l.Clear();
                        for (int i = 0; i < stWin.dt.Rows.Count; i++) {
                            startDate = stWin.dt.Rows[i][1].ToString();

                            string temp = stWin.dt.Rows[i][2].ToString();
                            temp = temp.Remove(temp.IndexOf(':'));
                            startTime = Convert.ToInt32(temp);

                            endDate = stWin.dt.Rows[i][3].ToString();

                            temp = stWin.dt.Rows[i][4].ToString();
                            temp = temp.Remove(temp.IndexOf(':'));
                            endTime = Convert.ToInt32(temp);

                            tempDate = Convert.ToDateTime(startDate);

                            for (; tempDate <= Convert.ToDateTime(endDate); tempDate = tempDate.AddDays(1)) {
                                int startIndex = 1;
                                int endIndex = 24;

                                if (tempDate == Convert.ToDateTime(startDate)) {
                                    startIndex = startTime + 1;      //第一天
                                } else if (tempDate == Convert.ToDateTime(endDate)) {
                                    endIndex = endTime;             //最后一天
                                } else {
                                    startIndex = 1;
                                }
                                for (; startIndex <= endIndex; startIndex++) {
                                    sqlComm = string.Format("select value{0} from `data` where date = date('{1}') and threshold = {2}", startIndex, tempDate.ToString("yyyy-M-d"), YuZhiCbB.SelectedIndex);
                                    comm = new MySqlCommand(sqlComm, myConn);
                                    dr = comm.ExecuteReader();
                                    //MessageBox.Show(tempDate.ToString("yyyy-m-d"));
                                    if (dr.Read()) {
                                        //MessageBox.Show(dr.GetString(0));
                                        l.Add(dr.GetInt32(0));
                                    }
                                    dr.Close();
                                }

                            }
                        }

                        StartSlider.Minimum = 1;
                        StartSlider.Maximum = l.Count;
                        EndSlider.Minimum = 1;
                        EndSlider.Maximum = l.Count;
                        EndSlider.Value = EndSlider.Maximum;

                        string rComm = null;
                        foreach (int i in l) {
                            rComm += i + ",";
                        }
                        rComm = rComm.Substring(0, rComm.Length - 1);

                        re.Evaluate("Index <- c(" + rComm + ")");

                        try {
                            avg = re.Evaluate("mean(Index)").AsNumeric()[0];
                            avg = Math.Round(avg, 2);
                            AvgLabel.Content = avg.ToString();

                            sd2 = re.Evaluate("var(Index)").AsNumeric()[0];
                            sd2 = Math.Round(sd2, 2);
                            Sd2Label.Content = sd2.ToString();

                            max = re.Evaluate("max(Index)").AsInteger()[0];
                            MaxLabel.Content = max.ToString();

                            min = re.Evaluate("min(Index)").AsInteger()[0];
                            MinLabel.Content = min.ToString();

                            median = re.Evaluate("median(Index)").AsInteger()[0];
                            MedianLabel.Content = median.ToString();

                            mode = Convert.ToInt32(re.Evaluate("names(which.max(table(Index)))").AsCharacter()[0]);
                            ModeLabel.Content = mode.ToString();
                        } catch {

                        }


                    } else {
                        MessageBox.Show("请选择时段并且确认按下了确定按钮");
                    }
                }
            }

            hasStatic = true;
        }

        private void DrawBtn_Click(object sender, RoutedEventArgs e)
        {
            // 
            //             if (StartPageCbB1.SelectedIndex >= 0 && EndPageCbB1.SelectedIndex >= 0) {
            //                 if (PicTypeCbB.SelectedIndex != -1) {
            //                     int startPage = Convert.ToInt32(StartPageCbB1.SelectedValue);
            //                     int endPage = Convert.ToInt32(EndPageCbB1.SelectedValue);
            //                     int minIndex = (startPage - 1) * rowPerPage + 1;
            //                     int maxIndex = endPage * rowPerPage;
            //                     int startIndex = (int) StartSlider.Value;
            //                     int endIndex = (int) EndSlider.Value;
            //                     if (endIndex > length) {
            //                         endIndex = length;
            //                     }
            //                     int type = PicTypeCbB.SelectedIndex;
            // 
            //                     DrawPar o = new DrawPar(type, minIndex, maxIndex, startIndex, endIndex);
            // 
            //                     SetLoadingBarVisibilityInvoke(true);
            //                     DrawPic(o);
            //                 } else {
            //                     MessageBox.Show("请选择图形类别");
            //                 }
            //             } else {
            //                 MessageBox.Show("请选择页数");
            //             }

            if (hasStatic) {
                int type = PicTypeCbB.SelectedIndex;
                int startIndex = (int)StartSlider.Value;
                int endIndex = (int) EndSlider.Value;

                switch (type) {
                    case -1 :
                        MessageBox.Show("请选择画图类型");
                        break;
                    case 0:
                        re.Evaluate(string.Format("plot(Index,type=\"h\",ylab=\"value\",xlim=c({0},{1}))",startIndex,endIndex));
                        break;
                    case 1:
                        re.Evaluate(string.Format("plot(Index,type=\"l\",ylab=\"value\",xlim=c({0},{1}))", startIndex, endIndex));
                        break;
                    case 2:
                        re.Evaluate(string.Format("pie(Index)"));
                        break;
                    case 3:
                        re.Evaluate(string.Format("plot(table(Index[{0}:{1}]),ylab=\"value\")", startIndex, endIndex));
                        break;
                    case 4:
                        re.Evaluate(string.Format("plot(Index,type=\"p\",ylab=\"value\",xlim=c({0},{1}))", startIndex, endIndex));
                        break;
                }
            } else {
                MessageBox.Show("请选进行【统计】操作");
            }


        }

        private void StartSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            if (StartSlider.Value > EndSlider.Value) {
                EndSlider.Value = StartSlider.Value + 1;
            }
        }

        private void EndSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            if (EndSlider.Value < StartSlider.Value) {
                StartSlider.Value = EndSlider.Value - 1;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type">类型：
        /// 0：条形图
        /// 1：折线图
        /// 2：饼图
        /// 3：分布图
        /// 4：散点图
        /// </param>
        /// <param name="minIndex">最小值</param>
        /// <param name="maxIndex">最大值</param>
        /// <param name="startIndex">可视范围开始</param>
        /// <param name="endIndex">可视范围结束</param>
        private void DrawPic(DrawPar o)
        {
            int type = o.Type;
            int minIndex = o.MinIndex;
            int maxIndex = o.MaxIndex;
            int startIndex = o.StartIndex;
            int endIndex = o.EndIndex;


            try {
                switch (type) {
                    case 0:
                        re.Evaluate(string.Format("plot(tb[{0}:{1},1],type=\"h\",xlim=c({2},{3}),ylab=\"value\")", minIndex, maxIndex, startIndex, endIndex));
                        break;
                    case 1:
                        re.Evaluate(string.Format("plot(tb[{0}:{1},1],type=\"l\",xlim=c({2},{3}),ylab=\"value\")", minIndex, maxIndex, startIndex, endIndex));
                        break;
                    case 2:
                        re.Evaluate(string.Format("pie(tb[{0}:{1},1])", startIndex, endIndex));
                        break;
                    case 3:
                        re.Evaluate(string.Format("plot(table(tb[{0}:{1},1]),ylab=\"value\")", startIndex, endIndex));
                        break;
                    case 4:
                        re.Evaluate(string.Format("plot(tb[{0}:{1},1],type=\"p\",xlim=c({2},{3}),ylab=\"value\")", minIndex, maxIndex, startIndex, endIndex));
                        break;
                }
            } catch (Exception ex) {
                MessageBox.Show(ex.Message);
            }

            SetLoadingBarVisibilityInvoke(false);
        }

        private void Label_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            int now = (int) StartSlider.Value;
            SetSliderValue win = new SetSliderValue(now);
            win.ShowDialog();
            if (win.isSet) {
                StartSlider.Value = win.newValue;
            }
        }

        private void Label_MouseDoubleClick_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            int now = (int) EndSlider.Value;
            SetSliderValue win = new SetSliderValue(now);
            win.ShowDialog();
            if (win.isSet) {
                EndSlider.Value = win.newValue;
            }
        }


        private void SelectTimeBtn_Click(object sender, RoutedEventArgs e)
        {
            if (myConn != null && CowIdCbB.SelectedIndex != -1) {
                int id = Convert.ToInt32(CowIdCbB.SelectedValue);
                string commText = "select min(date),max(date) from `data` where cowId = " + id;

                MySqlCommand comm = new MySqlCommand(commText, myConn);
                MySqlDataReader dr = comm.ExecuteReader();
                DateTime sd;            //StartDate
                DateTime ed;            //EndDate

                if (dr.Read()) {
                    sd = dr.GetDateTime(0);
                    ed = dr.GetDateTime(1);
                    //MessageBox.Show(sd.ToShortDateString() + ed.ToShortDateString());
                    SelectTime stWin;

                    if (ht.Contains(id)) {
                        stWin = (SelectTime) ht[id];
                    } else {
                        stWin = new SelectTime(sd, ed);
                        ht.Add(id, stWin);
                    }

                    stWin.isOK = false;
                    stWin.ShowDialog();
                }
                dr.Close();
            } else {
                MessageBox.Show("请选择奶牛ID");
            }
        }


        private void SmoothBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    public class DrawPar
    {
        private int type;

        public int Type
        {
            get { return type; }
            set { type = value; }
        }
        private int minIndex;

        public int MinIndex
        {
            get { return minIndex; }
            set { minIndex = value; }
        }
        private int maxIndex;

        public int MaxIndex
        {
            get { return maxIndex; }
            set { maxIndex = value; }
        }
        private int startIndex;

        public int StartIndex
        {
            get { return startIndex; }
            set { startIndex = value; }
        }
        private int endIndex;

        public int EndIndex
        {
            get { return endIndex; }
            set { endIndex = value; }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="t">type</param>
        /// <param name="m">minIndex</param>
        /// <param name="max">maxIndex</param>
        /// <param name="s">startIndex</param>
        /// <param name="e">endIndex</param>
        public DrawPar(int t, int m, int max, int s, int e)
        {
            type = t;
            minIndex = m;
            maxIndex = max;
            startIndex = s;
            endIndex = e;
        }
    }
}