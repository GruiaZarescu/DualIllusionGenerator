using static VoxelGrid;

namespace DualIllusionGenerator
{
    public partial class Form1 : Form
    {
        private Stencil _loadedStencil;

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
                long ramMB = totalVoxels / 8 / 1024 / 1024; // Estimate RAM in MB
                string warningMsg = $"The requested model requires {totalVoxels.ToString("N0")} voxels (~{ramMB} MB RAM for the voxels alone, and some more for writing the file to disk.).\n" +
                                    $"This will create an STL file large in size, which may crash 3D printer slicers.\n\n" +
                                    $"Consider dropping the quality by just one level, it would use 8x less memory!" +
                                    $"Do you wish to continue?";

                DialogResult result = MessageBox.Show(warningMsg, "Extreme Size Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                {
                    return; // User heeded the warning
                }
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

                    try
                    {
                        // Move heavy CPU and Disk IO to a background thread so UI doesn't freeze!
                        await Task.Run(() =>
                        {
                            // 1. Initialize EMPTY grid (all 0s)
                            VoxelGrid grid = new VoxelGrid(voxelCountX, voxelCountY, voxelCountZ, voxelSizeMm);

                            // 2. If a stencil is loaded, extrude it straight down from the top
                            if (_loadedStencil != null)
                            {
                                grid.ApplyStencil(_loadedStencil, CarvePlane.Top, CarveOperation.Extrude);
                            }

                            // 3. Export to STL
                            VoxelToStlExporter.Export(grid, sfd.FileName);
                        });

                        MessageBox.Show($"Successfully exported {voxelCountX}x{voxelCountY}x{voxelCountZ} block to STL!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void btnLoadImage1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select Stencil Image";
                ofd.Filter = "Image Files|*.png;*.bmp;*.jpg";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Try to load normally (autoFix = false by default)
                        _loadedStencil = StencilManager.CreateFromImage(ofd.FileName);

                        MessageBox.Show($"Success! Stencil loaded and trimmed.\nDimensions: {_loadedStencil.Width} x {_loadedStencil.Height} pixels.",
                                        "Stencil Validated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (AntiAliasingException ex)
                    {
                        // Specifically catch the anti-aliasing error
                        DialogResult result = MessageBox.Show(ex.Message, "Anti-Aliasing Detected", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                        if (result == DialogResult.Yes)
                        {
                            try
                            {
                                // Turn on debug mode for now to see what it generates
                                _loadedStencil = StencilManager.CreateFromImage(ofd.FileName, autoFix: true, debugMode: true);

                                MessageBox.Show($"Success! Image auto-fixed and trimmed.\nDimensions: {_loadedStencil.Width} x {_loadedStencil.Height} pixels.",
                                                "Stencil Validated", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            catch (Exception ex2)
                            {
                                // If it fails *after* fixing (e.g. multiple contours), show error
                                MessageBox.Show(ex2.Message, "Image Validation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // Catch all other errors (no transparency, multiple contours, etc.)
                        MessageBox.Show(ex.Message, "Image Validation Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}
