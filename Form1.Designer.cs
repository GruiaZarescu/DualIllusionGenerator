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
            btnLoadImage1 = new Button();
            VoxelDensityGroupBox.SuspendLayout();
            CubeDimensionsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)nudSizeZ).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudSizeY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudSizeX).BeginInit();
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
            // btnLoadImage1
            // 
            btnLoadImage1.Location = new Point(292, 167);
            btnLoadImage1.Name = "btnLoadImage1";
            btnLoadImage1.Size = new Size(134, 23);
            btnLoadImage1.TabIndex = 3;
            btnLoadImage1.Text = "Load Image 1";
            btnLoadImage1.UseVisualStyleBackColor = true;
            btnLoadImage1.Click += btnLoadImage1_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(884, 557);
            Controls.Add(btnLoadImage1);
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
        private Button btnLoadImage1;
    }
}
