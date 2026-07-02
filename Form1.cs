using static VoxelGrid;

namespace DualIllusionGenerator
{
    public partial class Form1 : Form
    {
        private Stencil _stencil1;
        private Stencil _stencil2;
        private Font _font1 = new Font("Arial", 12, FontStyle.Regular);
        private Font _font2 = new Font("Arial", 12, FontStyle.Regular);

        public Form1()
        {
            InitializeComponent();
        }

        private async void btnExport_Click(object sender, EventArgs e)
        {
            // 1. Determine Voxel Size in mm based on RadioButtons
            float voxelSizeMm = 0f;

            if (rbDensityVeryLow.Checked) voxelSizeMm = 10.0f;
            else if (rbDensityLow.Checked) voxelSizeMm = 5.0f;
            else if (rbDensityMedium.Checked) voxelSizeMm = 2.5f;
            else if (rbDensityHigh.Checked) voxelSizeMm = 1.25f;
            else if (rbDensityVeryHigh.Checked) voxelSizeMm = 0.625f;
            else if (rbDensityUltra.Checked) voxelSizeMm = 0.3125f;

            if (voxelSizeMm == 0f)
            {
                MessageBox.Show("Please select a Voxel Density before exporting.", "Density Not Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Read desired physical dimensions in mm from the UI
            float targetSizeXMm = (float)nudSizeX.Value;
            float targetSizeYMm = (float)nudSizeY.Value;
            float targetSizeZMm = (float)nudSizeZ.Value;

            // 3. Calculate number of voxels needed
            int voxelCountX = (int)Math.Floor(targetSizeXMm / voxelSizeMm);
            int voxelCountY = (int)Math.Floor(targetSizeYMm / voxelSizeMm);
            int voxelCountZ = (int)Math.Floor(targetSizeZMm / voxelSizeMm);

            if (voxelCountX < 1 || voxelCountY < 1 || voxelCountZ < 1)
            {
                MessageBox.Show("Dimensions are too small for the selected voxel density.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            long totalVoxels = (long)voxelCountX * voxelCountY * voxelCountZ;
            long maxVoxels = 2000000000; // 2 billion voxels soft limit 

            if (totalVoxels > maxVoxels)
            {
                long ramMB = totalVoxels / 8 / 1024 / 1024;
                string warningMsg = $"The requested model requires {totalVoxels.ToString("N0")} voxels (~{ramMB} MB RAM for the voxels alone, and some more for writing the file to disk.).\n" +
                                    $"This will create an STL file large in size, which may crash 3D printer slicers.\n\n" +
                                    $"Consider dropping the quality by just one level, it would use 8x less memory!" +
                                    $"Do you wish to continue?";

                DialogResult result = MessageBox.Show(warningMsg, "Extreme Size Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                {
                    return;
                }
            }

            // VALIDATION AND UI READING
            bool isDualImageMode = (tabModeSelector.SelectedTab == tabDualImage);

            // Variables for Dual Image Mode
            CarveOperation op1 = CarveOperation.Extrude;
            CarveOperation op2 = CarveOperation.Cut;
            bool stretch1 = false, stretch2 = false;
            float pad1 = 0, pad2 = 0, offX1 = 0, offY1 = 0, offX2 = 0, offY2 = 0;
            Stencil localStencil1 = null;
            Stencil localStencil2 = null;

            // Variables for Dual Text Mode
            string text1 = "";
            string text2 = "";
            Font localFont1 = null;
            Font localFont2 = null;

            if (isDualImageMode)
            {
                if (cbAction1.SelectedItem?.ToString() == cbAction2.SelectedItem?.ToString())
                {
                    MessageBox.Show("Actions for Image 1 and Image 2 cannot be the same. One must Extrude and one must Cut.", "Action Conflict", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (_stencil1 == null || _stencil2 == null)
                {
                    MessageBox.Show("Please load both Image 1 and Image 2.", "Missing Images", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                op1 = cbAction1.SelectedItem?.ToString() == "Extrude" ? CarveOperation.Extrude : CarveOperation.Cut;
                op2 = cbAction2.SelectedItem?.ToString() == "Extrude" ? CarveOperation.Extrude : CarveOperation.Cut;

                stretch1 = chkStretch1.Checked;
                stretch2 = chkStretch2.Checked;
                pad1 = (float)nudPad1.Value;
                pad2 = (float)nudPad2.Value;
                offX1 = (float)nudOffX1.Value;
                offY1 = (float)nudOffY1.Value;
                offX2 = (float)nudOffX2.Value;
                offY2 = (float)nudOffY2.Value;

                localStencil1 = _stencil1;
                localStencil2 = _stencil2;
            }
            else
            {
                text1 = txtText1.Text.ToUpper();
                text2 = txtText2.Text.ToUpper();
                localFont1 = _font1;
                localFont2 = _font2;
            }

            // Ask user where to save the file
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "STL Files|*.stl";
                sfd.Title = "Save Voxel STL";
                sfd.FileName = "VoxelBlock.stl";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    btnExport.Text = "Generating...";
                    btnExport.Enabled = false;

                    string filePath = sfd.FileName;

                    try
                    {
                        await Task.Run(() =>
                        {
                            VoxelGrid grid = new VoxelGrid(voxelCountX, voxelCountY, voxelCountZ, voxelSizeMm);

                            if (isDualImageMode)
                            {
                                // BOOLEAN LOGIC: Extrudes MUST happen before Cuts!
                                if (op1 == CarveOperation.Extrude)
                                    grid.ApplyStencil(localStencil1, CarvePlane.Front, CarveOperation.Extrude, stretch1, pad1, offX1, offY1);
                                if (op2 == CarveOperation.Extrude)
                                    grid.ApplyStencil(localStencil2, CarvePlane.Top, CarveOperation.Extrude, stretch2, pad2, offX2, offY2);

                                if (op1 == CarveOperation.Cut)
                                    grid.ApplyStencil(localStencil1, CarvePlane.Front, CarveOperation.Cut, stretch1, pad1, offX1, offY1);
                                if (op2 == CarveOperation.Cut)
                                    grid.ApplyStencil(localStencil2, CarvePlane.Top, CarveOperation.Cut, stretch2, pad2, offX2, offY2);
                            }
                            else
                            {
                                // Dual Text Mode
                                List<Stencil> stencils1 = TextManager.CreateStencilsFromText(text1, localFont1);
                                List<Stencil> stencils2 = TextManager.CreateStencilsFromText(text2, localFont2);

                                List<Stencil> extrudeStencils = stencils1.Count >= stencils2.Count ? stencils1 : stencils2;
                                List<Stencil> cutStencils = stencils1.Count >= stencils2.Count ? stencils2 : stencils1;

                                int totalSlots = extrudeStencils.Count;
                                int cutOffset = (totalSlots - cutStencils.Count) / 2;

                                // 1. Find the maximum letter width and height across BOTH texts
                                float maxLetterWidth = 0;
                                float maxStencilHeight = 0;
                                foreach (var s in extrudeStencils)
                                {
                                    if (s.Width > maxLetterWidth) maxLetterWidth = s.Width;
                                    if (s.Height > maxStencilHeight) maxStencilHeight = s.Height;
                                }
                                foreach (var s in cutStencils)
                                {
                                    if (s.Width > maxLetterWidth) maxLetterWidth = s.Width;
                                    if (s.Height > maxStencilHeight) maxStencilHeight = s.Height;
                                }

                                // 2. Calculate uniform scale
                                float slotWidth = (float)voxelCountX / totalSlots;
                                float cos45 = (float)Math.Cos(45.0f * Math.PI / 180.0f); // ~0.707f

                                // We want the text plane's X-projection to take up 80% of the slot width
                                // X-projection = maxLetterWidth * uniformScale * cos(45)
                                float scaleX = (slotWidth * 0.8f) / (maxLetterWidth * cos45);
                                float scaleZ = (voxelCountZ * 0.9f) / maxStencilHeight;
                                float uniformScale = Math.Min(scaleX, scaleZ);

                                // 3. Calculate slot centers
                                List<float> slotCenters = new List<float>();
                                for (int i = 0; i < totalSlots; i++)
                                {
                                    float slotCenterX = slotWidth * (i + 0.5f);
                                    slotCenters.Add(slotCenterX);
                                }

                                // 4. Apply stencils
                                // Extrude first, and remember each letter's exact occupied X-range —
                                // that's what the paired Intersect call needs to stay confined correctly.
                                List<VoxelGrid.LetterBounds> extrudeBoundsList = new List<VoxelGrid.LetterBounds>();
                                for (int i = 0; i < extrudeStencils.Count; i++)
                                {
                                    var bounds = grid.ComputeLetterBounds(extrudeStencils[i], -45.0f, uniformScale, slotWidth);
                                    extrudeBoundsList.Add(bounds);
                                    grid.ApplyTextStencil(extrudeStencils[i], -45.0f, CarveOperation.Extrude, slotCenters[i], uniformScale, slotWidth, bounds.HalfWidthX);
                                }

                                for (int i = 0; i < cutStencils.Count; i++)
                                {
                                    int targetSlot = i + cutOffset;
                                    // Confine strictly to the region the paired Extrude letter actually occupies.
                                    grid.ApplyTextStencil(cutStencils[i], 45.0f, CarveOperation.Intersect, slotCenters[targetSlot], uniformScale, slotWidth, extrudeBoundsList[targetSlot].HalfWidthX);
                                }
                            }

                            VoxelToStlExporter.Export(grid, filePath);
                        });

                        MessageBox.Show("Successfully exported to STL!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error exporting STL: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        btnExport.Text = "Export to STL";
                        btnExport.Enabled = true;
                    }
                }
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {

        }


        private void cbAction1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void btnLoadImage2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select Stencil Image 2";
                ofd.Filter = "Image Files|*.png;*.bmp;*.jpg";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _stencil2 = StencilManager.CreateFromImage(ofd.FileName);
                        lblImg2Status.Text = $"Loaded: {_stencil2.Width}x{_stencil2.Height}";
                    }
                    catch (AntiAliasingException ex)
                    {
                        if (MessageBox.Show(ex.Message, "Anti-Aliasing Detected", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                        {
                            _stencil2 = StencilManager.CreateFromImage(ofd.FileName, autoFix: true);
                            lblImg2Status.Text = $"Loaded (Auto-Fixed): {_stencil2.Width}x{_stencil2.Height}";
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnLoadImage1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select Stencil Image 1";
                ofd.Filter = "Image Files|*.png;*.bmp;*.jpg";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _stencil1 = StencilManager.CreateFromImage(ofd.FileName);
                        lblImg1Status.Text = $"Loaded: {_stencil1.Width}x{_stencil1.Height}";
                    }
                    catch (AntiAliasingException ex)
                    {
                        if (MessageBox.Show(ex.Message, "Anti-Aliasing Detected", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                        {
                            _stencil1 = StencilManager.CreateFromImage(ofd.FileName, autoFix: true);
                            lblImg1Status.Text = $"Loaded (Auto-Fixed): {_stencil1.Width}x{_stencil1.Height}";
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void txtText1_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtText2_TextChanged(object sender, EventArgs e)
        {

        }

        private void labelText1_Click(object sender, EventArgs e)
        {

        }

        private void tabDualText_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void btnFont1_Click(object sender, EventArgs e)
        {
            using (FontDialog fd = new FontDialog())
            {
                fd.Font = _font1;
                if (fd.ShowDialog() == DialogResult.OK)
                {
                    _font1 = fd.Font;
                    lblFont1.Text = $"{_font1.Name} ({_font1.Size})";
                }
            }
        }

        private void btnFont2_Click(object sender, EventArgs e)
        {
            using (FontDialog fd = new FontDialog())
            {
                fd.Font = _font2;
                if (fd.ShowDialog() == DialogResult.OK)
                {
                    _font2 = fd.Font;
                    lblFont2.Text = $"{_font2.Name} ({_font2.Size})";
                }
            }
        }
    }
}
