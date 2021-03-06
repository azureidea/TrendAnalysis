﻿using System.Collections.Generic;
using System.Text;
using TrendAnalysis.Models.DataBase;


namespace TrendAnalysis.Models.Trend
{

    /// <summary>
    ///     历史趋势
    /// </summary>
    public class HistoricalFactorTrend 
    {

        /// <summary>
        ///     历史趋势类型
        /// </summary>
        public HistoricalTrendTypeEnum HistoricalTrendType { get; set; }


        /// <summary>
        /// 明细项分类
        /// </summary>
        public int HistoricalTrendItemType { get; set; }


        /// <summary>
        ///     类型说明
        /// </summary>
        public string TypeDescription { get; set; }


        /// <summary>
        ///     开始期次
        /// </summary>
        public int StartTimes { get; set; }


        /// <summary>
        ///     第几位数
        /// </summary>
        public int Location { get; set; }


        /// <summary>
        ///     允许的连续次数
        /// </summary>
        public int AllowConsecutiveTimes { set; get; }


        /// <summary>
        ///     允许的间隔数
        /// </summary>
        public int AllowInterval { get; set; }


        /// <summary>
        ///     正确次数
        /// </summary>
        public int CorrectCount { get; set; }


        /// <summary>
        ///     有分析结果的次数（符合连续次数和间隔数的记录次数）
        /// </summary>
        public int AnalyticalCount { get; set; }


        /// <summary>
        ///     分析多少位号码
        /// </summary>
        public int AnalyseNumberCount { get; set; }

        /// <summary>
        ///     正确率
        /// </summary>
        public double CorrectRate { get; set; } //=>AnalyticalCount == 0 ? 0 : (double) CorrectCount/AnalyticalCount;


        public override string ToString()
        {
            var content = new StringBuilder();
            content.AppendLine(string.Format("第{0}位号码，允许连续次数{1},允许间隔数{2}。出现次数{3}，正确次数{4},正确率：{5:0.00%}", Location, AllowConsecutiveTimes, AllowInterval, AnalyticalCount, CorrectCount, CorrectRate));
            return content.ToString();
        }

    }

}