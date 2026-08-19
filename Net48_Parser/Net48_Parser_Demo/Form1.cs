using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Net48_Parser_Demo
{
    public partial class Form1 : Form
    {
        string m_MassKette = "(3*-4,25)+(1+-4*2)";
        Net48_Parser.NumericParser m_Parser = new Net48_Parser.NumericParser(true, 0);

        public Form1()
        {
            InitializeComponent();
            textBoxInput.Text = m_MassKette;
            //UpdateResult();
        }

        void UpdateResult()
        {
            string Formula = this.textBoxInput.Text;
            m_Parser.execute(Formula);
            this.textBoxResult.Text = m_Parser.Result.ToString();
            this.textBoxMessage.Text = m_Parser.ErrorMessage;
        }

        private void textBoxInput_TextChanged(object sender, EventArgs e)
        {
            UpdateResult();
        }
    }
}
