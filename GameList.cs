using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ChessRocks
{
  public partial class GameList : Form
  {
    bool row0found = false;
    public bool loadAll = false;

    public GameList()
    {
      InitializeComponent();
    }

    private void clear_Click(object sender, EventArgs e)
    {
      gameListDataGridView.Rows.Clear();
    }

    private void findString_Click(object sender, EventArgs e)
    {
      int row = gameListDataGridView.CurrentCell.RowIndex;
      if ((row > 0) || row0found)
      {
        row++;
      }

      for (; row < gameListDataGridView.Rows.Count; row++)
      {
        if (gameListDataGridView.Rows[row].Cells["moveList"].Value.ToString().IndexOf(findStr.Text) > 0)
        {
          gameListDataGridView.CurrentCell = gameListDataGridView.Rows[row].Cells["moveList"];
          row0found = (row == 0);
          return;
        }
      }

      if (row == gameListDataGridView.Rows.Count)
      {
        gameListDataGridView.CurrentCell = gameListDataGridView.Rows[0].Cells["moveList"];
      }
      MessageBox.Show("Not found...");

    }

    private void loadAllFirst_Click(object sender, EventArgs e)
    {
      loadAll = true;
    }

    private void cancel_Click(object sender, EventArgs e)
    {

    }

    private void findStr_TextChanged(object sender, EventArgs e)
    {

    }

    private void ok_Click(object sender, EventArgs e)
    {

    }

    private void GameList_Load(object sender, EventArgs e)
    {

    }
  }
}
