using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace GUIComponents
{
    public class HexUpDown : NumericUpDown
    {
        public HexUpDown()
        {
            this.Hexadecimal = true;
        }
        protected override void ValidateEditText()
        {
            try
            {
                var txt = this.Text;
                if (!string.IsNullOrEmpty(txt))
                {
                    if (txt.StartsWith("0x")) txt = txt.Substring(2);
                    var value = Convert.ToDecimal(Convert.ToInt32(txt, 16));
                    value = Math.Max(value, this.Minimum);
                    value = Math.Min(value, this.Maximum);
                    this.Value = value;
                }
            }
            catch { }
            base.UserEdit = false;
            UpdateEditText();
        }

        protected override void UpdateEditText()
        {
            int value = Convert.ToInt32(this.Value);
            this.Text = "0x" + value.ToString("X4");
        }
    }
}
