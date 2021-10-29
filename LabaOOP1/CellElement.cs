using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LabaOOP1
{
    public class CellElement
    {
        public int indRow;
        public int indCol;
        public string name;
        public string expression;
        public string value;

        public CellElement() { }
        public CellElement(int row, int col, string expression, string value)
        {
            indRow = row;
            indCol = col;
            name = "R" + indRow.ToString() + "C" + indCol.ToString();
            this.expression = expression;
            this.value = value;
        }

    }
}
