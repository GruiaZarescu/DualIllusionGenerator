namespace DualIllusionGenerator
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnExport = new Button();
            VoxelDensityGroupBox = new GroupBox();
            rbDensityUltra = new RadioButton();
            rbDensityVeryHigh = new RadioButton();
            rbDensityHigh = new RadioButton();
            rbDensityMedium = new RadioButton();
            rbDensityLow = new RadioButton();
            rbDensityVeryLow = new RadioButton();
            CubeDimensionsGroupBox = new GroupBox();
            nudSizeZ = new NumericUpDown();
            nudSizeY = new NumericUpDown();
            nudSizeX = new NumericUpDown();
            labelHeight = new Label();
            labelDepth = new Label();
            labelWidth = new Label();
            tabModeSelector = new TabControl();
            tabDualText = new TabPage();
            lblFont2 = new Label();
            lblFont1 = new Label();
            btnFont2 = new Button();
            btnFont1 = new Button();
            txtText2 = new TextBox();
            txtText1 = new TextBox();
            labelText2 = new Label();
            labelText1 = new Label();
            tabDualImage = new TabPage();
            groupBox2 = new GroupBox();
            lblImg2Font = new Label();
            btnImg2Font = new Button();
            txtImg2Text = new TextBox();
            rbImg2Text = new RadioButton();
            rbImg2Image = new RadioButton();
            nudOffY2 = new NumericUpDown();
            chkStretch2 = new CheckBox();
            nudOffX2 = new NumericUpDown();
            label2 = new Label();
            nudPad2 = new NumericUpDown();
            label6 = new Label();
            cbAction2 = new ComboBox();
            label7 = new Label();
            lblImg2Status = new Label();
            label8 = new Label();
            btnLoadImage2 = new Button();
            groupBox1 = new GroupBox();
            lblImg1Font = new Label();
            btnImg1Font = new Button();
            txtImg1Text = new TextBox();
            rbImg1Text = new RadioButton();
            rbImg1Image = new RadioButton();
            nudOffY1 = new NumericUpDown();
            nudOffX1 = new NumericUpDown();
            nudPad1 = new NumericUpDown();
            label5 = new Label();
            label4 = new Label();
            label3 = new Label();
            chkStretch1 = new CheckBox();
            label1 = new Label();
            cbAction1 = new ComboBox();
            lblImg1Status = new Label();
            btnLoadImage1 = new Button();
            panelPreview = new Panel();
            VoxelDensityGroupBox.SuspendLayout();
            CubeDimensionsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudSizeZ).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudSizeY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudSizeX).BeginInit();
            tabModeSelector.SuspendLayout();
            tabDualText.SuspendLayout();
            tabDualImage.SuspendLayout();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudOffY2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudOffX2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudPad2).BeginInit();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudOffY1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudOffX1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudPad1).BeginInit();
            SuspendLayout();
            // 
            // btnExport
            // 
            btnExport.Location = new Point(374, 483);
            btnExport.Name = "btnExport";
            btnExport.Size = new Size(115, 62);
            btnExport.TabIndex = 0;
            btnExport.Text = "Export To STL";
            btnExport.UseVisualStyleBackColor = true;
            btnExport.Click += btnExport_Click;
            // 
            // VoxelDensityGroupBox
            // 
            VoxelDensityGroupBox.Controls.Add(rbDensityUltra);
            VoxelDensityGroupBox.Controls.Add(rbDensityVeryHigh);
            VoxelDensityGroupBox.Controls.Add(rbDensityHigh);
            VoxelDensityGroupBox.Controls.Add(rbDensityMedium);
            VoxelDensityGroupBox.Controls.Add(rbDensityLow);
            VoxelDensityGroupBox.Controls.Add(rbDensityVeryLow);
            VoxelDensityGroupBox.Location = new Point(672, 12);
            VoxelDensityGroupBox.Name = "VoxelDensityGroupBox";
            VoxelDensityGroupBox.Size = new Size(200, 184);
            VoxelDensityGroupBox.TabIndex = 1;
            VoxelDensityGroupBox.TabStop = false;
            VoxelDensityGroupBox.Text = "Voxel Density";
            // 
            // rbDensityUltra
            // 
            rbDensityUltra.AutoSize = true;
            rbDensityUltra.Location = new Point(6, 159);
            rbDensityUltra.Name = "rbDensityUltra";
            rbDensityUltra.Size = new Size(50, 19);
            rbDensityUltra.TabIndex = 5;
            rbDensityUltra.TabStop = true;
            rbDensityUltra.Text = "Ultra";
            rbDensityUltra.UseVisualStyleBackColor = true;
            // 
            // rbDensityVeryHigh
            // 
            rbDensityVeryHigh.AutoSize = true;
            rbDensityVeryHigh.Location = new Point(6, 134);
            rbDensityVeryHigh.Name = "rbDensityVeryHigh";
            rbDensityVeryHigh.Size = new Size(76, 19);
            rbDensityVeryHigh.TabIndex = 4;
            rbDensityVeryHigh.TabStop = true;
            rbDensityVeryHigh.Text = "Very High";
            rbDensityVeryHigh.UseVisualStyleBackColor = true;
            // 
            // rbDensityHigh
            // 
            rbDensityHigh.AutoSize = true;
            rbDensityHigh.Location = new Point(6, 109);
            rbDensityHigh.Name = "rbDensityHigh";
            rbDensityHigh.Size = new Size(51, 19);
            rbDensityHigh.TabIndex = 3;
            rbDensityHigh.TabStop = true;
            rbDensityHigh.Text = "High";
            rbDensityHigh.UseVisualStyleBackColor = true;
            // 
            // rbDensityMedium
            // 
            rbDensityMedium.AutoSize = true;
            rbDensityMedium.Location = new Point(6, 84);
            rbDensityMedium.Name = "rbDensityMedium";
            rbDensityMedium.Size = new Size(70, 19);
            rbDensityMedium.TabIndex = 2;
            rbDensityMedium.TabStop = true;
            rbDensityMedium.Text = "Medium";
            rbDensityMedium.UseVisualStyleBackColor = true;
            // 
            // rbDensityLow
            // 
            rbDensityLow.AutoSize = true;
            rbDensityLow.Location = new Point(6, 59);
            rbDensityLow.Name = "rbDensityLow";
            rbDensityLow.Size = new Size(47, 19);
            rbDensityLow.TabIndex = 1;
            rbDensityLow.TabStop = true;
            rbDensityLow.Text = "Low";
            rbDensityLow.UseVisualStyleBackColor = true;
            // 
            // rbDensityVeryLow
            // 
            rbDensityVeryLow.AutoSize = true;
            rbDensityVeryLow.Location = new Point(6, 34);
            rbDensityVeryLow.Name = "rbDensityVeryLow";
            rbDensityVeryLow.Size = new Size(72, 19);
            rbDensityVeryLow.TabIndex = 0;
            rbDensityVeryLow.TabStop = true;
            rbDensityVeryLow.Text = "Very Low";
            rbDensityVeryLow.UseVisualStyleBackColor = true;
            rbDensityVeryLow.CheckedChanged += radioButton1_CheckedChanged;
            // 
            // CubeDimensionsGroupBox
            // 
            CubeDimensionsGroupBox.Controls.Add(nudSizeZ);
            CubeDimensionsGroupBox.Controls.Add(nudSizeY);
            CubeDimensionsGroupBox.Controls.Add(nudSizeX);
            CubeDimensionsGroupBox.Controls.Add(labelHeight);
            CubeDimensionsGroupBox.Controls.Add(labelDepth);
            CubeDimensionsGroupBox.Controls.Add(labelWidth);
            CubeDimensionsGroupBox.Location = new Point(466, 12);
            CubeDimensionsGroupBox.Name = "CubeDimensionsGroupBox";
            CubeDimensionsGroupBox.Size = new Size(200, 184);
            CubeDimensionsGroupBox.TabIndex = 2;
            CubeDimensionsGroupBox.TabStop = false;
            CubeDimensionsGroupBox.Text = "Box Dimensions(mm)";
            // 
            // nudSizeZ
            // 
            nudSizeZ.Location = new Point(64, 105);
            nudSizeZ.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            nudSizeZ.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudSizeZ.Name = "nudSizeZ";
            nudSizeZ.Size = new Size(120, 23);
            nudSizeZ.TabIndex = 5;
            nudSizeZ.Value = new decimal(new int[] { 50, 0, 0, 0 });
            nudSizeZ.ValueChanged += numericUpDown3_ValueChanged;
            // 
            // nudSizeY
            // 
            nudSizeY.Location = new Point(64, 72);
            nudSizeY.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            nudSizeY.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudSizeY.Name = "nudSizeY";
            nudSizeY.Size = new Size(120, 23);
            nudSizeY.TabIndex = 4;
            nudSizeY.Value = new decimal(new int[] { 50, 0, 0, 0 });
            // 
            // nudSizeX
            // 
            nudSizeX.Location = new Point(64, 34);
            nudSizeX.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            nudSizeX.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudSizeX.Name = "nudSizeX";
            nudSizeX.Size = new Size(120, 23);
            nudSizeX.TabIndex = 3;
            nudSizeX.Value = new decimal(new int[] { 200, 0, 0, 0 });
            // 
            // labelHeight
            // 
            labelHeight.AutoSize = true;
            labelHeight.Location = new Point(4, 107);
            labelHeight.Name = "labelHeight";
            labelHeight.Size = new Size(58, 15);
            labelHeight.TabIndex = 2;
            labelHeight.Text = "Height(Z)";
            // 
            // labelDepth
            // 
            labelDepth.AutoSize = true;
            labelDepth.Location = new Point(4, 74);
            labelDepth.Name = "labelDepth";
            labelDepth.Size = new Size(54, 15);
            labelDepth.TabIndex = 1;
            labelDepth.Text = "Depth(Y)";
            labelDepth.Click += label1_Click_1;
            // 
            // labelWidth
            // 
            labelWidth.AutoSize = true;
            labelWidth.Location = new Point(4, 36);
            labelWidth.Name = "labelWidth";
            labelWidth.Size = new Size(54, 15);
            labelWidth.TabIndex = 0;
            labelWidth.Text = "Width(X)";
            labelWidth.Click += label1_Click;
            // 
            // tabModeSelector
            // 
            tabModeSelector.Controls.Add(tabDualText);
            tabModeSelector.Controls.Add(tabDualImage);
            tabModeSelector.Location = new Point(43, 222);
            tabModeSelector.Name = "tabModeSelector";
            tabModeSelector.SelectedIndex = 0;
            tabModeSelector.Size = new Size(803, 255);
            tabModeSelector.TabIndex = 0;
            // 
            // tabDualText
            // 
            tabDualText.Controls.Add(lblFont2);
            tabDualText.Controls.Add(lblFont1);
            tabDualText.Controls.Add(btnFont2);
            tabDualText.Controls.Add(btnFont1);
            tabDualText.Controls.Add(txtText2);
            tabDualText.Controls.Add(txtText1);
            tabDualText.Controls.Add(labelText2);
            tabDualText.Controls.Add(labelText1);
            tabDualText.Location = new Point(4, 24);
            tabDualText.Name = "tabDualText";
            tabDualText.Padding = new Padding(3);
            tabDualText.Size = new Size(795, 227);
            tabDualText.TabIndex = 0;
            tabDualText.Text = "Dual Text";
            tabDualText.UseVisualStyleBackColor = true;
            // 
            // lblFont2
            // 
            lblFont2.AutoSize = true;
            lblFont2.Location = new Point(328, 84);
            lblFont2.Name = "lblFont2";
            lblFont2.Size = new Size(44, 15);
            lblFont2.TabIndex = 11;
            lblFont2.Text = "label10";
            // 
            // lblFont1
            // 
            lblFont1.AutoSize = true;
            lblFont1.Location = new Point(324, 47);
            lblFont1.Name = "lblFont1";
            lblFont1.Size = new Size(38, 15);
            lblFont1.TabIndex = 10;
            lblFont1.Text = "label9";
            // 
            // btnFont2
            // 
            btnFont2.Location = new Point(220, 81);
            btnFont2.Name = "btnFont2";
            btnFont2.Size = new Size(75, 23);
            btnFont2.TabIndex = 9;
            btnFont2.Text = "Font 2";
            btnFont2.UseVisualStyleBackColor = true;
            btnFont2.Click += btnFont2_Click;
            // 
            // btnFont1
            // 
            btnFont1.Location = new Point(220, 45);
            btnFont1.Name = "btnFont1";
            btnFont1.Size = new Size(75, 23);
            btnFont1.TabIndex = 8;
            btnFont1.Text = "Font 1";
            btnFont1.UseVisualStyleBackColor = true;
            btnFont1.Click += btnFont1_Click;
            // 
            // txtText2
            // 
            txtText2.Location = new Point(84, 81);
            txtText2.Name = "txtText2";
            txtText2.Size = new Size(100, 23);
            txtText2.TabIndex = 7;
            // 
            // txtText1
            // 
            txtText1.Location = new Point(84, 45);
            txtText1.Name = "txtText1";
            txtText1.Size = new Size(100, 23);
            txtText1.TabIndex = 6;
            // 
            // labelText2
            // 
            labelText2.AutoSize = true;
            labelText2.Location = new Point(31, 84);
            labelText2.Name = "labelText2";
            labelText2.Size = new Size(37, 15);
            labelText2.TabIndex = 5;
            labelText2.Text = "Text 2";
            // 
            // labelText1
            // 
            labelText1.AutoSize = true;
            labelText1.Location = new Point(31, 48);
            labelText1.Name = "labelText1";
            labelText1.Size = new Size(37, 15);
            labelText1.TabIndex = 4;
            labelText1.Text = "Text 1";
            // 
            // tabDualImage
            // 
            tabDualImage.Controls.Add(groupBox2);
            tabDualImage.Controls.Add(groupBox1);
            tabDualImage.Location = new Point(4, 24);
            tabDualImage.Name = "tabDualImage";
            tabDualImage.Padding = new Padding(3);
            tabDualImage.Size = new Size(795, 227);
            tabDualImage.TabIndex = 1;
            tabDualImage.Text = "Dual Image";
            tabDualImage.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(lblImg2Font);
            groupBox2.Controls.Add(btnImg2Font);
            groupBox2.Controls.Add(txtImg2Text);
            groupBox2.Controls.Add(rbImg2Text);
            groupBox2.Controls.Add(rbImg2Image);
            groupBox2.Controls.Add(nudOffY2);
            groupBox2.Controls.Add(chkStretch2);
            groupBox2.Controls.Add(nudOffX2);
            groupBox2.Controls.Add(label2);
            groupBox2.Controls.Add(nudPad2);
            groupBox2.Controls.Add(label6);
            groupBox2.Controls.Add(cbAction2);
            groupBox2.Controls.Add(label7);
            groupBox2.Controls.Add(lblImg2Status);
            groupBox2.Controls.Add(label8);
            groupBox2.Controls.Add(btnLoadImage2);
            groupBox2.Location = new Point(378, 18);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(396, 206);
            groupBox2.TabIndex = 8;
            groupBox2.TabStop = false;
            groupBox2.Text = "Image 2 (Top Plane)";
            groupBox2.Enter += groupBox2_Enter;
            // 
            // lblImg2Font
            // 
            lblImg2Font.AutoSize = true;
            lblImg2Font.Location = new Point(291, 146);
            lblImg2Font.Name = "lblImg2Font";
            lblImg2Font.Size = new Size(0, 15);
            lblImg2Font.TabIndex = 20;
            lblImg2Font.Visible = false;
            // 
            // btnImg2Font
            // 
            btnImg2Font.Location = new Point(273, 109);
            btnImg2Font.Name = "btnImg2Font";
            btnImg2Font.Size = new Size(75, 23);
            btnImg2Font.TabIndex = 20;
            btnImg2Font.Text = "Font";
            btnImg2Font.UseVisualStyleBackColor = true;
            btnImg2Font.Visible = false;
            btnImg2Font.Click += btnImg2Font_Click;
            // 
            // txtImg2Text
            // 
            txtImg2Text.Location = new Point(253, 74);
            txtImg2Text.Name = "txtImg2Text";
            txtImg2Text.Size = new Size(117, 23);
            txtImg2Text.TabIndex = 20;
            txtImg2Text.Visible = false;
            // 
            // rbImg2Text
            // 
            rbImg2Text.AutoSize = true;
            rbImg2Text.Location = new Point(268, 43);
            rbImg2Text.Name = "rbImg2Text";
            rbImg2Text.Size = new Size(68, 19);
            rbImg2Text.TabIndex = 21;
            rbImg2Text.Text = "Use Text";
            rbImg2Text.UseVisualStyleBackColor = true;
            rbImg2Text.CheckedChanged += rbImg2Text_CheckedChanged;
            // 
            // rbImg2Image
            // 
            rbImg2Image.AutoSize = true;
            rbImg2Image.Checked = true;
            rbImg2Image.Location = new Point(268, 18);
            rbImg2Image.Name = "rbImg2Image";
            rbImg2Image.Size = new Size(80, 19);
            rbImg2Image.TabIndex = 17;
            rbImg2Image.TabStop = true;
            rbImg2Image.Text = "Use Image";
            rbImg2Image.UseVisualStyleBackColor = true;
            rbImg2Image.CheckedChanged += rbImg2Image_CheckedChanged;
            // 
            // nudOffY2
            // 
            nudOffY2.Location = new Point(100, 138);
            nudOffY2.Name = "nudOffY2";
            nudOffY2.Size = new Size(120, 23);
            nudOffY2.TabIndex = 20;
            // 
            // chkStretch2
            // 
            chkStretch2.AutoSize = true;
            chkStretch2.Location = new Point(15, 49);
            chkStretch2.Name = "chkStretch2";
            chkStretch2.Size = new Size(205, 19);
            chkStretch2.TabIndex = 9;
            chkStretch2.Text = "Stretch to fit (Ignore Aspect Ratio)";
            chkStretch2.UseVisualStyleBackColor = true;
            chkStretch2.CheckedChanged += chkStretch2_CheckedChanged;
            // 
            // nudOffX2
            // 
            nudOffX2.Location = new Point(100, 109);
            nudOffX2.Name = "nudOffX2";
            nudOffX2.Size = new Size(120, 23);
            nudOffX2.TabIndex = 19;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(15, 22);
            label2.Name = "label2";
            label2.Size = new Size(88, 15);
            label2.TabIndex = 7;
            label2.Text = "Operation Type";
            // 
            // nudPad2
            // 
            nudPad2.Location = new Point(100, 80);
            nudPad2.Maximum = new decimal(new int[] { 90, 0, 0, 0 });
            nudPad2.Name = "nudPad2";
            nudPad2.Size = new Size(120, 23);
            nudPad2.TabIndex = 18;
            nudPad2.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(15, 141);
            label6.Name = "label6";
            label6.Size = new Size(49, 15);
            label6.TabIndex = 17;
            label6.Text = "Offset Y";
            // 
            // cbAction2
            // 
            cbAction2.DropDownStyle = ComboBoxStyle.DropDownList;
            cbAction2.FormattingEnabled = true;
            cbAction2.Items.AddRange(new object[] { "Extrude", "Cut" });
            cbAction2.Location = new Point(109, 19);
            cbAction2.Name = "cbAction2";
            cbAction2.Size = new Size(121, 23);
            cbAction2.TabIndex = 6;
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Location = new Point(15, 113);
            label7.Name = "label7";
            label7.Size = new Size(49, 15);
            label7.TabIndex = 16;
            label7.Text = "Offset X";
            // 
            // lblImg2Status
            // 
            lblImg2Status.AutoSize = true;
            lblImg2Status.Location = new Point(176, 181);
            lblImg2Status.Name = "lblImg2Status";
            lblImg2Status.Size = new Size(60, 15);
            lblImg2Status.TabIndex = 5;
            lblImg2Status.Text = "imgStatus";
            lblImg2Status.Visible = false;
            lblImg2Status.Click += lblImg2Status_Click;
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(15, 82);
            label8.Name = "label8";
            label8.Size = new Size(64, 15);
            label8.TabIndex = 15;
            label8.Text = "Padding %";
            // 
            // btnLoadImage2
            // 
            btnLoadImage2.Location = new Point(6, 177);
            btnLoadImage2.Name = "btnLoadImage2";
            btnLoadImage2.Size = new Size(134, 23);
            btnLoadImage2.TabIndex = 4;
            btnLoadImage2.Text = "Load Image 2";
            btnLoadImage2.UseVisualStyleBackColor = true;
            btnLoadImage2.Click += btnLoadImage2_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(lblImg1Font);
            groupBox1.Controls.Add(btnImg1Font);
            groupBox1.Controls.Add(txtImg1Text);
            groupBox1.Controls.Add(rbImg1Text);
            groupBox1.Controls.Add(rbImg1Image);
            groupBox1.Controls.Add(nudOffY1);
            groupBox1.Controls.Add(nudOffX1);
            groupBox1.Controls.Add(nudPad1);
            groupBox1.Controls.Add(label5);
            groupBox1.Controls.Add(label4);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(chkStretch1);
            groupBox1.Controls.Add(label1);
            groupBox1.Controls.Add(cbAction1);
            groupBox1.Controls.Add(lblImg1Status);
            groupBox1.Controls.Add(btnLoadImage1);
            groupBox1.Location = new Point(6, 15);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(366, 206);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Image 1 (Front Plane)";
            groupBox1.Enter += groupBox1_Enter_1;
            // 
            // lblImg1Font
            // 
            lblImg1Font.AutoSize = true;
            lblImg1Font.Location = new Point(257, 149);
            lblImg1Font.Name = "lblImg1Font";
            lblImg1Font.Size = new Size(0, 15);
            lblImg1Font.TabIndex = 19;
            lblImg1Font.Visible = false;
            // 
            // btnImg1Font
            // 
            btnImg1Font.Location = new Point(243, 112);
            btnImg1Font.Name = "btnImg1Font";
            btnImg1Font.Size = new Size(75, 23);
            btnImg1Font.TabIndex = 18;
            btnImg1Font.Text = "Font";
            btnImg1Font.UseVisualStyleBackColor = true;
            btnImg1Font.Visible = false;
            btnImg1Font.Click += btnImg1Font_Click;
            // 
            // txtImg1Text
            // 
            txtImg1Text.Location = new Point(229, 80);
            txtImg1Text.Name = "txtImg1Text";
            txtImg1Text.Size = new Size(117, 23);
            txtImg1Text.TabIndex = 17;
            txtImg1Text.Visible = false;
            txtImg1Text.TextChanged += txtImg1Text_TextChanged;
            // 
            // rbImg1Text
            // 
            rbImg1Text.AutoSize = true;
            rbImg1Text.Location = new Point(243, 50);
            rbImg1Text.Name = "rbImg1Text";
            rbImg1Text.Size = new Size(68, 19);
            rbImg1Text.TabIndex = 16;
            rbImg1Text.Text = "Use Text";
            rbImg1Text.UseVisualStyleBackColor = true;
            rbImg1Text.CheckedChanged += rbImg1Text_CheckedChanged;
            // 
            // rbImg1Image
            // 
            rbImg1Image.AutoSize = true;
            rbImg1Image.Checked = true;
            rbImg1Image.Location = new Point(243, 25);
            rbImg1Image.Name = "rbImg1Image";
            rbImg1Image.Size = new Size(80, 19);
            rbImg1Image.TabIndex = 15;
            rbImg1Image.TabStop = true;
            rbImg1Image.Text = "Use Image";
            rbImg1Image.UseVisualStyleBackColor = true;
            rbImg1Image.CheckedChanged += rbImg1Image_CheckedChanged;
            // 
            // nudOffY1
            // 
            nudOffY1.Location = new Point(97, 139);
            nudOffY1.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            nudOffY1.Minimum = new decimal(new int[] { 500, 0, 0, int.MinValue });
            nudOffY1.Name = "nudOffY1";
            nudOffY1.Size = new Size(120, 23);
            nudOffY1.TabIndex = 14;
            // 
            // nudOffX1
            // 
            nudOffX1.Location = new Point(97, 110);
            nudOffX1.Maximum = new decimal(new int[] { 500, 0, 0, 0 });
            nudOffX1.Minimum = new decimal(new int[] { 500, 0, 0, int.MinValue });
            nudOffX1.Name = "nudOffX1";
            nudOffX1.Size = new Size(120, 23);
            nudOffX1.TabIndex = 13;
            // 
            // nudPad1
            // 
            nudPad1.Location = new Point(97, 81);
            nudPad1.Maximum = new decimal(new int[] { 90, 0, 0, 0 });
            nudPad1.Name = "nudPad1";
            nudPad1.Size = new Size(120, 23);
            nudPad1.TabIndex = 12;
            nudPad1.Value = new decimal(new int[] { 10, 0, 0, 0 });
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Location = new Point(12, 142);
            label5.Name = "label5";
            label5.Size = new Size(49, 15);
            label5.TabIndex = 11;
            label5.Text = "Offset Y";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Location = new Point(12, 114);
            label4.Name = "label4";
            label4.Size = new Size(49, 15);
            label4.TabIndex = 10;
            label4.Text = "Offset X";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(12, 83);
            label3.Name = "label3";
            label3.Size = new Size(64, 15);
            label3.TabIndex = 9;
            label3.Text = "Padding %";
            // 
            // chkStretch1
            // 
            chkStretch1.AutoSize = true;
            chkStretch1.Location = new Point(12, 52);
            chkStretch1.Name = "chkStretch1";
            chkStretch1.Size = new Size(205, 19);
            chkStretch1.TabIndex = 8;
            chkStretch1.Text = "Stretch to fit (Ignore Aspect Ratio)";
            chkStretch1.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 25);
            label1.Name = "label1";
            label1.Size = new Size(88, 15);
            label1.TabIndex = 7;
            label1.Text = "Operation Type";
            // 
            // cbAction1
            // 
            cbAction1.DropDownStyle = ComboBoxStyle.DropDownList;
            cbAction1.FormattingEnabled = true;
            cbAction1.Items.AddRange(new object[] { "Extrude", "Cut" });
            cbAction1.Location = new Point(100, 22);
            cbAction1.Name = "cbAction1";
            cbAction1.Size = new Size(118, 23);
            cbAction1.TabIndex = 6;
            cbAction1.SelectedIndexChanged += cbAction1_SelectedIndexChanged;
            // 
            // lblImg1Status
            // 
            lblImg1Status.AutoSize = true;
            lblImg1Status.Location = new Point(176, 181);
            lblImg1Status.Name = "lblImg1Status";
            lblImg1Status.Size = new Size(60, 15);
            lblImg1Status.TabIndex = 5;
            lblImg1Status.Text = "imgStatus";
            lblImg1Status.Visible = false;
            // 
            // btnLoadImage1
            // 
            btnLoadImage1.Location = new Point(6, 177);
            btnLoadImage1.Name = "btnLoadImage1";
            btnLoadImage1.Size = new Size(134, 23);
            btnLoadImage1.TabIndex = 4;
            btnLoadImage1.Text = "Load Image 1";
            btnLoadImage1.UseVisualStyleBackColor = true;
            btnLoadImage1.Click += btnLoadImage1_Click;
            // 
            // panelPreview
            // 
            panelPreview.Location = new Point(58, 29);
            panelPreview.Name = "panelPreview";
            panelPreview.Size = new Size(341, 161);
            panelPreview.TabIndex = 3;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(860, 561);
            Controls.Add(panelPreview);
            Controls.Add(tabModeSelector);
            Controls.Add(CubeDimensionsGroupBox);
            Controls.Add(VoxelDensityGroupBox);
            Controls.Add(btnExport);
            Name = "Form1";
            Text = "Form1";
            VoxelDensityGroupBox.ResumeLayout(false);
            VoxelDensityGroupBox.PerformLayout();
            CubeDimensionsGroupBox.ResumeLayout(false);
            CubeDimensionsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudSizeZ).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudSizeY).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudSizeX).EndInit();
            tabModeSelector.ResumeLayout(false);
            tabDualText.ResumeLayout(false);
            tabDualText.PerformLayout();
            tabDualImage.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudOffY2).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudOffX2).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudPad2).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)nudOffY1).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudOffX1).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudPad1).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Button btnExport;
        private GroupBox VoxelDensityGroupBox;
        private RadioButton rbDensityVeryLow;
        private RadioButton rbDensityUltra;
        private RadioButton rbDensityVeryHigh;
        private RadioButton rbDensityHigh;
        private RadioButton rbDensityMedium;
        private RadioButton rbDensityLow;
        private GroupBox CubeDimensionsGroupBox;
        private Label labelWidth;
        private Label labelHeight;
        private Label labelDepth;
        private NumericUpDown nudSizeZ;
        private NumericUpDown nudSizeY;
        private NumericUpDown nudSizeX;
        private TabControl tabModeSelector;
        private TabPage tabDualText;
        private TabPage tabDualImage;
        private TextBox txtText2;
        private TextBox txtText1;
        private Label labelText2;
        private Label labelText1;
        private GroupBox groupBox1;
        private Label lblImg1Status;
        private Button btnLoadImage1;
        private ComboBox cbAction1;
        private Label label1;
        private GroupBox groupBox2;
        private Label label2;
        private ComboBox cbAction2;
        private Label lblImg2Status;
        private Button btnLoadImage2;
        private CheckBox chkStretch1;
        private CheckBox chkStretch2;
        private Label label5;
        private Label label4;
        private Label label3;
        private NumericUpDown nudOffY2;
        private NumericUpDown nudOffX2;
        private NumericUpDown nudPad2;
        private Label label6;
        private Label label7;
        private Label label8;
        private NumericUpDown nudOffY1;
        private NumericUpDown nudOffX1;
        private NumericUpDown nudPad1;
        private Button btnFont2;
        private Button btnFont1;
        private Label lblFont2;
        private Label lblFont1;
        private Panel panelPreview;
        private RadioButton rbImg2Text;
        private RadioButton rbImg2Image;
        private RadioButton rbImg1Text;
        private RadioButton rbImg1Image;
        private Button btnImg1Font;
        private TextBox txtImg1Text;
        private Label lblImg1Font;
        private Label lblImg2Font;
        private Button btnImg2Font;
        private TextBox txtImg2Text;
    }
}
