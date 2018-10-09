using SimulationEngine;
using System;
using System.Collections;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;

namespace ICEInstrumentMonitor
{
    public partial class MainForm : Form
    {
        private IPriceEngine _priceEngine;
        private ManualResetEvent _event = new ManualResetEvent(false);
        private int _sortColumn = -1;

        private class ColumnComparer : IComparer
        {
            private readonly int _column;
            private readonly SortOrder _sorting;

            public ColumnComparer(int column, SortOrder sorting)
            {
                _column = column;
                _sorting = sorting;
            }

            public int Compare(object x, object y)
            {
                ListViewItem lhs = (ListViewItem)x;
                ListViewItem rhs = (ListViewItem)y;

                int result;
                switch (_column)
                {
                    case 0:
                        result = String.Compare(lhs.Text, rhs.Text);
                        break;

                    case 1:
                        result = Decimal.Compare(((IMarketData)lhs.Tag).Price, ((IMarketData)rhs.Tag).Price);
                        break;

                    case 2:
                        result = DateTime.Compare(((IMarketData)lhs.Tag).TicksUTC, ((IMarketData)rhs.Tag).TicksUTC);
                        break;

                    default:
                        throw new NotImplementedException();
                }

                if (_sorting == SortOrder.Descending)
                {
                    result *= -1;
                }
                return result;
            }
        }

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _priceEngine = new SingleSourceEngine();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_priceEngine.IsStarted)
            {
                _event.Set();
                backgroundWorker1.CancelAsync();
                _priceEngine.Stop();
            }
        }

        private void fileToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            startToolStripMenuItem.Enabled = !_priceEngine.IsStarted;
            stopToolStripMenuItem.Enabled = _priceEngine.IsStarted;
        }

        private void startToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _priceEngine.Start();
            _event.Reset();
            backgroundWorker1.RunWorkerAsync();
        }

        private void stopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _event.Set();
           backgroundWorker1.CancelAsync();
            _priceEngine.Stop();
        }

        private void editToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
           removeToolStripMenuItem.Enabled = listView1.SelectedItems.Count == 1;
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dlg = new AddInstrumentForm();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _priceEngine.Subscribe(dlg.textBoxTicker.Text);
            }
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                _priceEngine.Unsubscribe(item.Text);
                listView1.Items.Remove(item);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            for (;;)
            {
                if (_event.WaitOne(0)) break;

                IMarketData data;
                if (_priceEngine.Feed.TryDequeue(out data))
                {
                    this.Invoke(new Action<IMarketData>(AddOrUpdatePrice), data);
                }
            }
        }

        private void listView1_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            if (e.Column != _sortColumn)
            {
                _sortColumn = e.Column;
                listView1.Sorting = SortOrder.Ascending;
            }
            else
            {
                if (listView1.Sorting == SortOrder.Ascending)
                {
                    listView1.Sorting = SortOrder.Descending;
                }
                else
                {
                    listView1.Sorting = SortOrder.Ascending;
                }
            }
            listView1.ListViewItemSorter = new ColumnComparer(e.Column, listView1.Sorting);
            listView1.Sort();
        }

        private void AddOrUpdatePrice(IMarketData data)
        {
            foreach (ListViewItem item in listView1.Items)
            {
                if (item.Text == data.Ticker)
                {
                    item.SubItems[1].Text = $"{data.Price:0.####}";
                    item.SubItems[2].Text = $"{data.TicksUTC.ToLocalTime():HH:mm:ss.fff}";
                    item.Tag = data;
                    return;
                }
            }

            var newItem = new ListViewItem(data.Ticker);
            newItem.SubItems.Add($"{data.Price:0.####}");
            newItem.SubItems.Add($"{data.TicksUTC.ToLocalTime():HH:mm:ss.fff}");
            newItem.Tag = data;

            listView1.Items.Add(newItem);

            if (listView1.ListViewItemSorter != null)
            {
                listView1.Sort();
            }
        }
    }
}
