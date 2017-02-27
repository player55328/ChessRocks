namespace ChessRocks
{
  partial class GameList
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.components = new System.ComponentModel.Container();
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GameList));
      this.gameListDataGridView = new System.Windows.Forms.DataGridView();
      this.ok = new System.Windows.Forms.Button();
      this.cancel = new System.Windows.Forms.Button();
      this.clear = new System.Windows.Forms.Button();
      this.findStr = new System.Windows.Forms.TextBox();
      this.findString = new System.Windows.Forms.Button();
      this.loadAllFirst = new System.Windows.Forms.Button();
      this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
      this.gameNum = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.white = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.black = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.result = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.moveList = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.header = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.fen = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.curpos = new System.Windows.Forms.DataGridViewTextBoxColumn();
      this.variant = new System.Windows.Forms.DataGridViewTextBoxColumn();
      ((System.ComponentModel.ISupportInitialize)(this.gameListDataGridView)).BeginInit();
      this.SuspendLayout();
      // 
      // gameListDataGridView
      // 
      this.gameListDataGridView.AllowUserToAddRows = false;
      this.gameListDataGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.gameListDataGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
      this.gameListDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
      this.gameListDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.gameNum,
            this.white,
            this.black,
            this.result,
            this.moveList,
            this.header,
            this.fen,
            this.curpos,
            this.variant});
      this.gameListDataGridView.Location = new System.Drawing.Point(12, 12);
      this.gameListDataGridView.Name = "gameListDataGridView";
      this.gameListDataGridView.ReadOnly = true;
      this.gameListDataGridView.RowHeadersVisible = false;
      this.gameListDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
      this.gameListDataGridView.Size = new System.Drawing.Size(852, 550);
      this.gameListDataGridView.TabIndex = 0;
      // 
      // ok
      // 
      this.ok.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.ok.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.ok.Location = new System.Drawing.Point(756, 572);
      this.ok.Name = "ok";
      this.ok.Size = new System.Drawing.Size(75, 23);
      this.ok.TabIndex = 1;
      this.ok.Text = "OK";
      this.toolTip1.SetToolTip(this.ok, "Load the selected game onto the board.");
      this.ok.UseVisualStyleBackColor = true;
      this.ok.Click += new System.EventHandler(this.ok_Click);
      // 
      // cancel
      // 
      this.cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancel.Location = new System.Drawing.Point(442, 572);
      this.cancel.Name = "cancel";
      this.cancel.Size = new System.Drawing.Size(75, 23);
      this.cancel.TabIndex = 2;
      this.cancel.Text = "Cancel";
      this.toolTip1.SetToolTip(this.cancel, "Cancel the request.");
      this.cancel.UseVisualStyleBackColor = true;
      this.cancel.Click += new System.EventHandler(this.cancel_Click);
      // 
      // clear
      // 
      this.clear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.clear.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.clear.Location = new System.Drawing.Point(323, 572);
      this.clear.Name = "clear";
      this.clear.Size = new System.Drawing.Size(75, 23);
      this.clear.TabIndex = 3;
      this.clear.Text = "Clear List";
      this.toolTip1.SetToolTip(this.clear, "Clear all the games listed.");
      this.clear.UseVisualStyleBackColor = true;
      this.clear.Click += new System.EventHandler(this.clear_Click);
      // 
      // findStr
      // 
      this.findStr.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.findStr.Location = new System.Drawing.Point(15, 574);
      this.findStr.Name = "findStr";
      this.findStr.Size = new System.Drawing.Size(100, 20);
      this.findStr.TabIndex = 4;
      this.toolTip1.SetToolTip(this.findStr, "Exact string to try and find in the Move List column of the game list.");
      this.findStr.TextChanged += new System.EventHandler(this.findStr_TextChanged);
      // 
      // findString
      // 
      this.findString.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.findString.Location = new System.Drawing.Point(131, 572);
      this.findString.Name = "findString";
      this.findString.Size = new System.Drawing.Size(75, 23);
      this.findString.TabIndex = 5;
      this.findString.Text = "Find String";
      this.toolTip1.SetToolTip(this.findString, "Find the specified text in the Move List column.");
      this.findString.UseVisualStyleBackColor = true;
      this.findString.Click += new System.EventHandler(this.findString_Click);
      // 
      // loadAllFirst
      // 
      this.loadAllFirst.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
      this.loadAllFirst.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.loadAllFirst.Location = new System.Drawing.Point(650, 572);
      this.loadAllFirst.Name = "loadAllFirst";
      this.loadAllFirst.Size = new System.Drawing.Size(75, 23);
      this.loadAllFirst.TabIndex = 6;
      this.loadAllFirst.Text = "Load All First";
      this.toolTip1.SetToolTip(this.loadAllFirst, "Check all games in the game list for correct PGN syntax.");
      this.loadAllFirst.UseVisualStyleBackColor = true;
      this.loadAllFirst.Click += new System.EventHandler(this.loadAllFirst_Click);
      // 
      // gameNum
      // 
      this.gameNum.Frozen = true;
      this.gameNum.HeaderText = "Game";
      this.gameNum.Name = "gameNum";
      this.gameNum.ReadOnly = true;
      this.gameNum.Width = 60;
      // 
      // white
      // 
      this.white.Frozen = true;
      this.white.HeaderText = "White";
      this.white.Name = "white";
      this.white.ReadOnly = true;
      this.white.Width = 60;
      // 
      // black
      // 
      this.black.Frozen = true;
      this.black.HeaderText = "Black";
      this.black.Name = "black";
      this.black.ReadOnly = true;
      this.black.Width = 59;
      // 
      // result
      // 
      this.result.Frozen = true;
      this.result.HeaderText = "Result";
      this.result.Name = "result";
      this.result.ReadOnly = true;
      this.result.Width = 62;
      // 
      // moveList
      // 
      this.moveList.HeaderText = "Move List";
      this.moveList.Name = "moveList";
      this.moveList.ReadOnly = true;
      this.moveList.Width = 78;
      // 
      // header
      // 
      this.header.HeaderText = "Header";
      this.header.Name = "header";
      this.header.ReadOnly = true;
      this.header.Width = 67;
      // 
      // fen
      // 
      this.fen.HeaderText = "FEN";
      this.fen.MaxInputLength = 64;
      this.fen.Name = "fen";
      this.fen.ReadOnly = true;
      this.fen.Width = 53;
      // 
      // curpos
      // 
      this.curpos.HeaderText = "Current Position";
      this.curpos.MaxInputLength = 64;
      this.curpos.Name = "curpos";
      this.curpos.ReadOnly = true;
      this.curpos.Width = 97;
      // 
      // variant
      // 
      this.variant.HeaderText = "Variant";
      this.variant.MaxInputLength = 64;
      this.variant.Name = "variant";
      this.variant.ReadOnly = true;
      this.variant.Width = 65;
      // 
      // GameList
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(876, 602);
      this.Controls.Add(this.loadAllFirst);
      this.Controls.Add(this.findString);
      this.Controls.Add(this.findStr);
      this.Controls.Add(this.clear);
      this.Controls.Add(this.cancel);
      this.Controls.Add(this.ok);
      this.Controls.Add(this.gameListDataGridView);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.Name = "GameList";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Game List - Select the game to put on the board...";
      this.Load += new System.EventHandler(this.GameList_Load);
      ((System.ComponentModel.ISupportInitialize)(this.gameListDataGridView)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    public System.Windows.Forms.DataGridView gameListDataGridView;
    private System.Windows.Forms.Button ok;
    private System.Windows.Forms.Button cancel;
    private System.Windows.Forms.Button clear;
    private System.Windows.Forms.TextBox findStr;
    private System.Windows.Forms.Button findString;
    private System.Windows.Forms.Button loadAllFirst;
    private System.Windows.Forms.ToolTip toolTip1;
    private System.Windows.Forms.DataGridViewTextBoxColumn gameNum;
    private System.Windows.Forms.DataGridViewTextBoxColumn white;
    private System.Windows.Forms.DataGridViewTextBoxColumn black;
    private System.Windows.Forms.DataGridViewTextBoxColumn result;
    private System.Windows.Forms.DataGridViewTextBoxColumn moveList;
    private System.Windows.Forms.DataGridViewTextBoxColumn header;
    private System.Windows.Forms.DataGridViewTextBoxColumn fen;
    private System.Windows.Forms.DataGridViewTextBoxColumn curpos;
    private System.Windows.Forms.DataGridViewTextBoxColumn variant;

  }
}