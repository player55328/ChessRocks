namespace ChessRocks
{
  partial class getHeader
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(getHeader));
      this.header = new System.Windows.Forms.TextBox();
      this.ok = new System.Windows.Forms.Button();
      this.cancel = new System.Windows.Forms.Button();
      this.warning = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // header
      // 
      this.header.Location = new System.Drawing.Point(13, 18);
      this.header.MaxLength = 96;
      this.header.Name = "header";
      this.header.Size = new System.Drawing.Size(259, 20);
      this.header.TabIndex = 0;
      this.header.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
      this.header.TextChanged += new System.EventHandler(this.header_TextChanged);
      // 
      // ok
      // 
      this.ok.Location = new System.Drawing.Point(197, 77);
      this.ok.Name = "ok";
      this.ok.Size = new System.Drawing.Size(75, 23);
      this.ok.TabIndex = 1;
      this.ok.Text = "OK";
      this.ok.UseVisualStyleBackColor = true;
      this.ok.Click += new System.EventHandler(this.ok_Click);
      // 
      // cancel
      // 
      this.cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancel.Location = new System.Drawing.Point(12, 77);
      this.cancel.Name = "cancel";
      this.cancel.Size = new System.Drawing.Size(75, 23);
      this.cancel.TabIndex = 2;
      this.cancel.Text = "Cancel";
      this.cancel.UseVisualStyleBackColor = true;
      this.cancel.Click += new System.EventHandler(this.cancel_Click);
      // 
      // warning
      // 
      this.warning.AutoSize = true;
      this.warning.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
      this.warning.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(128)))));
      this.warning.Location = new System.Drawing.Point(55, 47);
      this.warning.Name = "warning";
      this.warning.Size = new System.Drawing.Size(174, 13);
      this.warning.TabIndex = 3;
      this.warning.Text = "This header is already in use!";
      // 
      // getHeader
      // 
      this.AcceptButton = this.ok;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.cancel;
      this.ClientSize = new System.Drawing.Size(304, 112);
      this.Controls.Add(this.warning);
      this.Controls.Add(this.cancel);
      this.Controls.Add(this.ok);
      this.Controls.Add(this.header);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MaximumSize = new System.Drawing.Size(320, 150);
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(320, 150);
      this.Name = "getHeader";
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Save option settings under what heading?";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button ok;
    private System.Windows.Forms.Button cancel;
    public System.Windows.Forms.TextBox header;
    private System.Windows.Forms.Label warning;
  }
}