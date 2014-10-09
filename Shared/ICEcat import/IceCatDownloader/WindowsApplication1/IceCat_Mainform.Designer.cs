namespace WindowsApplication1
{
    partial class frm_IceCat_Main
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
            this.richTextBox1 = new System.Windows.Forms.RichTextBox();
            this.txtB_User = new System.Windows.Forms.TextBox();
            this.txtB_Pass = new System.Windows.Forms.TextBox();
            this.txtB_Path = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.richTextBox2 = new System.Windows.Forms.RichTextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.button5 = new System.Windows.Forms.Button();
            this.button7 = new System.Windows.Forms.Button();
            this.txtB_Detail = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.txtB_DutchFull = new System.Windows.Forms.TextBox();
            this.txtB_EnglishFull = new System.Windows.Forms.TextBox();
            this.txtB_FrenchFull = new System.Windows.Forms.TextBox();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.button6 = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.button_stop = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.pic_LeftBar = new System.Windows.Forms.PictureBox();
            this.timer_FullImport = new System.Windows.Forms.Timer(this.components);
            this.button4 = new System.Windows.Forms.Button();
            this.button8 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic_LeftBar)).BeginInit();
            this.SuspendLayout();
            // 
            // richTextBox1
            // 
            this.richTextBox1.Location = new System.Drawing.Point(214, 164);
            this.richTextBox1.Name = "richTextBox1";
            this.richTextBox1.Size = new System.Drawing.Size(513, 99);
            this.richTextBox1.TabIndex = 1;
            this.richTextBox1.Text = "";
            // 
            // txtB_User
            // 
            this.txtB_User.Location = new System.Drawing.Point(644, 62);
            this.txtB_User.Name = "txtB_User";
            this.txtB_User.Size = new System.Drawing.Size(116, 20);
            this.txtB_User.TabIndex = 4;
            this.txtB_User.Text = "bascomputers";
            this.txtB_User.Visible = false;
            // 
            // txtB_Pass
            // 
            this.txtB_Pass.AcceptsReturn = true;
            this.txtB_Pass.Location = new System.Drawing.Point(644, 84);
            this.txtB_Pass.Name = "txtB_Pass";
            this.txtB_Pass.Size = new System.Drawing.Size(116, 20);
            this.txtB_Pass.TabIndex = 5;
            this.txtB_Pass.Text = "1019asr";
            this.txtB_Pass.Visible = false;
            // 
            // txtB_Path
            // 
            this.txtB_Path.Location = new System.Drawing.Point(302, 290);
            this.txtB_Path.Name = "txtB_Path";
            this.txtB_Path.ReadOnly = true;
            this.txtB_Path.Size = new System.Drawing.Size(319, 20);
            this.txtB_Path.TabIndex = 6;
            this.txtB_Path.Text = "C:\\\\userfolder\\\\Importxml\\\\ProductList.xml";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(214, 344);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(122, 13);
            this.label1.TabIndex = 10;
            this.label1.Text = "Icecat full index urls";
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(631, 456);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(96, 23);
            this.button3.TabIndex = 12;
            this.button3.Text = "5. Products";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // timer1
            // 
            this.timer1.Interval = 300;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // richTextBox2
            // 
            this.richTextBox2.Location = new System.Drawing.Point(217, 486);
            this.richTextBox2.Name = "richTextBox2";
            this.richTextBox2.Size = new System.Drawing.Size(513, 65);
            this.richTextBox2.TabIndex = 13;
            this.richTextBox2.Text = "";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(211, 266);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(86, 13);
            this.label3.TabIndex = 14;
            this.label3.Text = "Template files";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(214, 469);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(46, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "Error log";
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(631, 306);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(96, 26);
            this.button5.TabIndex = 17;
            this.button5.Text = "Images2SQL";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Visible = false;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(634, 265);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(93, 41);
            this.button7.TabIndex = 19;
            this.button7.Text = "Download all Pictures";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // txtB_Detail
            // 
            this.txtB_Detail.Location = new System.Drawing.Point(302, 317);
            this.txtB_Detail.Name = "txtB_Detail";
            this.txtB_Detail.ReadOnly = true;
            this.txtB_Detail.Size = new System.Drawing.Size(319, 20);
            this.txtB_Detail.TabIndex = 20;
            this.txtB_Detail.Text = "C:\\\\userfolder\\\\Importxml\\\\ProductDetailTemp.xml";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(211, 293);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(85, 13);
            this.label5.TabIndex = 21;
            this.label5.Text = "Full productmap:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(211, 317);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(75, 13);
            this.label6.TabIndex = 22;
            this.label6.Text = "Product detail:";
            // 
            // txtB_DutchFull
            // 
            this.txtB_DutchFull.Location = new System.Drawing.Point(302, 367);
            this.txtB_DutchFull.Name = "txtB_DutchFull";
            this.txtB_DutchFull.ReadOnly = true;
            this.txtB_DutchFull.Size = new System.Drawing.Size(283, 20);
            this.txtB_DutchFull.TabIndex = 24;
            this.txtB_DutchFull.Text = "http://data.icecat.biz/export/level4/NL/files.index.xml";
            this.txtB_DutchFull.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            // 
            // txtB_EnglishFull
            // 
            this.txtB_EnglishFull.Location = new System.Drawing.Point(302, 393);
            this.txtB_EnglishFull.Name = "txtB_EnglishFull";
            this.txtB_EnglishFull.ReadOnly = true;
            this.txtB_EnglishFull.Size = new System.Drawing.Size(283, 20);
            this.txtB_EnglishFull.TabIndex = 25;
            this.txtB_EnglishFull.Text = "http://data.icecat.biz/export/level4/EN/files.index.xml";
            // 
            // txtB_FrenchFull
            // 
            this.txtB_FrenchFull.Location = new System.Drawing.Point(302, 419);
            this.txtB_FrenchFull.Name = "txtB_FrenchFull";
            this.txtB_FrenchFull.ReadOnly = true;
            this.txtB_FrenchFull.Size = new System.Drawing.Size(283, 20);
            this.txtB_FrenchFull.TabIndex = 26;
            this.txtB_FrenchFull.Text = "http://data.icecat.biz/export/level4/FR/files.index.xml";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Enabled = false;
            this.checkBox1.Location = new System.Drawing.Point(214, 370);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(55, 17);
            this.checkBox1.TabIndex = 30;
            this.checkBox1.Text = "Dutch";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Checked = true;
            this.checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox2.Enabled = false;
            this.checkBox2.Location = new System.Drawing.Point(214, 396);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(60, 17);
            this.checkBox2.TabIndex = 31;
            this.checkBox2.Text = "English";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Location = new System.Drawing.Point(214, 422);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(59, 17);
            this.checkBox3.TabIndex = 32;
            this.checkBox3.Text = "French";
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(631, 338);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(96, 22);
            this.button6.TabIndex = 33;
            this.button6.Text = "1. Clean Table";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.pictureBox1.Image = global::IceCatDownloader.Properties.Resources.TopBar;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(805, 133);
            this.pictureBox1.TabIndex = 35;
            this.pictureBox1.TabStop = false;
            // 
            // button_stop
            // 
            this.button_stop.BackgroundImage = global::IceCatDownloader.Properties.Resources.Button_Stop_icon_24;
            this.button_stop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.button_stop.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_stop.Location = new System.Drawing.Point(12, 230);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(125, 33);
            this.button_stop.TabIndex = 16;
            this.button_stop.Text = "S&top";
            this.button_stop.UseVisualStyleBackColor = true;
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // button2
            // 
            this.button2.BackgroundImage = global::IceCatDownloader.Properties.Resources.Button_Close_icon_241;
            this.button2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button2.Location = new System.Drawing.Point(12, 497);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(125, 33);
            this.button2.TabIndex = 2;
            this.button2.Text = "E&xit";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.BackgroundImage = global::IceCatDownloader.Properties.Resources.Button_Play_icon_241;
            this.button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(12, 164);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(125, 33);
            this.button1.TabIndex = 0;
            this.button1.Text = "&Start";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // pic_LeftBar
            // 
            this.pic_LeftBar.Image = global::IceCatDownloader.Properties.Resources.LeftBar;
            this.pic_LeftBar.Location = new System.Drawing.Point(0, 130);
            this.pic_LeftBar.Name = "pic_LeftBar";
            this.pic_LeftBar.Size = new System.Drawing.Size(190, 498);
            this.pic_LeftBar.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pic_LeftBar.TabIndex = 34;
            this.pic_LeftBar.TabStop = false;
            this.pic_LeftBar.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // timer_FullImport
            // 
            this.timer_FullImport.Interval = 14400000;
            this.timer_FullImport.Tick += new System.EventHandler(this.timer_FullImport_Tick);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(631, 365);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(96, 22);
            this.button4.TabIndex = 36;
            this.button4.Text = "2. Import NL";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button8
            // 
            this.button8.Location = new System.Drawing.Point(631, 391);
            this.button8.Name = "button8";
            this.button8.Size = new System.Drawing.Size(96, 22);
            this.button8.TabIndex = 37;
            this.button8.Text = "3. Import EN";
            this.button8.UseVisualStyleBackColor = true;
            this.button8.Click += new System.EventHandler(this.button8_Click);
            // 
            // button9
            // 
            this.button9.Location = new System.Drawing.Point(631, 415);
            this.button9.Name = "button9";
            this.button9.Size = new System.Drawing.Size(96, 35);
            this.button9.TabIndex = 38;
            this.button9.Text = "4. Process full import";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            // 
            // frm_IceCat_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(805, 569);
            this.Controls.Add(this.button9);
            this.Controls.Add(this.button8);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.button6);
            this.Controls.Add(this.checkBox3);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.txtB_FrenchFull);
            this.Controls.Add(this.txtB_EnglishFull);
            this.Controls.Add(this.txtB_DutchFull);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.txtB_Detail);
            this.Controls.Add(this.button7);
            this.Controls.Add(this.button5);
            this.Controls.Add(this.button_stop);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.richTextBox2);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtB_Path);
            this.Controls.Add(this.txtB_Pass);
            this.Controls.Add(this.txtB_User);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.richTextBox1);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.pic_LeftBar);
            this.Name = "frm_IceCat_Main";
            this.Text = "IceCat Download and Import";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pic_LeftBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.RichTextBox richTextBox1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox txtB_User;
        private System.Windows.Forms.TextBox txtB_Pass;
        private System.Windows.Forms.TextBox txtB_Path;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.RichTextBox richTextBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button_stop;
        private System.Windows.Forms.Button button5;
        private System.Windows.Forms.Button button7;
        private System.Windows.Forms.TextBox txtB_Detail;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox txtB_DutchFull;
        private System.Windows.Forms.TextBox txtB_EnglishFull;
        private System.Windows.Forms.TextBox txtB_FrenchFull;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.Button button6;
        private System.Windows.Forms.PictureBox pic_LeftBar;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Timer timer_FullImport;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button8;
        private System.Windows.Forms.Button button9;
    }
}

