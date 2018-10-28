using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using CommonCSharpLibary.CustomExtensions;
using CommonCSharpLibary.KeyMouseOperation;

namespace CryptoCommon.Chart
{
    public class CustomChart : IDisposable
    {
        public SeriesCollection ChartSeries { get { return _chart.Series; } }
        public bool _bShowChart = false;
        public bool IsAllowDispose { get; set; }  //set this to false if we do not allow user to dispose the chart

        protected Form _form;
        protected System.Windows.Forms.DataVisualization.Charting.Chart _chart;
        protected Label _label;
        protected Label _labelBot;

        public CustomChart()
        {
            this.Create();
        }
        public CustomChart(string title)
            : this()
        {
            _form.Text = title;
        }

        private void Create()
        {
            _form = new Form();
            _form.Width = (int)(Screen.PrimaryScreen.Bounds.Width * 0.8);
            _form.Height = (int)(Screen.PrimaryScreen.Bounds.Height * 0.8);
            _form.Visible = false;
            //_form.FormClosing += (s, e) => { _form.Hide(); e.Cancel = true; };
            //_form.FormClosing += form_FormClosing;

            _chart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            _form.Controls.Add(_chart);
            _chart.Anchor = (AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right);
            _chart.SetBounds(0, 0, _form.Width, _form.Height - 50);
            //_chart.ChartAreas.Add(new ChartArea("Area1"));

            _chart.CursorPositionChanged += this.chart_CursorPositionChanged;
            _chart.KeyDown += this.chart_KeyDown;
            _chart.PreviewKeyDown += this.chart_PreviewKeyDown;
            _chart.MouseClick += _chart_MouseClick;
            _chart.Focus();

            //_label = new Label();
            //_form.Controls.Add(_label);
            //_label.Anchor = (AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right);
            //_label.Width = _form.Width - 50;
            //_label.Height = 30;
            //_label.Padding = new System.Windows.Forms.Padding(5);
            //_label.AutoSize = false;
            //_label.Enabled = true;
            //_label.Visible = true;
            //_label.TabIndex = 0;
            //_label.Location = new Point(0, 0);
            //_label.Name = "_label";
            //_label.TextAlign = ContentAlignment.TopRight;

            //_labelBot = new Label();
            //_form.Controls.Add(_labelBot);
            //_labelBot.Anchor = (AnchorStyles.Left | AnchorStyles.Bottom | AnchorStyles.Right);
            //_labelBot.Width = _form.Width;
            //_labelBot.Enabled = true;
            ////_labelBot.Location = new Point(0, 10);
            //_labelBot.Visible = true;
            //_labelBot.Name = "_labelBot";

            _form.Location = new Point(0, 20);
            IsAllowDispose = true;
        }

        protected virtual void _chart_MouseClick(object sender, MouseEventArgs e)
        {
        }

        protected virtual void chart_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
        }
        protected virtual void chart_KeyDown(object sender, KeyEventArgs e)
        {
        }
        protected virtual void chart_CursorPositionChanged(object sender, CursorEventArgs e)
        {
        }

        public virtual void ShowChart(bool show, int l = -1, int t = -1, int w = -1, int h = -1)
        {
            if (show)
            {
                _form.InvokeIfRequired(c => c.Show());
                if (l != -1 && t != -1 && w != -1 && h != -1)
                    WindowMove.SetWindowPosition(_form.Handle, l, t, w, h);
                else if (l != -1 && t != -1)
                    WindowMove.SetWindowPosition(_form.Handle, l, t, -1, -1, WindowMove.SWP_NOSIZE);

                //disable the Close button, such that we cannot manually cose the chart
                _form.InvokeIfRequired(c =>
                {
                    IntPtr hSystemMenu = UnsafeNativeMethods.GetSystemMenu(_form.Handle, false);
                    UnsafeNativeMethods.RemoveMenu(hSystemMenu, UnsafeNativeMethods.SC_CLOSE, UnsafeNativeMethods.MF_BYCOMMAND);      //remove the "X". both work and we need only one.
                });
            }
            else
            {
                _form.InvokeIfRequired(c => c.Hide());
            }

            _bShowChart = show;
        }
        public void Dispose()
        {
            _form.InvokeIfRequired(c => c.Close());
        }
    }
}
