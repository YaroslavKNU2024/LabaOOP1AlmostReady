﻿using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LabaOOP1
{
    public partial class Form1 : Form
    {

        private int _row = 10;
        private int _col = 10;
        private bool _showExpression = false;
        private List<CellElement> _cells = new List<CellElement>();

        public Form1()
        {
            InitializeComponent();
            WindowState = FormWindowState.Maximized;
            InitializeDataGridView();
        }
        private void InitializeDataGridView()
        {
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.ColumnCount = _col;
            dataGridView1.RowCount = _row;
            FillHeaders();
            dataGridView1.AutoResizeRows();
            dataGridView1.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dataGridView1.RowHeadersWidth = 100;
        }

        private void FillHeaders()
        {
            foreach (DataGridViewColumn col in dataGridView1.Columns)
            {
                col.HeaderText = "C" + (col.Index + 1);
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                row.HeaderCell.Value = "R" + (row.Index + 1);
            }
        }

        // get values from other cells
        private string ResolveAddresses(string s)
        {
            string x = "";
            int i = 0, len = s.Length;
            while (i < len)
            {
                if (s[i] == '@')
                {
                    Tuple<int, int> t = getIndecesFromName(s.Substring(i + 1, len - i - 1));
                    x += ResolveAddresses(dataGridView1.Rows[t.Item1].Cells[t.Item2].Value.ToString());
                    i += 3 + t.Item1.ToString().Length + t.Item2.ToString().Length;
                }
                else
                {
                    x += s[i];
                    ++i;
                }
            }
            return x;
        }

        // get indeces from the name of a cell (ex. R2C1)
        private Tuple<int, int> getIndecesFromName(string s)
        {
            string slen = "";
            foreach (char n in s)
                if (n <= '9' && n >= '0')
                    slen += n;
                else if (n == 'C')
                    break;
            int r = Int32.Parse(slen);
            slen = "";
            bool col = false;
            foreach (char n in s)
                if (n == 'C') col = true;
                else if (col)
                {
                    if (n <= '9' && n >= '0')
                        slen += n;
                    else break;
                }
            int c = Int32.Parse(slen);
            return new Tuple<int, int>(r, c);
        }

        // updating values in cells
        private void updateAllCells()
        {
            for (int i = 0; i < _cells.Count; ++i)
            {
                string expr = _cells[i].expression;
                string valueOfCell = ResolveAddresses(expr);
                _cells[i].value = valueOfCell;
            }
            for (int k = 0; k < 2; ++k)
            {
                for (int i = 0; i < _cells.Count; ++i)
                {
                    Tuple<int, int> t = getIndecesFromName(_cells[i].name);
                    Calculator calc = new Calculator();
                    var res = calc.Evaluate(_cells[i].value.ToString());
                    dataGridView1.Rows[t.Item1].Cells[t.Item2].Value = res;
                }
            }
        }
        // hide values & show expressions
        private void ShowExpressions()
        {
            for (int i = 0; i < _cells.Count; ++i)
            {
                Tuple<int, int> t = getIndecesFromName(_cells[i].name);
                dataGridView1.Rows[t.Item1].Cells[t.Item2].Value = _cells[i].expression;
            }
        }
        
        // function to check correctness
        HashSet<char> _operators = new HashSet<char>() { '+', '-', '*', '/', '^', 'm', 'a', 'x', 'p', 'o', 'w' };
        HashSet<char> _digits = new HashSet<char>() { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
        private void checkCorrect(string expr)
        {
            int lparen = 0, rparen = 0;
            bool parentopen = false;
            for (int i = 0, len = expr.Length; i < len; ++i)
            {
                if (expr[i] == '(')
                {
                    lparen++;
                    parentopen = true;
                } else if (expr[i] == ')')
                {
                    rparen++;
                    parentopen = false;
                    if (lparen < rparen)
                        throw new ArithmeticException();
                } else if (_operators.Contains(expr[i]))
                {
                    if (parentopen)
                        throw new ArithmeticException();
                } else if (_digits.Contains(expr[i]))
                {
                    if (i != 0 && i != len-1)
                    {
                        if (expr[i+1] == '(' || expr[i - 1] == ')')
                            throw new ArithmeticException();
                    } else if (i == 0)
                    {
                        if (len > 1 && expr[1] == '(')
                            throw new ArithmeticException();
                    } else {
                        if (len > 1 && expr[len-1] == ')')
                            throw new ArithmeticException();
                    }
                }
            }
            if (lparen != rparen)
                throw new ArithmeticException();
        }

        // change a selected cell
        private void ApplyButton_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentCell == null) return;

            int r = dataGridView1.CurrentCell.RowIndex;
            int c = dataGridView1.CurrentCell.ColumnIndex;
            if (dataGridView1.Rows[r].Cells[c].Value == null)
            {
                if (textBox1.Text == string.Empty)
                    return;
                dataGridView1.Rows[r].Cells[c].Value = textBox1.Text;
                textBox1.Text = string.Empty;
            }
            CellElement Cell = new CellElement(r, c, dataGridView1.Rows[r].Cells[c].Value.ToString(), "to be implemented");

            string expr = dataGridView1.Rows[r].Cells[c].Value.ToString();
            checkCorrect(expr);
            string valueOfCell = ResolveAddresses(expr);
            Cell.value = valueOfCell;

            bool found = false;
            for (int i = 0, len = _cells.Count; i < len; ++i)
            {
                if (_cells[i].name == Cell.name)
                {
                    _cells[i] = Cell;
                    found = true;
                    break;
                }
            }
            if (!found)
                _cells.Add(Cell);
            var test = _cells.FirstOrDefault(c => c.name == Cell.name);
            _cells.FirstOrDefault(c => c.name == Cell.name);
            dataGridView1.Rows[r].Cells[c].Value = Cell.value.ToString();
            updateAllCells();
        }

        // add row / column
        private void addRowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _row++;
            dataGridView1.RowCount = _row;
            FillHeaders();
        }

        private void addColumnToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _col++;
            dataGridView1.ColumnCount = _col;
            FillHeaders();
        }


        private void showExpressionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _showExpression = true;
            ShowExpressions();
        }

        private void showValuesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _showExpression = false;
            updateAllCells();
        }
        
        private void MakeNull()
        {
            for (int i = 0; i < _row; ++i)
                for (int j = 0; j < _col; ++j)
                    dataGridView1.Rows[i].Cells[j].Value = "";
        }
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MakeNull();
            using (var reader = new StreamReader(@"C:\Users\yasma\source\repos\LabaOOP1\test.csv"))
            {
                List<string> listIndexRow = new List<string>();
                List<string> listIndexCol = new List<string>();
                List<string> listIndexExpression = new List<string>();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    var values = line.Split(':');

                    listIndexRow.Add(values[0]);
                    listIndexCol.Add(values[1]);
                    listIndexExpression.Add(values[2]);
                    int rl = Int32.Parse(values[0]), cl = Int32.Parse(values[1]);
                    if (rl > _row)
                    {
                        for (; rl - _row > 0;) {
                            _row++;
                            dataGridView1.RowCount = _row;
                            FillHeaders();
                        }
                    }
                    if (cl > _col)
                    {
                        for (; cl - _col > 0;) {
                            _col++;
                            dataGridView1.ColumnCount = _col;
                            FillHeaders();
                        }
                    }
                    Calculator calc = new Calculator();
                    values[2] = ResolveAddresses(values[2]);
                    var res = calc.Evaluate(values[2]);
                    dataGridView1.Rows[rl].Cells[cl].Value = res;
                    updateAllCells();
                }
            }
            updateAllCells();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (var w = new StreamWriter(@"C:\Users\yasma\source\repos\LabaOOP1\test.csv"))
            {
                foreach (CellElement el in _cells)
                {
                    var line = string.Format("{0 }:{1}:{2}\t", el.indRow, el.indCol, el.value) ;
                    w.WriteLine(line);
                    w.Flush();
                }
            }
            //DataTable dt = GetDataTableFromDGV(dataGridView1);
            //DataSet ds = new DataSet();
            //ds.Tables.Add(dt);
            //ds.WriteXml(File.OpenWrite(@"C:\Users\yasma\source\repos\LabaOOP1\logfile.xlsx"));
        }
    }
}
