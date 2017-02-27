namespace ChessRocks
{
  partial class About
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(About));
      this.aboutLabel = new System.Windows.Forms.Label();
      this.pictureBox1 = new System.Windows.Forms.PictureBox();
      this.ok = new System.Windows.Forms.Button();
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
      this.SuspendLayout();
      // 
      // aboutLabel
      // 
      this.aboutLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.aboutLabel.AutoSize = true;
      this.aboutLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.aboutLabel.Location = new System.Drawing.Point(143, 11);
      this.aboutLabel.Name = "aboutLabel";
      this.aboutLabel.Size = new System.Drawing.Size(219, 13);
      this.aboutLabel.TabIndex = 187;
      this.aboutLabel.Text = "Chess Rocks! © 2014  Kenneth Chamberlain";
      // 
      // pictureBox1
      // 
      this.pictureBox1.Image = global::ChessRocks.Properties.Resources.uci;
      this.pictureBox1.InitialImage = global::ChessRocks.Properties.Resources.uci;
      this.pictureBox1.Location = new System.Drawing.Point(6, 11);
      this.pictureBox1.Name = "pictureBox1";
      this.pictureBox1.Size = new System.Drawing.Size(130, 65);
      this.pictureBox1.TabIndex = 0;
      this.pictureBox1.TabStop = false;
      // 
      // ok
      // 
      this.ok.Location = new System.Drawing.Point(287, 53);
      this.ok.Name = "ok";
      this.ok.Size = new System.Drawing.Size(75, 23);
      this.ok.TabIndex = 188;
      this.ok.Text = "OK";
      this.ok.UseVisualStyleBackColor = true;
      this.ok.Click += new System.EventHandler(this.ok_Click);
      // 
      // About
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(374, 87);
      this.Controls.Add(this.ok);
      this.Controls.Add(this.aboutLabel);
      this.Controls.Add(this.pictureBox1);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximumSize = new System.Drawing.Size(390, 125);
      this.MinimumSize = new System.Drawing.Size(390, 125);
      this.Name = "About";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Chess Rocks!";
      ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.PictureBox pictureBox1;
    private System.Windows.Forms.Label aboutLabel;
    private System.Windows.Forms.Button ok;
  }
}