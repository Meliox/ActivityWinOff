using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace ActivityWinOff
{
    public partial class Arguments : Form
    {
        public string ReturnValue { get; set; }

        public Arguments(string argument)
        {
            InitializeComponent();
            ArgumenttextBox.Text = argument;
        }

        private void OKbutton_Click(object sender, EventArgs e)
        {
            this.ReturnValue = ArgumenttextBox.Text;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Cancelbutton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
