using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ChessRocks
{
  public partial class About : Form
  {
    public About()
    {
      InitializeComponent();
    }

    private void ok_Click(object sender, EventArgs e)
    {
      Dispose(true);
    }
  }
}
