﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrendAnalysis.Service.Trend
{
    /// <summary>
    /// 解析因子结果
    /// </summary>
    /// <typeparam name="T">因子类型</typeparam>
    public class FactorTrendAnalyseResult<T>
    {
        /// <summary>
        /// 因子
        /// </summary>
        public List<T> Factor { get; set; }


        /// <summary>
        /// 结果因子（反因子）
        /// </summary>
        public List<T> OppositeFactor { get; set; }

        /// <summary>
        /// 历史的连续次数分析结果，键为次数，值为数量  表示每个连续次数出现的次数
        /// </summary>
        public SortedDictionary<int, int> HistoricalConsecutiveTimes { get; set; } = new SortedDictionary<int, int>();

        /// <summary>
        /// 当前指定期次此因子连续次数
        /// </summary>
        public int FactorCurrentConsecutiveTimes { get; set; }


        /// <summary>
        /// 最大连续期数-指定期次此因子连续次数的间隔数，数越小，表示变化的趋势越大
        /// </summary>
        public int Interval => HistoricalConsecutiveTimes == null || HistoricalConsecutiveTimes.Count == 0 ? 0 : HistoricalConsecutiveTimes.Max(k => k.Key) - FactorCurrentConsecutiveTimes;

    }
}
