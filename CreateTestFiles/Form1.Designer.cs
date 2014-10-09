namespace CreateTestFiles
{
  partial class Form1
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
      this.label2 = new System.Windows.Forms.Label();
      this.textBoxShipmentNotificationFile = new System.Windows.Forms.TextBox();
      this.buttonSelectShipmentNotificationFile = new System.Windows.Forms.Button();
      this.buttonCreateShipmentConfirmationFile = new System.Windows.Forms.Button();
      this.buttonCreateProductRelationFile = new System.Windows.Forms.Button();
      this.buttonSelectProductInformationFile = new System.Windows.Forms.Button();
      this.textBoxProductInformationFile = new System.Windows.Forms.TextBox();
      this.label3 = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(12, 80);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(126, 13);
      this.label2.TabIndex = 0;
      this.label2.Text = "Shipment Notification File";
      // 
      // textBoxShipmentNotificationFile
      // 
      this.textBoxShipmentNotificationFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxShipmentNotificationFile.Location = new System.Drawing.Point(162, 77);
      this.textBoxShipmentNotificationFile.Name = "textBoxShipmentNotificationFile";
      this.textBoxShipmentNotificationFile.Size = new System.Drawing.Size(461, 20);
      this.textBoxShipmentNotificationFile.TabIndex = 1;
      // 
      // buttonSelectShipmentNotificationFile
      // 
      this.buttonSelectShipmentNotificationFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonSelectShipmentNotificationFile.Location = new System.Drawing.Point(629, 75);
      this.buttonSelectShipmentNotificationFile.Name = "buttonSelectShipmentNotificationFile";
      this.buttonSelectShipmentNotificationFile.Size = new System.Drawing.Size(25, 23);
      this.buttonSelectShipmentNotificationFile.TabIndex = 2;
      this.buttonSelectShipmentNotificationFile.Text = "...";
      this.buttonSelectShipmentNotificationFile.UseVisualStyleBackColor = true;
      this.buttonSelectShipmentNotificationFile.Click += new System.EventHandler(this.buttonSelectShipmentNotificationFile_Click);
      // 
      // buttonCreateShipmentConfirmationFile
      // 
      this.buttonCreateShipmentConfirmationFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCreateShipmentConfirmationFile.Location = new System.Drawing.Point(660, 75);
      this.buttonCreateShipmentConfirmationFile.Name = "buttonCreateShipmentConfirmationFile";
      this.buttonCreateShipmentConfirmationFile.Size = new System.Drawing.Size(204, 23);
      this.buttonCreateShipmentConfirmationFile.TabIndex = 3;
      this.buttonCreateShipmentConfirmationFile.Text = "Create Shipment Confirmation File";
      this.buttonCreateShipmentConfirmationFile.UseVisualStyleBackColor = true;
      this.buttonCreateShipmentConfirmationFile.Click += new System.EventHandler(this.buttonCreateShipmentConfirmationFile_Click);
      // 
      // buttonCreateProductRelationFile
      // 
      this.buttonCreateProductRelationFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonCreateProductRelationFile.Location = new System.Drawing.Point(660, 31);
      this.buttonCreateProductRelationFile.Name = "buttonCreateProductRelationFile";
      this.buttonCreateProductRelationFile.Size = new System.Drawing.Size(204, 23);
      this.buttonCreateProductRelationFile.TabIndex = 7;
      this.buttonCreateProductRelationFile.Text = "Create Product Relation File";
      this.buttonCreateProductRelationFile.UseVisualStyleBackColor = true;
      this.buttonCreateProductRelationFile.Click += new System.EventHandler(this.buttonCreateProductRelationFile_Click);
      // 
      // buttonSelectProductInformationFile
      // 
      this.buttonSelectProductInformationFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.buttonSelectProductInformationFile.Location = new System.Drawing.Point(629, 31);
      this.buttonSelectProductInformationFile.Name = "buttonSelectProductInformationFile";
      this.buttonSelectProductInformationFile.Size = new System.Drawing.Size(25, 23);
      this.buttonSelectProductInformationFile.TabIndex = 6;
      this.buttonSelectProductInformationFile.Text = "...";
      this.buttonSelectProductInformationFile.UseVisualStyleBackColor = true;
      this.buttonSelectProductInformationFile.Click += new System.EventHandler(this.buttonSelectProductInformationFile_Click);
      // 
      // textBoxProductInformationFile
      // 
      this.textBoxProductInformationFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.textBoxProductInformationFile.Location = new System.Drawing.Point(162, 33);
      this.textBoxProductInformationFile.Name = "textBoxProductInformationFile";
      this.textBoxProductInformationFile.Size = new System.Drawing.Size(461, 20);
      this.textBoxProductInformationFile.TabIndex = 5;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(12, 36);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(118, 13);
      this.label3.TabIndex = 4;
      this.label3.Text = "Product Information File";
      // 
      // Form1
      // 
      this.ClientSize = new System.Drawing.Size(876, 384);
      this.Controls.Add(this.buttonCreateProductRelationFile);
      this.Controls.Add(this.buttonSelectProductInformationFile);
      this.Controls.Add(this.textBoxProductInformationFile);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.buttonCreateShipmentConfirmationFile);
      this.Controls.Add(this.buttonSelectShipmentNotificationFile);
      this.Controls.Add(this.textBoxShipmentNotificationFile);
      this.Controls.Add(this.label2);
      this.Name = "Form1";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Button button1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TextBox textBox1;
    private System.Windows.Forms.Button button2;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.TextBox textBoxShipmentNotificationFile;
    private System.Windows.Forms.Button buttonSelectShipmentNotificationFile;
    private System.Windows.Forms.Button buttonCreateShipmentConfirmationFile;
    private System.Windows.Forms.Button buttonCreateProductRelationFile;
    private System.Windows.Forms.Button buttonSelectProductInformationFile;
    private System.Windows.Forms.TextBox textBoxProductInformationFile;
    private System.Windows.Forms.Label label3;
  }
}

