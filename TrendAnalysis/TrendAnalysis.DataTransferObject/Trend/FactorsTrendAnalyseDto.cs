﻿using System.Collections.Generic;
using TrendAnalysis.Models.Trend;


namespace TrendAnalysis.DataTransferObject.Trend
{

    /// <summary>
    ///     分析数字记录的传输对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FactorsTrendAnalyseDto<T> : BaseTrendAnalyseDto<T>
    {

        /// <summary>
        ///     因子列表
        /// </summary>
        public List<Factor<T>> Factors { get; set; }



    }

}