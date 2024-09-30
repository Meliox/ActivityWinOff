using Newtonsoft.Json;
using System.Data;
using System.Diagnostics;

namespace ActivityWinOff
{
    class DataGridViewHelpers
    {
        /// <summary>
        /// Converts the contents of a DataGridView to a JSON string representation.
        /// </summary>
        /// <param name="dgv">The DataGridView to be converted.</param>
        /// <returns>A JSON string representing the data in the DataGridView.</returns>
        public static string DataGridviewToJSON(DataGridView dgv)
        {
            DataTable dt = new DataTable();

            // Add columns to the DataTable based on the DataGridView columns
            foreach (DataGridViewColumn col in dgv.Columns)
            {
                dt.Columns.Add(col.Name);
            }

            // Add rows to the DataTable based on the DataGridView rows
            foreach (DataGridViewRow row in dgv.Rows)
            {
                // Skip the new row placeholder (if present)
                if (row.IsNewRow) continue;

                DataRow dRow = dt.NewRow();
                foreach (DataGridViewCell cell in row.Cells)
                {
                    dRow[cell.ColumnIndex] = cell.Value;
                }
                dt.Rows.Add(dRow);
            }

            // Serialize the DataTable to JSON and return it
            return JsonConvert.SerializeObject(dt);
        }

        /// <summary>
        /// Populates a DataGridView with data from a JSON string representation.
        /// </summary>
        /// <param name="dgvToPopulate">The DataGridView to be populated.</param>
        /// <param name="JSONString">The JSON string containing the data.</param>
        public static void JSONToDataGridview(DataGridView dgvToPopulate, string JSONString)
        {
            // Check if the JSON string is not empty
            if (!string.IsNullOrEmpty(JSONString))
            {
                // Deserialize the JSON string into a DataTable
                DataTable dt = JsonConvert.DeserializeObject<DataTable>(JSONString);

                // Populate the DataGridView with the rows from the DataTable
                foreach (DataRow dr in dt.Rows)
                {
                    dgvToPopulate.Rows.Add(dr.ItemArray);
                }

                // Set the ValueType for specific columns to ensure correct sorting and type handling
                dgvToPopulate.Columns[0].ValueType = typeof(string);
                dgvToPopulate.Columns[1].ValueType = typeof(string);
                dgvToPopulate.Columns[3].ValueType = typeof(int);
                dgvToPopulate.Columns[4].ValueType = typeof(int);
                dgvToPopulate.Columns[5].ValueType = typeof(bool);
            }
        }

        /// <summary>
        /// Adds an order number to the row header of each row in the given DataGridView.
        /// The order number corresponds to the row's index + 1.
        /// </summary>
        /// <param name="dgv">The DataGridView containing the rows to be updated.</param>
        public static void AddRowLabel(DataGridView dgv)
        {
            foreach (DataGridViewRow row in dgv.Rows)
            {
                // Set the row header value to the row index + 1
                row.HeaderCell.Value = (row.Index + 1).ToString();
            }

            // Refresh the DataGridView to ensure the row headers are updated
            dgv.Refresh();
        }


        /// <summary>
        /// Handles user input events for the specified DataGridView, allowing for file selection and argument editing.
        /// </summary>
        /// <param name="sender">The source of the event, typically the DataGridView.</param>
        /// <param name="e">The event data containing information about the cell that triggered the event.</param>
        public static void InputHandler(object sender, DataGridViewCellEventArgs e)
        {
            var dgv = (DataGridView)sender;

            // Handle file selection for the first column
            if (e.ColumnIndex == 0 && e.RowIndex != -1)
            {
                using (OpenFileDialog dialog = new OpenFileDialog())
                {
                    // Set the initial directory and file name if a value exists
                    if (dgv[e.ColumnIndex, e.RowIndex].Value != null &&
                        !string.IsNullOrEmpty(dgv[e.ColumnIndex, e.RowIndex].Value.ToString()))
                    {
                        dialog.InitialDirectory = System.IO.Path.GetDirectoryName(dgv[e.ColumnIndex, e.RowIndex].Value.ToString());
                        dialog.FileName = dgv[e.ColumnIndex, e.RowIndex].Value.ToString();
                    }

                    // Show the file dialog and update the cell value with the selected file path
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        dgv[e.ColumnIndex, e.RowIndex].Value = dialog.FileName;
                    }
                }
            }

            // Handle argument editing for the second column
            if (e.ColumnIndex == 1 && e.RowIndex != -1)
            {
                string argument = dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();

                using (Arguments f1 = new Arguments(argument))
                {
                    DialogResult result = f1.ShowDialog();
                    // Update the cell value if the dialog result is OK
                    if (result == DialogResult.OK)
                    {
                        dgv.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = f1.ReturnValue;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the KeyPress event for editing controls in a DataGridView, allowing specific columns to handle key presses.
        /// </summary>
        /// <param name="sender">The source of the event, typically the DataGridView.</param>
        /// <param name="e">The event data containing information about the editing control that triggered the event.</param>
        public static void DataGridViewHandleKeyPress(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            var dgv = (DataGridView)sender;

            // Unsubscribe any existing KeyPress event handler to avoid duplicate subscriptions
            e.Control.KeyPress -= DataGridViewKeyPress;

            // Check if the current cell is in the desired columns (3 or 4)
            if (dgv.CurrentCell.ColumnIndex == 3 || dgv.CurrentCell.ColumnIndex == 4)
            {
                // Cast the editing control to a TextBox and subscribe to the KeyPress event
                if (e.Control is TextBox tb)
                {
                    tb.KeyPress += DataGridViewKeyPress;
                }
            }
        }

        /// <summary>
        /// Handles the KeyPress event for a DataGridView, allowing only digit inputs or control characters.
        /// </summary>
        /// <param name="sender">The source of the event, typically the TextBox being edited in the DataGridView.</param>
        /// <param name="e">The event data containing information about the key press event.</param>
        public static void DataGridViewKeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only digit inputs or control characters (e.g., backspace)
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true; // Mark the event as handled to prevent further processing
            }
        }

        public static async void Sleep(int t)
        {
            await Task.Delay(t * 1000);
        }

    /// <summary>
    /// Executes actions defined in a DataGridView, starting processes based on the parameters in each row.
    /// The method supports pre- and post-execution delays and optional waiting for the process to exit.
    /// </summary>
    /// <param name="dgv">The DataGridView containing actions to execute.</param>
    public static async void DataGridViewExecutor(DataGridView dgv)
    {
        Logger.add(1, "Starting action(s)");

        foreach (DataGridViewRow row in dgv.Rows)
        {
            // Retrieve the program path and arguments
            string programPath = row.Cells[0].Value?.ToString() ?? string.Empty;
            string arguments = row.Cells[1].Value?.ToString() ?? string.Empty;

            // Determine the window style
            ProcessWindowStyle windowStyle = row.Cells[2].Value?.ToString() switch
            {
                "Hidden" => ProcessWindowStyle.Hidden,
                "Maximized" => ProcessWindowStyle.Maximized,
                "Minimized" => ProcessWindowStyle.Minimized,
                _ => ProcessWindowStyle.Normal,
            };

            // Retrieve delays and wait for exit options
            int delayBeforeExecution = row.Cells[3].Value != null ? Int32.Parse(row.Cells[3].Value.ToString()) : 0;
            int delayAfterExecution = row.Cells[4].Value != null ? Int32.Parse(row.Cells[4].Value.ToString()) : 0;
            bool waitForExit = Boolean.Parse(row.Cells[5].Value?.ToString() ?? "false");

            Logger.add(1, $"Action: Program={programPath}, Args={arguments}, WindowStyle={row.Cells[2].Value}, PreDelay={delayBeforeExecution}, PostDelay={delayAfterExecution}");

            try
            {
                await Task.Delay(delayBeforeExecution * 1000);

                ProcessStartInfo pInfo = new ProcessStartInfo
                {
                    FileName = programPath,
                    Arguments = arguments,
                    WindowStyle = windowStyle
                };

                using Process p = Process.Start(pInfo);
                if (waitForExit && p != null)
                {
                    await Task.Run(() => p.WaitForExit());
                }

                await Task.Delay(delayAfterExecution * 1000);
            }
            catch (Exception ex)
            {
                Logger.add(1, $"Action (Failed): Program={programPath}, Args={arguments}, WindowStyle={row.Cells[2].Value}, PreDelay={delayBeforeExecution}, PostDelay={delayAfterExecution}. Error: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Moves the specified row up by one position in the given DataGridView.
    /// If the row is the first one, it will not perform any action.
    /// </summary>
    /// <param name="dgv">The DataGridView containing the rows to be moved.</param>
    /// <param name="row">The DataGridViewRow to be moved up.</param>
    public static void DataGridViewMoveUp(DataGridView dgv, DataGridViewRow row)
        {
            if (row == null || row.Index == 0)
                return; // No row to move up or already at the first row.

            // Get the index of the current row
            int currentIndex = row.Index;

            // Swap the current row with the previous row
            DataGridViewRow prevRow = dgv.Rows[currentIndex - 1];

            // Store the current row's data temporarily
            var tempCells = new object[row.Cells.Count];

            // Copy the current row's data to the temp array
            for (int i = 0; i < row.Cells.Count; i++)
            {
                tempCells[i] = row.Cells[i].Value;
            }

            // Swap values between the two rows
            for (int i = 0; i < row.Cells.Count; i++)
            {
                row.Cells[i].Value = prevRow.Cells[i].Value;
                prevRow.Cells[i].Value = tempCells[i];
            }

            // Update the selection to the moved row
            dgv.ClearSelection();
            dgv.Rows[currentIndex - 1].Selected = true; // Select the newly moved row

            // Set the current cell to the first cell of the newly selected row
            dgv.CurrentCell = dgv.Rows[currentIndex - 1].Cells[0];
        }

        /// <summary>
        /// Moves the specified row down by one position in the given DataGridView.
        /// If the row is the last one, it will not perform any action.
        /// </summary>
        /// <param name="dgv">The DataGridView containing the rows to be moved.</param>
        /// <param name="row">The DataGridViewRow to be moved down.</param>
        public static void DataGridViewMoveDown(DataGridView dgv, DataGridViewRow row)
        {
            if (row == null || row.Index == dgv.Rows.Count - 1)
                return; // No row to move down or already at the last row.

            // Get the index of the current row
            int currentIndex = row.Index;

            // Swap the current row with the next row
            DataGridViewRow nextRow = dgv.Rows[currentIndex + 1];

            // Store the current row's data temporarily
            var tempCells = new object[row.Cells.Count];

            // Copy the current row's data to the temp array
            for (int i = 0; i < row.Cells.Count; i++)
            {
                tempCells[i] = row.Cells[i].Value;
            }

            // Swap values between the two rows
            for (int i = 0; i < row.Cells.Count; i++)
            {
                row.Cells[i].Value = nextRow.Cells[i].Value;
                nextRow.Cells[i].Value = tempCells[i];
            }

            // Update the selection to the moved row
            dgv.ClearSelection();
            dgv.Rows[currentIndex + 1].Selected = true; // Select the newly moved row

            // Set the current cell to the first cell of the newly selected row
            dgv.CurrentCell = dgv.Rows[currentIndex + 1].Cells[0];
        }
    }
}