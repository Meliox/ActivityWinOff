using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ActivityWinOff
{
    class DataGridViewHelpers
    {
        public static string DataGridviewToJSON(DataGridView dgv)
        {
            DataTable dt = new DataTable();
            foreach (DataGridViewColumn col in dgv.Columns)
                dt.Columns.Add(col.Name);

            foreach (DataGridViewRow row in dgv.Rows)
            {
                DataRow dRow = dt.NewRow();
                foreach (DataGridViewCell cell in row.Cells)
                    dRow[cell.ColumnIndex] = cell.Value;
                dt.Rows.Add(dRow);
            }
            return JsonConvert.SerializeObject(dt);
        }

        public static void JSONToDataGridview(DataGridView dgvToPopulate, string JSONString)
        {
            if (JSONString != "")
            {
                DataTable dt = JsonConvert.DeserializeObject<DataTable>(JSONString);

                foreach (DataRow dr in dt.Rows)
                    dgvToPopulate.Rows.Add(dr.ItemArray);

                // sort imported results by order
                dgvToPopulate.Columns[0].ValueType = typeof(int);
                dgvToPopulate.Columns[1].ValueType = typeof(string);
                dgvToPopulate.Columns[2].ValueType = typeof(string);
                //dgvToPopulate.Columns[3].ValueType = typeof(ComboBox);
                dgvToPopulate.Columns[4].ValueType = typeof(int);
                dgvToPopulate.Columns[5].ValueType = typeof(int);
                dgvToPopulate.Columns[6].ValueType = typeof(bool);
                dgvToPopulate.Sort(dgvToPopulate.Columns[0], ListSortDirection.Ascending);
            }
        }

        public static void DataGridViewAddOrder(DataGridView dgv)
        {
            foreach (DataGridViewRow row in dgv.Rows)
                row.Cells[0].Value = String.Format("{0}", row.Index + 1);
        }

        public static void DataGridViewFileInput(object sender, DataGridViewCellEventArgs e)
        {
            var dgv = (DataGridView)sender;
            if (e.ColumnIndex == 1 && e.RowIndex != -1)
            {
                OpenFileDialog dialog = new OpenFileDialog();
                if (dgv[e.ColumnIndex, e.RowIndex].Value != null)
                    dialog.FileName = dgv[e.ColumnIndex, e.RowIndex].Value.ToString();
                if (dialog.ShowDialog() == DialogResult.OK)
                    dgv[e.ColumnIndex, e.RowIndex].Value = dialog.FileName;
            }
        }

        public static void DataGridViewHandleKeyPress(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            var dgv = (DataGridView)sender;
            e.Control.KeyPress -= new KeyPressEventHandler(DataGridViewKeyPress);
            if (dgv.CurrentCell.ColumnIndex == 4 || dgv.CurrentCell.ColumnIndex == 5) //Desired Column
            {
                TextBox tb = e.Control as TextBox;
                if (tb != null)
                    tb.KeyPress += new KeyPressEventHandler(DataGridViewKeyPress);
            }
        }


        public static void DataGridViewKeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
                e.Handled = true;
        }

        public static async void Sleep(int t)
        {
            await Task.Delay(t * 1000);
        }

        public static async void DataGridViewExecutor(DataGridView dgv)
        {
            Logger.add(1, "Starting action(s)");
            foreach (DataGridViewRow row in dgv.Rows)
            {
                string ProgramPath = row.Cells[1].Value.ToString();
                string Arguments;
                if (row.Cells[2].Value != null)
                    Arguments = row.Cells[2].Value.ToString();
                else
                    Arguments = "";
                ProcessWindowStyle WindowStyle = ProcessWindowStyle.Normal;
                switch (row.Cells[3].Value.ToString())
                {
                    case "Normal":
                        WindowStyle = ProcessWindowStyle.Normal;
                        break;
                    case "Hidden":
                        WindowStyle = ProcessWindowStyle.Hidden;
                        break;
                    case "Maximized":
                        WindowStyle = ProcessWindowStyle.Maximized;
                        break;
                    case "Minimized":
                        WindowStyle = ProcessWindowStyle.Minimized;
                        break;
                }
                int DelayBeforeExecution;
                if (row.Cells[4].Value != null)
                    DelayBeforeExecution = Int32.Parse(row.Cells[4].Value.ToString());
                else
                    DelayBeforeExecution = 0;
                int DelayAfterExecution;
                if (row.Cells[5].Value != null)
                    DelayAfterExecution = Int32.Parse(row.Cells[5].Value.ToString());
                else
                    DelayAfterExecution = 0;
                bool WaitForExit = Boolean.Parse(row.Cells[6].Value.ToString());

                Logger.add(1, "Action: Program=" + ProgramPath + ", Args=" + Arguments + ", WindowStyle=" + row.Cells[3].Value.ToString() + ", PreDelay=" + DelayBeforeExecution + ", PostDelay=" + DelayAfterExecution);

                await Task.Delay(DelayBeforeExecution * 1000);

                ProcessStartInfo pInfo = new ProcessStartInfo();
                pInfo.FileName = @ProgramPath;
                pInfo.Arguments = Arguments;
                pInfo.WindowStyle = WindowStyle;
                Process p = Process.Start(pInfo);
                if (WaitForExit)
                    await Task.Run(() => p.WaitForExit());

                await Task.Delay(DelayAfterExecution * 1000);
            }

        }
    }
}
