using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows.Forms;

namespace FixedDataUi
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FixedDataForm());
        }
    }

    internal sealed class FixedDataForm : Form
    {
        private readonly DataGridView fieldGrid;
        private readonly ComboBox alignmentComboBox;
        private readonly TextBox resultTextBox;
        private readonly Label byteCountLabel;
        private readonly Label statusLabel;
        private readonly string automaticStatePath;

        public FixedDataForm()
        {
            Text = "fixed 데이터 생성기";
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(760, 560);
            Size = new Size(900, 650);
            Font = new Font("맑은 고딕", 9F);
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            fieldGrid = CreateFieldGrid();
            alignmentComboBox = CreateAlignmentComboBox();
            resultTextBox = CreateResultTextBox();
            byteCountLabel = CreateByteCountLabel();
            statusLabel = new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            automaticStatePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "FixedDataUi",
                "last-input.json");

            Controls.Add(CreateMainLayout());
            Controls.Add(CreateFooterPanel());

            FormClosing += SaveLastInput;
            LoadInitialState();
        }

        private Control CreateMainLayout()
        {
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Padding = new Padding(12)
            };

            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));

            root.Controls.Add(CreateButtonPanel(), 0, 0);
            root.Controls.Add(fieldGrid, 0, 1);
            root.Controls.Add(CreateGeneratePanel(), 0, 2);
            root.Controls.Add(CreateResultHeader(), 0, 3);
            root.Controls.Add(resultTextBox, 0, 4);

            return root;
        }

        private Control CreateFooterPanel()
        {
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Bottom,
                ColumnCount = 2,
                RowCount = 1,
                Height = 24,
                Padding = new Padding(12, 0, 12, 0)
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));

            panel.Controls.Add(statusLabel, 0, 0);
            panel.Controls.Add(new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                Text = "made by jngsngjn",
                TextAlign = ContentAlignment.MiddleRight
            }, 1, 0);

            return panel;
        }

        private DataGridView CreateFieldGrid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = SystemColors.Window,
                BorderStyle = BorderStyle.FixedSingle,
                MultiSelect = true,
                RowHeadersWidth = 48,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "length",
                Name = "lengthColumn",
                FillWeight = 20F
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "value",
                Name = "valueColumn",
                FillWeight = 80F
            });

            grid.RowPostPaint += PaintRowNumber;
            return grid;
        }

        private TextBox CreateResultTextBox()
        {
            return new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Both,
                WordWrap = false,
                Font = new Font("Consolas", 10F)
            };
        }

        private Control CreateButtonPanel()
        {
            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 0, 0, 8)
            };

            panel.Controls.Add(CreateButton("행 추가", AddRow));
            panel.Controls.Add(CreateButton("선택 행 삭제", DeleteSelectedRows));
            panel.Controls.Add(CreateButton("전체 삭제", ClearRows));
            panel.Controls.Add(CreateButton("내보내기", ExportState));
            panel.Controls.Add(CreateButton("임포트", ImportState));
            panel.Controls.Add(CreatePaddingLabel());
            panel.Controls.Add(alignmentComboBox);

            return panel;
        }

        private static Control CreatePaddingLabel()
        {
            return new Label
            {
                AutoSize = true,
                Margin = new Padding(10, 6, 4, 0),
                Text = "여백 방향"
            };
        }

        private static ComboBox CreateAlignmentComboBox()
        {
            var comboBox = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 120,
                Margin = new Padding(0, 1, 8, 0)
            };
            comboBox.Items.Add("왼쪽 붙임");
            comboBox.Items.Add("오른쪽 붙임");
            comboBox.SelectedIndex = 0;
            return comboBox;
        }

        private Control CreateGeneratePanel()
        {
            var panel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                FlowDirection = FlowDirection.LeftToRight,
                Padding = new Padding(0, 10, 0, 10)
            };

            panel.Controls.Add(CreateButton("생성", GenerateResult));
            panel.Controls.Add(CreateButton("복사", CopyResult));

            return panel;
        }

        private Control CreateResultHeader()
        {
            var panel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Height = 24
            };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            panel.Controls.Add(new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                Text = "결과",
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);
            panel.Controls.Add(byteCountLabel, 1, 0);

            return panel;
        }

        private static Label CreateByteCountLabel()
        {
            return new Label
            {
                AutoSize = false,
                Dock = DockStyle.Fill,
                Text = "바이트: 0",
                TextAlign = ContentAlignment.MiddleRight
            };
        }

        private static Button CreateButton(string text, EventHandler handler)
        {
            var button = new Button
            {
                Text = text,
                AutoSize = true,
                MinimumSize = new Size(92, 30),
                Margin = new Padding(0, 0, 8, 0)
            };
            button.Click += handler;
            return button;
        }

        private void AddSampleRows()
        {
            fieldGrid.Rows.Add("10", "ABC");
            fieldGrid.Rows.Add("5", "ABC");
            statusLabel.Text = "기본 예시가 입력되었습니다.";
        }

        private void LoadInitialState()
        {
            if (!File.Exists(automaticStatePath))
            {
                AddSampleRows();
                return;
            }

            try
            {
                ApplyState(FixedDataStateFile.Load(automaticStatePath));
                statusLabel.Text = "마지막 입력값을 복원했습니다.";
            }
            catch (Exception ex)
            {
                AddSampleRows();
                statusLabel.Text = "마지막 입력값을 복원하지 못했습니다: " + ex.Message;
            }
        }

        private void SaveLastInput(object sender, FormClosingEventArgs e)
        {
            try
            {
                fieldGrid.EndEdit();
                FixedDataStateFile.Save(automaticStatePath, CaptureState());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "마지막 입력값을 저장하지 못했습니다.\r\n" + ex.Message,
                    "자동 저장 오류",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }

        private void ExportState(object sender, EventArgs e)
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "JSON 파일 (*.json)|*.json|모든 파일 (*.*)|*.*";
                dialog.DefaultExt = "json";
                dialog.AddExtension = true;
                dialog.FileName = "fixed-data.json";

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    fieldGrid.EndEdit();
                    FixedDataStateFile.Save(dialog.FileName, CaptureState());
                    statusLabel.Text = "입력값을 JSON 파일로 내보냈습니다.";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        this,
                        "입력값을 내보내지 못했습니다.\r\n" + ex.Message,
                        "내보내기 오류",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void ImportState(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog())
            {
                dialog.Filter = "JSON 파일 (*.json)|*.json|모든 파일 (*.*)|*.*";
                dialog.DefaultExt = "json";
                dialog.CheckFileExists = true;

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                try
                {
                    FixedDataState state = FixedDataStateFile.Load(dialog.FileName);
                    ApplyState(state);
                    statusLabel.Text = "JSON 파일에서 입력값을 임포트했습니다.";
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        this,
                        "입력값을 임포트하지 못했습니다.\r\n" + ex.Message,
                        "임포트 오류",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private FixedDataState CaptureState()
        {
            var state = new FixedDataState
            {
                Version = FixedDataStateFile.CurrentVersion,
                Alignment = alignmentComboBox.SelectedIndex == 1 ? "right" : "left",
                Rows = new List<FixedDataRow>()
            };

            for (int i = 0; i < fieldGrid.Rows.Count; i++)
            {
                DataGridViewRow row = fieldGrid.Rows[i];
                if (row.IsNewRow)
                {
                    continue;
                }

                state.Rows.Add(new FixedDataRow
                {
                    Length = Convert.ToString(row.Cells["lengthColumn"].Value, CultureInfo.InvariantCulture) ?? string.Empty,
                    Value = Convert.ToString(row.Cells["valueColumn"].Value, CultureInfo.InvariantCulture) ?? string.Empty
                });
            }

            return state;
        }

        private void ApplyState(FixedDataState state)
        {
            fieldGrid.Rows.Clear();
            for (int i = 0; i < state.Rows.Count; i++)
            {
                FixedDataRow row = state.Rows[i];
                fieldGrid.Rows.Add(row.Length, row.Value);
            }

            alignmentComboBox.SelectedIndex = state.Alignment == "right" ? 1 : 0;
            ClearResult();
        }

        private void ClearResult()
        {
            resultTextBox.Clear();
            byteCountLabel.Text = "바이트: 0";
        }

        private void AddRow(object sender, EventArgs e)
        {
            int rowIndex = fieldGrid.Rows.Add("1", string.Empty);
            fieldGrid.CurrentCell = fieldGrid.Rows[rowIndex].Cells[0];
            fieldGrid.BeginEdit(true);
            statusLabel.Text = "행을 추가했습니다.";
        }

        private void DeleteSelectedRows(object sender, EventArgs e)
        {
            if (fieldGrid.Rows.Count == 0)
            {
                statusLabel.Text = "삭제할 행이 없습니다.";
                return;
            }

            if (fieldGrid.SelectedRows.Count > 0)
            {
                int[] indexes = new int[fieldGrid.SelectedRows.Count];
                for (int i = 0; i < fieldGrid.SelectedRows.Count; i++)
                {
                    indexes[i] = fieldGrid.SelectedRows[i].Index;
                }

                Array.Sort(indexes);

                for (int i = indexes.Length - 1; i >= 0; i--)
                {
                    int rowIndex = indexes[i];
                    if (rowIndex >= 0 && rowIndex < fieldGrid.Rows.Count && !fieldGrid.Rows[rowIndex].IsNewRow)
                    {
                        fieldGrid.Rows.RemoveAt(rowIndex);
                    }
                }
            }
            else if (fieldGrid.CurrentRow != null && !fieldGrid.CurrentRow.IsNewRow)
            {
                fieldGrid.Rows.Remove(fieldGrid.CurrentRow);
            }

            statusLabel.Text = "선택한 행을 삭제했습니다.";
        }

        private void ClearRows(object sender, EventArgs e)
        {
            fieldGrid.Rows.Clear();
            ClearResult();
            statusLabel.Text = "전체 행과 결과를 삭제했습니다.";
        }

        private void GenerateResult(object sender, EventArgs e)
        {
            fieldGrid.EndEdit();

            string message;
            string result;
            bool padLeft = alignmentComboBox.SelectedIndex == 1;
            if (!TryCreateFixedData(fieldGrid, padLeft, out result, out message))
            {
                MessageBox.Show(this, message, "입력 오류", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                statusLabel.Text = message;
                return;
            }

            resultTextBox.Text = result;
            byteCountLabel.Text = "바이트: " + Encoding.UTF8.GetByteCount(result).ToString(CultureInfo.InvariantCulture);
            statusLabel.Text = "fixed 데이터를 생성했습니다.";
        }

        private void CopyResult(object sender, EventArgs e)
        {
            if (resultTextBox.TextLength == 0)
            {
                statusLabel.Text = "복사할 결과가 없습니다.";
                return;
            }

            Clipboard.SetText(resultTextBox.Text);
            statusLabel.Text = "결과를 클립보드에 복사했습니다.";
        }

        private static bool TryCreateFixedData(DataGridView grid, bool padLeft, out string result, out string message)
        {
            var builder = new StringBuilder();

            for (int i = 0; i < grid.Rows.Count; i++)
            {
                DataGridViewRow row = grid.Rows[i];
                if (row.IsNewRow)
                {
                    continue;
                }

                int rowNumber = i + 1;
                string lengthText = Convert.ToString(row.Cells["lengthColumn"].Value, CultureInfo.InvariantCulture);
                string value = Convert.ToString(row.Cells["valueColumn"].Value, CultureInfo.InvariantCulture) ?? string.Empty;

                int length;
                if (!int.TryParse(lengthText, NumberStyles.Integer, CultureInfo.InvariantCulture, out length) || length < 1)
                {
                    result = string.Empty;
                    message = rowNumber.ToString(CultureInfo.InvariantCulture) + "행 length는 1 이상의 정수여야 합니다.";
                    return false;
                }

                if (value.Length > length)
                {
                    result = string.Empty;
                    message = rowNumber.ToString(CultureInfo.InvariantCulture) + "행 value 길이는 length보다 클 수 없습니다.";
                    return false;
                }

                int paddingLength = length - value.Length;
                if (padLeft)
                {
                    builder.Append(' ', paddingLength);
                    builder.Append(value);
                }
                else
                {
                    builder.Append(value);
                    builder.Append(' ', paddingLength);
                }
            }

            result = builder.ToString();
            message = string.Empty;
            return true;
        }

        private static void PaintRowNumber(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            var grid = (DataGridView)sender;
            string rowNumber = (e.RowIndex + 1).ToString(CultureInfo.InvariantCulture);
            using (var brush = new SolidBrush(grid.RowHeadersDefaultCellStyle.ForeColor))
            {
                e.Graphics.DrawString(
                    rowNumber,
                    grid.Font,
                    brush,
                    e.RowBounds.Left + 14,
                    e.RowBounds.Top + 4);
            }
        }
    }

    [DataContract]
    internal sealed class FixedDataState
    {
        [DataMember(Name = "version", Order = 1, IsRequired = true)]
        public int Version { get; set; }

        [DataMember(Name = "alignment", Order = 2, IsRequired = true)]
        public string Alignment { get; set; }

        [DataMember(Name = "rows", Order = 3, IsRequired = true)]
        public List<FixedDataRow> Rows { get; set; }
    }

    [DataContract]
    internal sealed class FixedDataRow
    {
        [DataMember(Name = "length", Order = 1, IsRequired = true)]
        public string Length { get; set; }

        [DataMember(Name = "value", Order = 2, IsRequired = true)]
        public string Value { get; set; }
    }

    internal static class FixedDataStateFile
    {
        public const int CurrentVersion = 1;

        public static FixedDataState Load(string path)
        {
            var serializer = new DataContractJsonSerializer(typeof(FixedDataState));
            FixedDataState state;
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                state = serializer.ReadObject(stream) as FixedDataState;
            }

            Validate(state);
            return state;
        }

        public static void Save(string path, FixedDataState state)
        {
            Validate(state);

            string directoryPath = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var serializer = new DataContractJsonSerializer(typeof(FixedDataState));
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                serializer.WriteObject(stream, state);
            }
        }

        private static void Validate(FixedDataState state)
        {
            if (state == null)
            {
                throw new InvalidDataException("JSON 상태가 비어 있습니다.");
            }

            if (state.Version != CurrentVersion)
            {
                throw new InvalidDataException("지원하지 않는 JSON 버전입니다.");
            }

            if (state.Alignment != "left" && state.Alignment != "right")
            {
                throw new InvalidDataException("alignment는 left 또는 right여야 합니다.");
            }

            if (state.Rows == null)
            {
                throw new InvalidDataException("rows 필드가 필요합니다.");
            }

            for (int i = 0; i < state.Rows.Count; i++)
            {
                FixedDataRow row = state.Rows[i];
                if (row == null || row.Length == null || row.Value == null)
                {
                    throw new InvalidDataException(
                        (i + 1).ToString(CultureInfo.InvariantCulture) + "행의 length와 value가 필요합니다.");
                }
            }
        }
    }
}
