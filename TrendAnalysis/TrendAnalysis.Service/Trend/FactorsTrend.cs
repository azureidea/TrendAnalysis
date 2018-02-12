﻿using System;
using System.Collections.Generic;
using System.Linq;
using TrendAnalysis.DataTransferObject;
using TrendAnalysis.DataTransferObject.Trend;
using TrendAnalysis.Models.Trend;


namespace TrendAnalysis.Service.Trend
{

    /// <summary>
    ///     因子的历史趋势
    /// </summary>
    public class FactorsTrend
    {

        /// <summary>
        ///     非连续指示
        /// </summary>
        public const int DiscontinuousFlag = int.MaxValue;


        /// <summary>
        ///     统计连续次数允许的最小次数
        /// </summary>
        public const int AllowMinTimes = 1;


        public List<Factor<T>> Analyse<T>(FactorsTrendAnalyseDto<T> dto)
        {
            //预测的可能因子
            var predictiveFactors = new List<Factor<T>>();

            //分析历史趋势,排除最后一位号码，（最后一位号码分析当前要分析的可能号码）
            var historicalNumbers = dto.Numbers.Take(dto.Numbers.Count - 1).ToList();

            //分析每个因子
            foreach (var factor in dto.Factors)
            {
                //统计每个因子在记录中的趋势
                var trendResult = CountFactorContinuousTimes(dto.Numbers, factor.Left, factor.Right);

                //行明细结果集
                var rowDetailses = trendResult.RowDetailses;
                if (rowDetailses == null || rowDetailses.Count == 0) continue;
                var lastIndexResult = rowDetailses[rowDetailses.Count - 1];

                //因子不包含最后一个号码，（连续次数为0）
                if (lastIndexResult.ConsecutiveTimes == 0)
                    continue;

                var historicalTrends = GetCorrectRates(historicalNumbers, trendResult, dto.AnalyseHistoricalTrendCount, factor.Right);

                //筛选正确100%的历史趋势，如没有不记录
                //historicalTrends = historicalTrends.Where(h => h.CorrectRate == 1).OrderBy(h => h.AllowInterval).ThenByDescending(h => h.AllowContinuousTimes).ToList();
                historicalTrends = historicalTrends.Where(h => h.CorrectRate == 1).OrderByDescending(h => h.AllowContinuousTimes).ThenBy(h => h.AllowInterval).ToList();
                if (historicalTrends.Count == 0) continue;

                var firstHistoricalTrend = historicalTrends.FirstOrDefault();
                if (firstHistoricalTrend == null)
                    continue;

                //可以考虑加大连续次数和间隔数
                if (lastIndexResult.ConsecutiveTimes >= firstHistoricalTrend.AllowContinuousTimes + dto.AddConsecutiveTimes && lastIndexResult.MaxConsecutiveTimesInterval <= firstHistoricalTrend.AllowInterval - dto.AddInterval)
                {
                    predictiveFactors.Add(factor);
                    continue;
                }
            }
            return predictiveFactors;
        }


        /// <summary>
        ///     分析因子一段日期的历史趋势，（通过号码集合分析历史趋势）
        /// </summary>
        /// <param name="numbers">号码集合</param>
        /// <param name="trendResult">统计结果</param>
        /// <param name="analyseNumberCount">要分析多少位记录</param>
        /// <param name="predictiveFactor">可能的因子</param>
        /// <returns></returns>
        public List<FactorTrendCorrectRate> GetCorrectRates<T>(List<T> numbers, FactorTrendContinuousDistribution<T> trendResult, int analyseNumberCount, List<T> predictiveFactor)
        {
            var trends = new List<FactorTrendCorrectRate>();

            if (analyseNumberCount <= 0)
                throw new Exception("分析历史趋势时，分析记录数量不能小于等于0！");

            if (numbers == null || numbers.Count == 0)
                throw new Exception("分析历史趋势时，记录不能为空！");

            var numberCount = numbers.Count;

            if (numberCount < analyseNumberCount)
                throw new Exception("分析历史趋势时，分析记录数量不能大于记录数量！");

            var minConsecutiveTimes = trendResult.RowDetailses.Where(n => n.ConsecutiveTimes != 0).Min(n => n.ConsecutiveTimes);
            var maxConsecutiveTimes = trendResult.RowDetailses.Where(n => n.ConsecutiveTimes != 0).Max(n => n.ConsecutiveTimes);

            var minInterval = trendResult.RowDetailses.Where(n => n.MaxConsecutiveTimesInterval != DiscontinuousFlag).Min(n => n.MaxConsecutiveTimesInterval);
            var maxInterval = trendResult.RowDetailses.Where(n => n.MaxConsecutiveTimesInterval != DiscontinuousFlag).Max(n => n.MaxConsecutiveTimesInterval);

            //允许的连续次数，由小到大
            for (var consecutiveTimes = minConsecutiveTimes; consecutiveTimes <= maxConsecutiveTimes; consecutiveTimes++)
            {
                //允许的间隔数，由大到小
                for (var interval = maxInterval; interval >= minInterval; interval--)
                {
                    var resultCount = 0;
                    var successCount = 0;

                    var trend = new FactorTrendCorrectRate
                    {
                        AllowContinuousTimes = consecutiveTimes,
                        AllowInterval = interval,
                        AnalyseNumberCount = analyseNumberCount
                    };
                    trends.Add(trend);

                    //行明细结果集
                    var rowDetailses = trendResult.RowDetailses;
                    for (var i = numberCount - 1; i >= analyseNumberCount; i--)
                    {
                        var number = numbers[i];

                        //上一索引位置的分析结果,10个号码，分析第10位（索引位置9），取第9位（索引位置8）
                        var curIndexResult = rowDetailses[i - 1];

                        //对结果再分析
                        //1、按允许的最小因子当前连续次数和允许的最大间隔次数筛选
                        //2、先按最大连续次数然后按最小间隔次数排序
                        if (curIndexResult.ConsecutiveTimes >= consecutiveTimes && curIndexResult.MaxConsecutiveTimesInterval <= interval)
                        {
                            if (predictiveFactor != null && predictiveFactor.Count > 0)
                            {
                                resultCount++;

                                if (predictiveFactor.Contains(number))
                                {
                                    successCount++;
                                }
                            }
                        }
                    }
                    trend.AnalyticalCount = resultCount;
                    trend.CorrectCount = successCount;
                    trend.CorrectRate = trend.AnalyticalCount == 0 ? 0 : (double)trend.CorrectCount / trend.AnalyticalCount;
                }
            }
            return trends;
        }


        /// <summary>
        ///     统计因子在记录中的连续次数
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="numbers">记录</param>
        /// <param name="factor">判断因子</param>
        /// <param name="predictiveFactor">反因子</param>
        /// <returns></returns>
        public static FactorTrendContinuousDistribution<T> CountFactorContinuousTimes<T>(IReadOnlyList<T> numbers, List<T> factor, List<T> predictiveFactor)
        {
            var curResult = new FactorTrendContinuousDistribution<T>
            {
                Factor = factor,
                PredictiveFactor = predictiveFactor,
                ContinuousDistributions = new SortedDictionary<int, int>(),
                RowDetailses = new List<FactorTrendContinuousDistributionRowDetails>()
            };
            var i = 0;

            //最大连续次数
            var maxConsecutiveTimes = 0;

            //连续次数
            var times = 0;
            var length = numbers.Count;
            while (i < length)
            {
                if (factor.Contains(numbers[i]))
                {
                    times++;

                    //因子连续，最大连续次数－当前连续次数
                    curResult.RowDetailses.Add(
                        new FactorTrendContinuousDistributionRowDetails
                        {
                            Index = i,
                            MaxConsecutiveTimesInterval = maxConsecutiveTimes - times,
                            ConsecutiveTimes = times
                        });
                }
                else
                {
                    if (curResult.ContinuousDistributions.ContainsKey(times))
                    {
                        curResult.ContinuousDistributions[times]++;
                    }
                    else if (times >= AllowMinTimes)
                    {
                        curResult.ContinuousDistributions.Add(times, 1);
                    }
                    if (times > maxConsecutiveTimes)
                        maxConsecutiveTimes = times;
                    times = 0;

                    //因子不连续
                    curResult.RowDetailses.Add(
                        new FactorTrendContinuousDistributionRowDetails
                        {
                            Index = i,
                            MaxConsecutiveTimesInterval = DiscontinuousFlag,
                            ConsecutiveTimes = times
                        });
                }
                i++;
            }
            if (curResult.ContinuousDistributions.ContainsKey(times))
            {
                curResult.ContinuousDistributions[times]++;
            }
            else if (times >= AllowMinTimes)
            {
                curResult.ContinuousDistributions.Add(times, 1);
            }
            return curResult;
        }
    }

}