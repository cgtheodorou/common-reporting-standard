using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using CRS.Implementation;
using CRS.Implementation.Common;
using System.Collections.Generic;
using CRS.Implementation.OECD;
using System.Text;

namespace CRS
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

        private void btnSelect_Click(object sender, EventArgs e)
        {
            fileDlg.ShowDialog();
            txtInputFileName.Text = fileDlg.FileName;
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            try
            {
                ValidateInput();

                txtMsg.Text = "";
                btnProcess.Enabled = false;

                var engine = TransformationEngineFactory.Create(TransformationEngineTypeEnum.OECD);
                IList<TransformationMessage> Messages = engine.TransformToXml(fileDlg.FileName);

                if (Messages.Count > 0)
                {
                    var sb = new StringBuilder();
                    foreach (var item in Messages)
                    {
                        sb.AppendLine(item.InvalidValue);
                        sb.AppendLine(item.Level.ToString());
                        sb.AppendLine(item.XPath?.ToString());
                        sb.AppendLine(item.Data?.ToString());
                        sb.AppendLine("");
                    }
                    txtMsg.Text = sb.ToString();
                }

                btnProcess.Enabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                btnProcess.Enabled = true;
            }
        }

        private void ValidateInput()
        {
            if (string.IsNullOrEmpty(txtInputFileName.Text.Trim()))
            {
                throw new Exception("Invalid input file");
            }
        }
    }
}
