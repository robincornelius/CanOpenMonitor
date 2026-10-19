using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using libCanopenSimple;
using libEDSsharp;

namespace SDOEditorPlugin
{
    internal interface IPdoEditorDevice
    {
        Task<PdoConfigurationSnapshot> ReadAsync(PdoSelection selection);
        Task<PdoConfigurationResult> SaveAsync(SdoParameterWrite[] writes);
    }

    internal sealed class CanPdoEditorDevice : IPdoEditorDevice
    {
        private readonly libCanopenSimple.libCanopenSimple can;
        private readonly byte node;

        internal CanPdoEditorDevice(libCanopenSimple.libCanopenSimple can, byte node)
        {
            this.can = can;
            this.node = node;
        }

        public Task<PdoConfigurationSnapshot> ReadAsync(PdoSelection selection)
        {
            return PdoConfiguration.ReadAsync(can, node, selection.Index,
                selection.Record.subobjects.Keys.Select(sub => checked((byte)sub)).Concat(new byte[] { 0, 1, 2 }));
        }

        public Task<PdoConfigurationResult> SaveAsync(SdoParameterWrite[] writes)
        {
            return PdoConfiguration.ApplyAsync(can, node, writes);
        }
    }

    internal sealed class PdoEditorForm : Form
    {
        private readonly EDSsharp dictionary;
        private readonly IPdoEditorDevice device;
        private readonly ComboBox selector = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
        private readonly Button reload = new Button { Text = "Read device", AutoSize = true };
        private readonly Button save = new Button { Text = "Save to device", AutoSize = true };
        private readonly Button revert = new Button { Text = "Revert", AutoSize = true };
        private readonly Button close = new Button { Text = "Close", AutoSize = true };
        private readonly Button add = new Button { Text = "Add mapping", AutoSize = true };
        private readonly Button remove = new Button { Text = "Remove", AutoSize = true };
        private readonly Button up = new Button { Text = "Move up", AutoSize = true };
        private readonly Button down = new Button { Text = "Move down", AutoSize = true };
        private readonly Label status = new Label { Dock = DockStyle.Fill, AutoEllipsis = true, TextAlign = ContentAlignment.MiddleLeft };
        private readonly Label budget = new Label { AutoSize = true, Margin = new Padding(16, 8, 3, 3) };
        private readonly TableLayoutPanel parameters = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 4 };
        private readonly DataGridView grid = new DataGridView { Dock = DockStyle.Fill, AllowUserToAddRows = false,
            AllowUserToDeleteRows = false, RowHeadersVisible = false, MultiSelect = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells };
        private readonly Dictionary<byte, TextBox> editors = new Dictionary<byte, TextBox>();
        private List<uint> draftMappings = new List<uint>();
        private List<PdoMappingOption> options = new List<PdoMappingOption>();
        private PdoConfigurationSnapshot snapshot;
        private PdoSelection selected;
        private bool busy;
        private bool rendering;
        private bool mappingWritable;

        internal PdoEditorForm(EDSsharp dictionary, byte node, IPdoEditorDevice device)
        {
            this.dictionary = dictionary;
            this.device = device;
            Text = "Edit PDOs — Node 0x" + node.ToString("X2");
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(860, 650);
            Size = new Size(1100, 820);
            Font = SystemFonts.MessageBoxFont;
            BuildLayout();
            selector.Items.AddRange(PdoEditorModel.Selections(dictionary));
            selector.SelectedIndexChanged += SelectionChanged;
            reload.Click += async (sender, args) =>
            {
                if (DiscardDraft())
                {
                    await LoadSelection();
                }
            };
            save.Click += async (sender, args) => await Save();
            revert.Click += (sender, args) =>
            {
                RenderSnapshot();
                status.Text = "Draft reverted to the last device read. No writes sent.";
            };
            close.Click += (sender, args) => Close();
            add.Click += (sender, args) =>
            {
                if (options.Count != 0)
                {
                    draftMappings.Add(options[0].Descriptor);
                    RenderMappings();
                }
            };
            remove.Click += (sender, args) =>
            {
                if (grid.CurrentRow != null)
                {
                    draftMappings.RemoveAt(grid.CurrentRow.Index);
                    RenderMappings();
                }
            };
            up.Click += (sender, args) => MoveMapping(-1);
            down.Click += (sender, args) => MoveMapping(1);
            grid.CurrentCellDirtyStateChanged += (sender, args) =>
            {
                if (grid.IsCurrentCellDirty)
                {
                    grid.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            };
            grid.CellValueChanged += MappingChanged;
            grid.DataError += (sender, args) =>
            {
                args.ThrowException = false;
                status.Text = "Choose a mapping from the loaded dictionary.";
            };
            FormClosing += (sender, args) => args.Cancel = busy || !DiscardDraft();
            Shown += (sender, args) =>
            {
                if (selector.Items.Count != 0)
                {
                    selector.SelectedIndex = 0;
                }
                else
                {
                    status.Text = "The loaded dictionary contains no PDO communication records.";
                    SetBusy(false);
                }
            };
            SetBusy(false);
        }

        private void BuildLayout()
        {
            var root = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(10), ColumnCount = 1, RowCount = 5 };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 43));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 57));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
            root.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
            var top = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3 };
            top.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            top.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            top.Controls.Add(new Label { Text = "Device PDO", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 0);
            top.Controls.Add(selector, 1, 0);
            top.Controls.Add(reload, 2, 0);
            var communication = new GroupBox { Text = "Communication parameters — decimal or 0x hexadecimal", Dock = DockStyle.Fill };
            var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(5) };
            parameters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
            parameters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22));
            parameters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22));
            parameters.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20));
            scroll.Controls.Add(parameters);
            communication.Controls.Add(scroll);
            var mappings = new GroupBox { Text = "PDO mapping — current device values and draft", Dock = DockStyle.Fill };
            var mappingLayout = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, Padding = new Padding(5) };
            mappingLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            mappingLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mappingLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 37));
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "slot", HeaderText = "Slot", ReadOnly = true, FillWeight = 12 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "current", HeaderText = "Current hex / decoded target", ReadOnly = true, FillWeight = 100 });
            grid.Columns["current"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            grid.Columns.Add(new DataGridViewComboBoxColumn { Name = "target", HeaderText = "Draft object — index:subindex / name", FillWeight = 110,
                DisplayStyle = DataGridViewComboBoxDisplayStyle.ComboBox, DisplayMember = "Label", ValueMember = "Descriptor", ValueType = typeof(uint),
                DropDownWidth = 650, MaxDropDownItems = 20 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "hex", HeaderText = "Draft mapping hex", ReadOnly = true, FillWeight = 38 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "bits", HeaderText = "Bits", ReadOnly = true, FillWeight = 14 });
            var actions = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            actions.Controls.AddRange(new Control[] { add, remove, up, down, budget });
            mappingLayout.Controls.Add(grid, 0, 0);
            mappingLayout.Controls.Add(actions, 0, 1);
            mappings.Controls.Add(mappingLayout);
            var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
            buttons.Controls.AddRange(new Control[] { close, revert, save });
            root.Controls.Add(top, 0, 0);
            root.Controls.Add(communication, 0, 1);
            root.Controls.Add(mappings, 0, 2);
            root.Controls.Add(status, 0, 3);
            root.Controls.Add(buttons, 0, 4);
            Controls.Add(root);
            AcceptButton = save;
            CancelButton = close;
        }

        private async void SelectionChanged(object sender, EventArgs args)
        {
            if (rendering || selector.SelectedItem == null)
            {
                return;
            }
            var next = (PdoSelection)selector.SelectedItem;
            if (!DiscardDraft())
            {
                rendering = true;
                selector.SelectedItem = selected;
                rendering = false;
                return;
            }
            selected = next;
            snapshot = null;
            await LoadSelection();
        }

        private async Task LoadSelection()
        {
            if (selected == null || busy)
            {
                return;
            }
            SetBusy(true);
            status.Text = "Reading communication parameters and current mapping...";
            try
            {
                snapshot = await device.ReadAsync(selected);
                RenderSnapshot();
                status.Text = "Device values loaded. Revert discards draft edits; Read device obtains fresh values.";
            }
            catch (Exception error)
            {
                snapshot = null;
                parameters.Controls.Clear();
                editors.Clear();
                draftMappings.Clear();
                grid.Rows.Clear();
                status.Text = "Read failed: " + error.Message;
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void RenderSnapshot()
        {
            if (snapshot == null)
            {
                return;
            }
            rendering = true;
            parameters.SuspendLayout();
            try
            {
                foreach (Control control in parameters.Controls.Cast<Control>().ToArray())
                {
                    control.Dispose();
                }
                parameters.Controls.Clear();
                parameters.RowStyles.Clear();
                parameters.RowCount = 1;
                editors.Clear();
                string[] headers = { "Parameter", "Current device", "Draft", "Units / access" };
                for (int column = 0; column < headers.Length; column++)
                {
                    parameters.Controls.Add(new Label { Text = headers[column], AutoSize = true, Margin = new Padding(3, 5, 3, 8) }, column, 0);
                }
                int row = 1;
                foreach (byte subindex in snapshot.Communication.Keys.Concat(snapshot.Unavailable.Keys).OrderBy(value => value))
                {
                    ODentry metadata = selected.Record.Getsubobject(subindex);
                    string name = metadata == null ? "Parameter" : metadata.parameter_name;
                    parameters.RowCount = row + 1;
                    parameters.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                    parameters.Controls.Add(new Label { Text = string.Format("0x{0:X4}:{1:X2} — {2}", selected.Index, subindex, name), AutoSize = true,
                        Anchor = AnchorStyles.Left }, 0, row);
                    byte[] data;
                    bool available = snapshot.Communication.TryGetValue(subindex, out data);
                    bool numeric = available && (data.Length == 1 || data.Length == 2 || data.Length == 4);
                    string value = !available ? snapshot.Unavailable[subindex] : numeric ? PdoEditorModel.Format(subindex, data)
                        : "0x" + BitConverter.ToString(data).Replace("-", "");
                    var current = new TextBox { Text = value, ReadOnly = true, Dock = DockStyle.Fill };
                    bool writable = numeric && PdoEditorModel.CanWrite(selected, subindex);
                    var draft = new TextBox { Text = value, ReadOnly = !writable, Dock = DockStyle.Fill,
                        AccessibleName = string.Format("Draft {0:X4}:{1:X2}", selected.Index, subindex) };
                    parameters.Controls.Add(current, 1, row);
                    parameters.Controls.Add(draft, 2, row);
                    string units = subindex == 3 ? "100 µs (200 = 20 ms)" : subindex == 5 ? "ms" : subindex == 1 ? "COB-ID + flags" : "";
                    parameters.Controls.Add(new Label { Text = writable ? units : "Read-only / unavailable", AutoSize = true, Anchor = AnchorStyles.Left }, 3, row);
                    if (writable)
                    {
                        editors.Add(subindex, draft);
                    }
                    row++;
                }
                draftMappings = snapshot.Mappings.ToList();
                options = PdoEditorModel.Options(dictionary, selected.Transmit);
                mappingWritable = snapshot.MappingError == null && PdoEditorModel.CanMap(dictionary, selected);
                RenderMappings();
            }
            finally
            {
                rendering = false;
                parameters.ResumeLayout(true);
            }
        }

        private void RenderMappings()
        {
            bool previous = rendering;
            rendering = true;
            grid.Rows.Clear();
            for (int position = 0; position < draftMappings.Count; position++)
            {
                uint descriptor = draftMappings[position];
                int row = grid.Rows.Add();
                var choices = new List<PdoMappingOption>(options);
                if (!choices.Any(option => option.Descriptor == descriptor))
                {
                    choices.Add(new PdoMappingOption(descriptor, PdoEditorModel.Decode(dictionary, descriptor) + " (existing)"));
                }
                var target = (DataGridViewComboBoxCell)grid.Rows[row].Cells["target"];
                target.DataSource = choices;
                target.Value = descriptor;
                grid.Rows[row].Cells["slot"].Value = position + 1;
                grid.Rows[row].Cells["current"].Value = position < snapshot.Mappings.Count
                    ? "0x" + snapshot.Mappings[position].ToString("X8") + " — " + PdoEditorModel.Decode(dictionary, snapshot.Mappings[position]) : "New slot";
                grid.Rows[row].Cells["current"].ToolTipText = Convert.ToString(grid.Rows[row].Cells["current"].Value);
                grid.Rows[row].Cells["hex"].Value = "0x" + descriptor.ToString("X8");
                grid.Rows[row].Cells["bits"].Value = (byte)descriptor;
            }
            rendering = previous;
            UpdateBudget();
        }

        private void MappingChanged(object sender, DataGridViewCellEventArgs args)
        {
            if (rendering || args.RowIndex < 0 || args.ColumnIndex != grid.Columns["target"].Index)
            {
                return;
            }
            object value = grid.Rows[args.RowIndex].Cells["target"].Value;
            if (value == null)
            {
                return;
            }
            uint descriptor = (uint)value;
            draftMappings[args.RowIndex] = descriptor;
            grid.Rows[args.RowIndex].Cells["hex"].Value = "0x" + descriptor.ToString("X8");
            grid.Rows[args.RowIndex].Cells["bits"].Value = (byte)descriptor;
            UpdateBudget();
        }

        private void MoveMapping(int delta)
        {
            if (grid.CurrentRow == null)
            {
                return;
            }
            int position = grid.CurrentRow.Index;
            int destination = position + delta;
            if (destination < 0 || destination >= draftMappings.Count)
            {
                return;
            }
            uint value = draftMappings[position];
            draftMappings[position] = draftMappings[destination];
            draftMappings[destination] = value;
            RenderMappings();
            grid.CurrentCell = grid.Rows[destination].Cells["target"];
        }

        private void UpdateBudget()
        {
            int bits = draftMappings.Sum(value => (byte)value);
            budget.Text = snapshot == null ? "Select a PDO to read its mapping." : string.Format("0x{0:X4}: current {1}, draft {2} objects; {3}/64 bits{4}",
                selected.MappingIndex, snapshot.Mappings.Count, draftMappings.Count, bits,
                snapshot != null && snapshot.MappingError != null ? " — mapping unavailable" : !mappingWritable ? " — fixed mapping" : "");
            budget.ForeColor = bits > 64 ? Color.DarkRed : SystemColors.ControlText;
            add.Enabled = !busy && mappingWritable && options.Count != 0 && draftMappings.Count < 64;
        }

        private bool IsDirty()
        {
            if (snapshot == null)
            {
                return false;
            }
            if (!snapshot.Mappings.SequenceEqual(draftMappings))
            {
                return true;
            }
            try
            {
                return editors.Any(pair => !PdoEditorModel.Parse(pair.Value.Text, snapshot.Communication[pair.Key].Length)
                    .SequenceEqual(snapshot.Communication[pair.Key]));
            }
            catch (FormatException)
            {
                return true;
            }
        }

        private bool DiscardDraft()
        {
            return !IsDirty() || MessageBox.Show(this, "Discard unsaved changes to this PDO?", "Unsaved PDO edits",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        private async Task Save()
        {
            if (busy || snapshot == null)
            {
                return;
            }
            grid.EndEdit();
            SetBusy(true);
            try
            {
                SdoParameterWrite[] writes = PdoEditorModel.BuildWrites(selected, snapshot,
                    editors.ToDictionary(pair => pair.Key, pair => pair.Value.Text), draftMappings, options, mappingWritable);
                if (writes.Length == 0)
                {
                    status.Text = "No changes to save.";
                    return;
                }
                status.Text = "Checking current device values before saving...";
                PdoConfigurationSnapshot fresh = await device.ReadAsync(selected);
                if (!fresh.Mappings.SequenceEqual(snapshot.Mappings) || snapshot.Communication.Any(pair =>
                    !fresh.Communication.ContainsKey(pair.Key) || !pair.Value.SequenceEqual(fresh.Communication[pair.Key])))
                {
                    throw new InvalidOperationException("Device values changed since the last read. Use Read device to refresh before editing again.");
                }
                status.Text = "Saving through the PDO configuration handshake...";
                PdoConfigurationResult result = await device.SaveAsync(writes);
                snapshot = await device.ReadAsync(selected);
                RenderSnapshot();
                status.Text = result.NmtStateChanged && !result.NmtStateConfirmed
                    ? "Saved and read back. Operational NMT sent; heartbeat disabled, NMT state unconfirmed. Not stored to nonvolatile memory."
                    : "Saved and read back. Not stored to nonvolatile memory.";
            }
            catch (Exception error)
            {
                // Keep the draft after a failure. The user may need it to
                // recover a partially applied configuration; never auto-retry.
                status.Text = "Save stopped: " + error.Message;
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void SetBusy(bool value)
        {
            busy = value;
            selector.Enabled = !value;
            reload.Enabled = !value && selected != null;
            save.Enabled = !value && snapshot != null;
            revert.Enabled = !value && snapshot != null;
            close.Enabled = !value;
            parameters.Enabled = !value;
            grid.Enabled = !value && snapshot != null;
            grid.Columns["target"].ReadOnly = !mappingWritable;
            remove.Enabled = up.Enabled = down.Enabled = !value && mappingWritable;
            UpdateBudget();
        }
    }
}
