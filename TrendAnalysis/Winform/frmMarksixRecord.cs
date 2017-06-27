﻿using System;
using System.Windows.Forms;
using TrendAnalysis.Service;
using TrendAnalysis.DataTransferObject;
using Common;
using System.Data;
using System.Linq;
using TrendAnalysis.Models;
using System.Drawing;
using System.ComponentModel;
using System.Collections.Generic;

namespace Winform
{
    public partial class frmMarksixRecord : Form
    {
        /// <summary>
        /// 导入的记录条数
        /// </summary>
        private int importCount = 0;
        /// <summary>
        /// 是否停止导入记录
        /// </summary>
        private bool isStopImport = false;
        /// <summary>
        /// 引用主窗口
        /// </summary>
        private frmMain frmMdi = null;
        /// <summary>
        /// 导入趋势分析后台对象
        /// </summary>
        private BackgroundWorker bgwImport = null;

        public frmMarksixRecord()
        {
            InitializeComponent();
        }

        private void tsbSearch_Click(object sender, EventArgs e)
        {
            var service = new MarkSixRecordService();
            var rows=service.Search(new MarkSixRecordSearchDto {StartIndex=0,TakeCount=20 });
            var table = rows.ConvertDataTable(properties=> 
            {
                var rowVersionProperty = properties.FirstOrDefault(p => p.Name == nameof(MarkSixRecord.RowVersion));
                if (rowVersionProperty != null)
                {
                    properties.Remove(rowVersionProperty);
                }
                var idProperty= properties.FirstOrDefault(p => p.Name == nameof(MarkSixRecord.Id));
                if (idProperty != null)
                {
                    properties.Remove(idProperty);
                }
                properties.Insert(0, idProperty);
            });
            dgvList.DataSource = table;
        }

        private void tsbImport_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("确认导入记录数据吗？", "确认", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Cancel)
            {
                return;
            }
            //文件选择窗口
            OpenFileDialog ofd = new OpenFileDialog();
            //筛选条件
            ofd.Filter = "Excel2003(*.xls)|*.xls|Excel2007(*.xlsx)|*.xlsx";
            //文件名
            string fileName = string.Empty;
            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                fileName = ofd.FileName;
            }
            if (fileName.Length > 0)
            {
                if (bgwImport == null)
                {
                    bgwImport = new BackgroundWorker();
                    //记录完成进度
                    bgwImport.WorkerReportsProgress = true;
                    //支持取消任务
                    bgwImport.WorkerSupportsCancellation = true;
                    //绑定事件
                    bgwImport.DoWork += new DoWorkEventHandler(bgwImport_DoWork);
                    bgwImport.ProgressChanged += new ProgressChangedEventHandler(bgwImport_ProgressChanged);
                    bgwImport.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgwImport_RunWorkerCompleted);
                }
                //设置工具条和菜单状态
                foreach (ToolStripItem item in this.MainMenuStrip.Items)
                {
                    item.Enabled = false;
                }
                //tsmStopImport.Visible = true;
                //tsmStopImport.Enabled = true;
                foreach (ToolStripItem item in tslMaster.Items)
                {
                    item.Enabled = false;
                }
                tsbStopImport.Visible = true;
                tsbStopImport.Enabled = true;
                frmMdi.tspbCompletePrecent.Visible = true;
                frmMdi.tspbCompletePrecent.Value = 0;
                frmMdi.tsslInfo.Text = "正在导入记录";
                frmMdi.tsslInfo.BackColor = Color.Yellow;
                //执行异步获取记录列表
                bgwImport.RunWorkerAsync(fileName);
            }
        }

        #region "处理导入记录后台事件"
        void bgwImport_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (isStopImport)
            {
                MessageBox.Show(
                    "您取消了导入记录！",
                    "取消",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.None
                    );
                frmMdi.tsslInfo.Text = "取消了导入记录";
                frmMdi.tsslInfo.BackColor = Color.Yellow;
            }
            else
            {
                if (importCount > 0)
                {
                    MessageBox.Show(
                        string.Format("成功导入{0}条记录", importCount),
                        "成功",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.None
                        );
                    frmMdi.tsslInfo.Text = "导入成功";
                    frmMdi.tsslInfo.BackColor = frmMdi.BackColor;
                }
                else
                {
                    MessageBox.Show(
                        string.Format("导入了{0}条记录", importCount),
                        "失败",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                        );
                    frmMdi.tsslInfo.Text = string.Format("导入了{0}条记录", importCount);
                    frmMdi.tsslInfo.BackColor = Color.Yellow;
                }
            }
            //设置工具条和菜单状态
            foreach (ToolStripItem item in this.MainMenuStrip.Items)
            {
                item.Enabled = true;
            }
            //tsmStopImport.Visible = false;
            foreach (ToolStripItem item in tslMaster.Items)
            {
                item.Enabled = true;
            }
            tsbStopImport.Visible = false;
            try
            {
                frmMdi.tspbCompletePrecent.Visible = false;
            }
            catch { }
        }

        void bgwImport_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            frmMdi.tspbCompletePrecent.Value = e.ProgressPercentage;
        }

        void bgwImport_DoWork(object sender, DoWorkEventArgs e)
        {
            string fileName = e.Argument.ToString();
            var service = new MarkSixRecordService();
            service.ImportingEvent += Service_ImportingEvent;
            try
            {
                service.Import(fileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show("导入记录时错误，" + ex.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                bgwImport.CancelAsync();
            }
        }

        private void Service_ImportingEvent(object sender, EventArgs e)
        {
            
        }

        /// <summary>
        /// 增加记录，此方法作为业务层添加列表内容的参数
        /// </summary>
        /// <returns>是否停止批量保存</returns>
        private bool AddRecord()
        {
            if (bgwImport.CancellationPending == true)
            {
                return true;
            }
            else
            {
                int value = frmMdi.tspbCompletePrecent.Value + 1;
                int max = frmMdi.tspbCompletePrecent.Maximum;
                if (value > max)
                {
                    value = value - max;
                }
                bgwImport.ReportProgress(value);
                return false;
            }
        }
        void helper_GetingRecord(object sender, EventArgs e)
        {
            //ExcelHelper helper = sender as ExcelHelper;
            //if (helper != null)
            //{
            //    if (bgwImport.CancellationPending == true)
            //    {
            //        helper.IsStop = true;
            //        return;
            //    }
            //}
            //int value = frmMdi.tspbCompletePrecent.Value + 1;
            //int max = frmMdi.tspbCompletePrecent.Maximum;
            //if (value > max)
            //{
            //    value = value - max;
            //}
            //bgwImport.ReportProgress(value);
        }
        #endregion

        private void tsbExport_Click(object sender, EventArgs e)
        {

        }

        private void tsbExit_Click(object sender, EventArgs e)
        {

        }

        private void tsbStopImport_Click(object sender, EventArgs e)
        {

        }

        private void frmMarksixRecord_Load(object sender, EventArgs e)
        {
            tsbSearch_Click(null, e);
        }

        private void dgvList_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            ////自动调整列宽
            //dgvList.AutoResizeColumns();
            //设置ID列只读
            dgvList.Columns[nameof(MarkSixRecord.Id)].ReadOnly = true;
            //设置日期列的显示格式
            dgvList.Columns["开奖日期"].DefaultCellStyle.Format = "yyyy-MM-dd";

        }

        private void dgvList_CellMouseEnter(object sender, DataGridViewCellEventArgs e)
        {
            int rowIndex = e.RowIndex;
            if (rowIndex < 0 || rowIndex > dgvList.Rows.Count) { return; }
            dgvList.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.MistyRose;

        }

        private void dgvList_CellMouseLeave(object sender, DataGridViewCellEventArgs e)
        {
            int rowIndex = e.RowIndex;
            if (rowIndex < 0 || rowIndex > dgvList.Rows.Count) { return; }
            dgvList.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.Empty;
        }
    }
}