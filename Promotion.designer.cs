namespace ns_Promotion
{
  partial class Promotion
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
      this.promoteQueen = new System.Windows.Forms.Panel();
      this.promoteRook = new System.Windows.Forms.Panel();
      this.promoteKnight = new System.Windows.Forms.Panel();
      this.promoteBishop = new System.Windows.Forms.Panel();
      this.label1 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // promoteQueen
      // 
      this.promoteQueen.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.promoteQueen.Location = new System.Drawing.Point(16, 53);
      this.promoteQueen.Name = "promoteQueen";
      this.promoteQueen.Size = new System.Drawing.Size(72, 72);
      this.promoteQueen.TabIndex = 0;
      this.promoteQueen.Click += new System.EventHandler(this.promoteQueen_Click);
      // 
      // promoteRook
      // 
      this.promoteRook.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.promoteRook.Location = new System.Drawing.Point(131, 53);
      this.promoteRook.Name = "promoteRook";
      this.promoteRook.Size = new System.Drawing.Size(72, 72);
      this.promoteRook.TabIndex = 1;
      this.promoteRook.Click += new System.EventHandler(this.promoteRook_Click);
      // 
      // promoteKnight
      // 
      this.promoteKnight.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.promoteKnight.Location = new System.Drawing.Point(246, 53);
      this.promoteKnight.Name = "promoteKnight";
      this.promoteKnight.Size = new System.Drawing.Size(72, 72);
      this.promoteKnight.TabIndex = 1;
      this.promoteKnight.Click += new System.EventHandler(this.promoteKnight_Click);
      // 
      // promoteBishop
      // 
      this.promoteBishop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.promoteBishop.Location = new System.Drawing.Point(361, 53);
      this.promoteBishop.Name = "promoteBishop";
      this.promoteBishop.Size = new System.Drawing.Size(72, 72);
      this.promoteBishop.TabIndex = 1;
      this.promoteBishop.Click += new System.EventHandler(this.promoteBishop_Click);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.label1.Location = new System.Drawing.Point(167, 15);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(115, 13);
      this.label1.TabIndex = 2;
      this.label1.Text = "Select Promotion...";
      // 
      // Promotion
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(128)))), ((int)(((byte)(255)))));
      this.ClientSize = new System.Drawing.Size(449, 152);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.promoteBishop);
      this.Controls.Add(this.promoteKnight);
      this.Controls.Add(this.promoteRook);
      this.Controls.Add(this.promoteQueen);
      this.Name = "Promotion";
      this.Text = "Pawn Promotion";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Panel promoteQueen;
    private System.Windows.Forms.Panel promoteRook;
    private System.Windows.Forms.Panel promoteKnight;
    private System.Windows.Forms.Panel promoteBishop;
    private System.Windows.Forms.Label label1;
  }
}