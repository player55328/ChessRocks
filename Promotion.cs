using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ChessRocks;

namespace ns_Promotion
{
  public partial class Promotion : Form
  {
    public string PromotedTo = "";

    public Promotion(ChessRocks.ChessRocks parent, bool whitesMove)
    {
      InitializeComponent();

      if (whitesMove)
      {
        promoteQueen.BackgroundImage = parent.wQueen;
        promoteRook.BackgroundImage = parent.wRook;
        promoteKnight.BackgroundImage = parent.wKnight;
        promoteBishop.BackgroundImage = parent.wBishop;
      }
      else
      {
        promoteQueen.BackgroundImage = parent.kQueen;
        promoteRook.BackgroundImage = parent.kRook;
        promoteKnight.BackgroundImage = parent.kKnight;
        promoteBishop.BackgroundImage = parent.kBishop;
      }
    }

    private void promoteQueen_Click(object sender, EventArgs e)
    {
      PromotedTo = "q";
      DialogResult = DialogResult.OK;
    }

    private void promoteRook_Click(object sender, EventArgs e)
    {
      PromotedTo = "r";
      DialogResult = DialogResult.OK;
    }

    private void promoteKnight_Click(object sender, EventArgs e)
    {
      PromotedTo = "n";
      DialogResult = DialogResult.OK;
    }

    private void promoteBishop_Click(object sender, EventArgs e)
    {
      PromotedTo = "b";
      DialogResult = DialogResult.OK;
    }
  }
}
