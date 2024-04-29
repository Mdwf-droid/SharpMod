namespace AdvancedNetModTest
{
    partial class Form1
    {
        /// <summary>
        /// Variable nécessaire au concepteur.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Nettoyage des ressources utilisées.
        /// </summary>
        /// <param name="disposing">true si les ressources managées doivent être supprimées ; sinon, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Code généré par le Concepteur Windows Form

        /// <summary>
        /// Méthode requise pour la prise en charge du concepteur - ne modifiez pas
        /// le contenu de cette méthode avec l'éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            btnPlayStop = new System.Windows.Forms.Button();
            groupBox1 = new System.Windows.Forms.GroupBox();
            checkBox3 = new System.Windows.Forms.CheckBox();
            checkBox1 = new System.Windows.Forms.CheckBox();
            groupBox2 = new System.Windows.Forms.GroupBox();
            radioButton3 = new System.Windows.Forms.RadioButton();
            radioButton2 = new System.Windows.Forms.RadioButton();
            radioButton1 = new System.Windows.Forms.RadioButton();
            BtnOpen = new System.Windows.Forms.Button();
            tbModNfo = new System.Windows.Forms.TextBox();
            ofdModFile = new System.Windows.Forms.OpenFileDialog();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            tbPatNo = new System.Windows.Forms.TextBox();
            tbPos = new System.Windows.Forms.TextBox();
            chkLoop = new System.Windows.Forms.CheckBox();
            vuMeterRight = new SharpMod.Win.UI.VuMeter();
            vuMeterLeft = new SharpMod.Win.UI.VuMeter();
            listView1 = new System.Windows.Forms.ListView();
            columnHeader1 = new System.Windows.Forms.ColumnHeader();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // btnPlayStop
            // 
            btnPlayStop.Location = new System.Drawing.Point(92, 1);
            btnPlayStop.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            btnPlayStop.Name = "btnPlayStop";
            btnPlayStop.Size = new System.Drawing.Size(88, 27);
            btnPlayStop.TabIndex = 4;
            btnPlayStop.Text = "Play";
            btnPlayStop.UseVisualStyleBackColor = true;
            btnPlayStop.Click += Button2Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(checkBox3);
            groupBox1.Controls.Add(checkBox1);
            groupBox1.Location = new System.Drawing.Point(8, 89);
            groupBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox1.Size = new System.Drawing.Size(97, 81);
            groupBox1.TabIndex = 7;
            groupBox1.TabStop = false;
            groupBox1.Text = "groupBox1";
            // 
            // checkBox3
            // 
            checkBox3.AutoSize = true;
            checkBox3.Location = new System.Drawing.Point(9, 50);
            checkBox3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBox3.Name = "checkBox3";
            checkBox3.Size = new System.Drawing.Size(83, 19);
            checkBox3.TabIndex = 2;
            checkBox3.Text = "Interpolate";
            checkBox3.UseVisualStyleBackColor = true;
            // 
            // checkBox1
            // 
            checkBox1.AutoSize = true;
            checkBox1.Location = new System.Drawing.Point(9, 23);
            checkBox1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            checkBox1.Name = "checkBox1";
            checkBox1.Size = new System.Drawing.Size(60, 19);
            checkBox1.TabIndex = 0;
            checkBox1.Text = "16 Bits";
            checkBox1.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(radioButton3);
            groupBox2.Controls.Add(radioButton2);
            groupBox2.Controls.Add(radioButton1);
            groupBox2.Location = new System.Drawing.Point(8, 177);
            groupBox2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            groupBox2.Size = new System.Drawing.Size(98, 88);
            groupBox2.TabIndex = 8;
            groupBox2.TabStop = false;
            groupBox2.Text = "groupBox2";
            // 
            // radioButton3
            // 
            radioButton3.AutoSize = true;
            radioButton3.Location = new System.Drawing.Point(8, 61);
            radioButton3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButton3.Name = "radioButton3";
            radioButton3.Size = new System.Drawing.Size(74, 19);
            radioButton3.TabIndex = 2;
            radioButton3.TabStop = true;
            radioButton3.Text = "Surround";
            radioButton3.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            radioButton2.AutoSize = true;
            radioButton2.Location = new System.Drawing.Point(8, 40);
            radioButton2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButton2.Name = "radioButton2";
            radioButton2.Size = new System.Drawing.Size(58, 19);
            radioButton2.TabIndex = 1;
            radioButton2.TabStop = true;
            radioButton2.Text = "Stereo";
            radioButton2.UseVisualStyleBackColor = true;
            // 
            // radioButton1
            // 
            radioButton1.AutoSize = true;
            radioButton1.Location = new System.Drawing.Point(8, 21);
            radioButton1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            radioButton1.Name = "radioButton1";
            radioButton1.Size = new System.Drawing.Size(57, 19);
            radioButton1.TabIndex = 0;
            radioButton1.TabStop = true;
            radioButton1.Text = "Mono";
            radioButton1.UseVisualStyleBackColor = true;
            // 
            // BtnOpen
            // 
            BtnOpen.Location = new System.Drawing.Point(2, 1);
            BtnOpen.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            BtnOpen.Name = "BtnOpen";
            BtnOpen.Size = new System.Drawing.Size(88, 27);
            BtnOpen.TabIndex = 11;
            BtnOpen.Text = "Open";
            BtnOpen.UseVisualStyleBackColor = true;
            BtnOpen.Click += BtnOpenClick;
            // 
            // tbModNfo
            // 
            tbModNfo.Location = new System.Drawing.Point(283, 35);
            tbModNfo.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbModNfo.Multiline = true;
            tbModNfo.Name = "tbModNfo";
            tbModNfo.ReadOnly = true;
            tbModNfo.Size = new System.Drawing.Size(482, 100);
            tbModNfo.TabIndex = 12;
            // 
            // ofdModFile
            // 
            ofdModFile.FileName = "openFileDialog1";
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(187, 13);
            label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(34, 15);
            label1.TabIndex = 13;
            label1.Text = "Pat #";
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(313, 13);
            label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(32, 15);
            label2.TabIndex = 14;
            label2.Text = "Pos :";
            // 
            // tbPatNo
            // 
            tbPatNo.Location = new System.Drawing.Point(223, 9);
            tbPatNo.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbPatNo.Name = "tbPatNo";
            tbPatNo.Size = new System.Drawing.Size(61, 23);
            tbPatNo.TabIndex = 15;
            // 
            // tbPos
            // 
            tbPos.Location = new System.Drawing.Point(349, 9);
            tbPos.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            tbPos.Name = "tbPos";
            tbPos.Size = new System.Drawing.Size(82, 23);
            tbPos.TabIndex = 16;
            // 
            // chkLoop
            // 
            chkLoop.AutoSize = true;
            chkLoop.Location = new System.Drawing.Point(534, 8);
            chkLoop.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            chkLoop.Name = "chkLoop";
            chkLoop.Size = new System.Drawing.Size(53, 19);
            chkLoop.TabIndex = 17;
            chkLoop.Text = "Loop";
            chkLoop.UseVisualStyleBackColor = true;
            chkLoop.CheckedChanged += chkLoop_CheckedChanged;
            // 
            // vuMeterRight
            // 
            vuMeterRight.BackColor = System.Drawing.SystemColors.ButtonShadow;
            vuMeterRight.Fps = 25;
            vuMeterRight.Location = new System.Drawing.Point(528, 142);
            vuMeterRight.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            vuMeterRight.MeterStyle = SharpMod.Win.UI.VuStyle.Wave;
            vuMeterRight.Name = "vuMeterRight";
            vuMeterRight.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            vuMeterRight.Size = new System.Drawing.Size(238, 177);
            vuMeterRight.TabIndex = 10;
            // 
            // vuMeterLeft
            // 
            vuMeterLeft.BackColor = System.Drawing.SystemColors.ButtonShadow;
            vuMeterLeft.Fps = 25;
            vuMeterLeft.Location = new System.Drawing.Point(283, 142);
            vuMeterLeft.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            vuMeterLeft.MeterStyle = SharpMod.Win.UI.VuStyle.Wave;
            vuMeterLeft.Name = "vuMeterLeft";
            vuMeterLeft.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            vuMeterLeft.Size = new System.Drawing.Size(238, 177);
            vuMeterLeft.TabIndex = 9;
            // 
            // listView1
            // 
            listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { columnHeader1 });
            listView1.Location = new System.Drawing.Point(112, 35);
            listView1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            listView1.MultiSelect = false;
            listView1.Name = "listView1";
            listView1.Size = new System.Drawing.Size(163, 284);
            listView1.TabIndex = 19;
            listView1.UseCompatibleStateImageBehavior = false;
            listView1.View = System.Windows.Forms.View.Details;
            listView1.SelectedIndexChanged += listView1_SelectedIndexChanged;
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(778, 331);
            Controls.Add(listView1);
            Controls.Add(chkLoop);
            Controls.Add(tbPos);
            Controls.Add(tbPatNo);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(tbModNfo);
            Controls.Add(BtnOpen);
            Controls.Add(vuMeterRight);
            Controls.Add(vuMeterLeft);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(btnPlayStop);
            Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            Name = "Form1";
            Text = "Form1";
            FormClosing += Form1FormClosing;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Button btnPlayStop;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton1;
        private SharpMod.Win.UI.VuMeter vuMeterLeft;
        private SharpMod.Win.UI.VuMeter vuMeterRight;
        private System.Windows.Forms.Button BtnOpen;
        private System.Windows.Forms.TextBox tbModNfo;
        private System.Windows.Forms.OpenFileDialog ofdModFile;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbPatNo;
        private System.Windows.Forms.TextBox tbPos;
        private System.Windows.Forms.CheckBox chkLoop;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
    }
}

