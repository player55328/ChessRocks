using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ns_ini;

namespace ChessRocks
{
  public partial class getHeader : Form
  {
    public ns_ini.ini chess;

    public DialogResult result = DialogResult.Cancel;
    public string headerString;

    public getHeader(ns_ini.ini Chess, string CurrentHeader)
    {
      chess = Chess;
      
      InitializeComponent();

      header.Text = CurrentHeader;
      headerString = CurrentHeader;
    }

    private void header_TextChanged(object sender, EventArgs e)
    {
      warning.Visible = headerInUse();
    }

    private bool headerInUse()
    {
      //check to see if this header is in use by loading the first option...
      string option = "";
      chess.GetINIValue(header.Text, "OPTION_1", ref option, "not_in_use", true);
      return !option.Equals("not_in_use");
    }

    private void ok_Click(object sender, EventArgs e)
    {
      headerString = header.Text;
      result = DialogResult.OK;
      Dispose(true);
    }

    private void cancel_Click(object sender, EventArgs e)
    {
      Dispose(true);
    }
  }
}
