﻿using System.Collections.Generic;
using System.Text;


namespace TrendAnalysis.Models.Trend
{

    /// <summary>
    ///     历史趋势
    /// </summary>
    public class HistoricalTrend<T> : BaseEntity
    {

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
        ///     总期次
        /// </summary>
        public int TotalHistoricalCount { get; set; }


        /// <summary>
        ///     正确率
        /// </summary>
        public double CorrectRate => AnalyticalCount == 0 ? 0 : (double) CorrectCount/AnalyticalCount;

        public List<HistoricalTrendItem<T>> Items { get; set; }


        public override string ToString()
        {
            var content = new StringBuilder();
            content.AppendLine(string.Format("第{0}位号码，允许连续次数{1},允许间隔数{2}。出现次数{3}，正确次数{4},正确率：{5:0.00%}", Location, AllowConsecutiveTimes, AllowInterval, AnalyticalCount, CorrectCount, CorrectRate));
            if (Items != null && Items.Count > 0)
            {
                foreach (var item in Items)
                {
                    var message = string.Format("期次：{0},号码：{1},分析结果：{2},结果连续次数:{3}结果间隔数：{4}  ", item.Times, item.Number, item.Success ? "-Yes- " : "-No-  ", item.ResultConsecutiveTimes, item.ResultInterval);
                    content.AppendLine(message + (item.PredictiveFactor != null ? string.Join(";", item.PredictiveFactor) : ""));
                }
            }
            return content.ToString();
        }

    }

}