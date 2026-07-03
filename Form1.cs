using static VoxelGrid;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using HelixToolkit.Wpf;

namespace DualIllusionGenerator
{
    public partial class Form1 : Form
    {
        private Stencil _stencil1;
        private Stencil _stencil2;
        private Font _font1 = new Font("Arial", 12, FontStyle.Regular);
        private Font _font2 = new Font("Arial", 12, FontStyle.Regular);

        private HelixViewport3D _viewport;
        private ModelVisual3D _lettersModel;
        private VoxelGrid _lastPreviewGrid;
        private System.Windows.Forms.Timer _previewDebounce;
        private CancellationTokenSource _previewCts;

        public Form1()
        {
            InitializeComponent();
            SetupPreview();
            WireUpPreviewTriggers();
        }

        // ─── PREVIEW SETUP ────────────────────────────────────────────────────────

        private void SetupPreview()
        {
            _viewport = new HelixViewport3D
            {
                Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 40, 40)),
                ShowCoordinateSystem = false,
                ShowViewCube = true,
                ZoomExtentsWhenLoaded = true,
            };
            _viewport.Children.Add(new SunLight());

            var host = new ElementHost { Dock = DockStyle.Fill, Child = _viewport };
            panelPreview.Controls.Add(host);

            _previewDebounce = new System.Windows.Forms.Timer { Interval = 400 };
            _previewDebounce.Tick += (s, e) => { _previewDebounce.Stop(); _ = RegeneratePreviewAsync(); };
        }

        private void WireUpPreviewTriggers()
        {
            void Hook(System.Windows.Forms.Control c)
            {
                switch (c)
                {
                    case RadioButton rb: rb.CheckedChanged += OnPreviewSettingChanged; break;
                    case NumericUpDown nud: nud.ValueChanged += OnPreviewSettingChanged; break;
                    case System.Windows.Forms.TextBox tb: tb.TextChanged += OnPreviewSettingChanged; break;
                    case ComboBox cb: cb.SelectedIndexChanged += OnPreviewSettingChanged; break;
                    case System.Windows.Forms.CheckBox chk: chk.CheckedChanged += OnPreviewSettingChanged; break;
                    case TabControl tc: tc.SelectedIndexChanged += OnPreviewSettingChanged; break;
                }
                foreach (System.Windows.Forms.Control child in c.Controls) Hook(child);
            }
            Hook(this);
        }

        private void OnPreviewSettingChanged(object sender, EventArgs e)
        {
            _previewDebounce.Stop();
            _previewDebounce.Start();
        }

        private async Task RegeneratePreviewAsync()
        {
            _lastPreviewGrid = null; // invalidate immediately so a racing export doesn't use stale grid
            _previewCts?.Cancel();
            var cts = new CancellationTokenSource();
            _previewCts = cts;

            try
            {
                float voxelSizeMm = GetSelectedVoxelSize();
                if (voxelSizeMm == 0f) return;

                // Capture all UI state on the UI thread before going async
                bool isDualImageMode = (tabModeSelector.SelectedTab == tabDualImage);
                float sizeX = (float)nudSizeX.Value;
                float sizeY = (float)nudSizeY.Value;
                float sizeZ = (float)nudSizeZ.Value;

                // Text mode captures
                string text1 = txtText1.Text;
                string text2 = txtText2.Text;
                Font font1 = _font1;
                Font font2 = _font2;
                if (!isDualImageMode && CountGlyphs(text2) > CountGlyphs(text1))
                {
                    (text1, text2) = (text2, text1);
                    (font1, font2) = (font2, font1);
                }

                // Image mode captures
                CarveOperation op1 = CarveOperation.Extrude, op2 = CarveOperation.Cut;
                bool stretch1 = false, stretch2 = false;
                float pad1 = 0, pad2 = 0, offX1 = 0, offY1 = 0, offX2 = 0, offY2 = 0;
                Stencil stencil1 = null, stencil2 = null;
                if (isDualImageMode)
                {
                    if (_stencil1 == null || _stencil2 == null) return;
                    op1 = cbAction1.SelectedItem?.ToString() == "Extrude" ? CarveOperation.Extrude : CarveOperation.Cut;
                    op2 = cbAction2.SelectedItem?.ToString() == "Extrude" ? CarveOperation.Extrude : CarveOperation.Cut;
                    stretch1 = chkStretch1.Checked; stretch2 = chkStretch2.Checked;
                    pad1 = (float)nudPad1.Value; pad2 = (float)nudPad2.Value;
                    offX1 = (float)nudOffX1.Value; offY1 = (float)nudOffY1.Value;
                    offX2 = (float)nudOffX2.Value; offY2 = (float)nudOffY2.Value;
                    stencil1 = _stencil1; stencil2 = _stencil2;
                }

                VoxelGrid grid = await Task.Run(() =>
                {
                    if (cts.Token.IsCancellationRequested) return null;

                    int cx = (int)Math.Floor(sizeX / voxelSizeMm);
                    int cy = (int)Math.Floor(sizeY / voxelSizeMm);
                    int lz = (int)Math.Floor(sizeZ / voxelSizeMm);
                    if (cx < 1 || cy < 1 || lz < 1) return null;

                    int baseVox = 0;
                    if (!isDualImageMode)
                    {
                        float baseMm = Math.Max(3f, Math.Min(7f, sizeZ * 0.05f));
                        baseVox = Math.Max(1, (int)Math.Ceiling(baseMm / voxelSizeMm));
                    }

                    var g = new VoxelGrid(cx, cy, lz + baseVox, voxelSizeMm);
                    BuildGrid(g, isDualImageMode,
                        text1, text2, font1, font2,
                        stencil1, stencil2,
                        op1, op2,
                        stretch1, stretch2,
                        pad1, pad2,
                        offX1, offY1, offX2, offY2,
                        lz, baseVox);
                    return g;
                }, cts.Token);

                if (cts.Token.IsCancellationRequested || grid == null) return;
                _lastPreviewGrid = grid;

                var faceData = await Task.Run(() => VoxelFaceBuilder.Build(grid), cts.Token);
                if (cts.Token.IsCancellationRequested) return;

                var wpfMesh = new MeshGeometry3D();
                foreach (var v in faceData.Vertices)
                    wpfMesh.Positions.Add(new Point3D(v.X, v.Y, v.Z));
                foreach (var n in faceData.Normals)
                    wpfMesh.Normals.Add(new Vector3D(n.X, n.Y, n.Z));
                foreach (var t in faceData.Triangles)
                    wpfMesh.TriangleIndices.Add(t);

                var model = new GeometryModel3D(
                    wpfMesh,
                    new DiffuseMaterial(new SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 180, 60))));
                model.BackMaterial = new DiffuseMaterial(
                    new SolidColorBrush(System.Windows.Media.Color.FromRgb(180, 60, 60)));

                if (_lettersModel != null) _viewport.Children.Remove(_lettersModel);
                _lettersModel = new ModelVisual3D { Content = model };
                _viewport.Children.Add(_lettersModel);
                _viewport.ZoomExtents(0);
            }
            catch (OperationCanceledException) { }
        }

        // ─── SHARED GRID BUILDER ──────────────────────────────────────────────────

        private void BuildGrid(VoxelGrid grid, bool isDualImageMode,
            string text1, string text2, Font font1, Font font2,
            Stencil stencil1, Stencil stencil2,
            CarveOperation op1, CarveOperation op2,
            bool stretch1, bool stretch2,
            float pad1, float pad2,
            float offX1, float offY1, float offX2, float offY2,
            int letterVoxelCountZ, int baseThicknessVoxels)
        {
            if (isDualImageMode)
            {
                if (op1 == CarveOperation.Extrude)
                    grid.ApplyStencil(stencil1, CarvePlane.Front, CarveOperation.Extrude, stretch1, pad1, offX1, offY1);
                if (op2 == CarveOperation.Extrude)
                    grid.ApplyStencil(stencil2, CarvePlane.Top, CarveOperation.Extrude, stretch2, pad2, offX2, offY2);
                if (op1 == CarveOperation.Cut)
                    grid.ApplyStencil(stencil1, CarvePlane.Front, CarveOperation.Cut, stretch1, pad1, offX1, offY1);
                if (op2 == CarveOperation.Cut)
                    grid.ApplyStencil(stencil2, CarvePlane.Top, CarveOperation.Cut, stretch2, pad2, offX2, offY2);
            }
            else
            {
                if (baseThicknessVoxels > 0)
                    grid.AddBasePlate(baseThicknessVoxels);

                List<Stencil> stencils1 = TextManager.CreateStencilsFromText(text1, font1);
                List<Stencil> stencils2 = TextManager.CreateStencilsFromText(text2, font2);

                if (stencils1.Count == 0 && stencils2.Count == 0) return;

                List<Stencil> extrudeStencils = stencils1.Count >= stencils2.Count ? stencils1 : stencils2;
                List<Stencil> cutStencils = stencils1.Count >= stencils2.Count ? stencils2 : stencils1;

                if (extrudeStencils.Count == 0) return;

                int totalSlots = extrudeStencils.Count;
                int cutOffset = (totalSlots - cutStencils.Count) / 2;

                float maxLetterWidth = 0, maxStencilHeight = 0;
                foreach (var s in extrudeStencils) { if (s.Width > maxLetterWidth) maxLetterWidth = s.Width; if (s.Height > maxStencilHeight) maxStencilHeight = s.Height; }
                foreach (var s in cutStencils) { if (s.Width > maxLetterWidth) maxLetterWidth = s.Width; if (s.Height > maxStencilHeight) maxStencilHeight = s.Height; }

                float slotWidth = (float)grid.Width / totalSlots;
                float cos45 = (float)Math.Cos(Math.PI / 4);
                float sin45 = (float)Math.Sin(Math.PI / 4);

                float scaleX = (slotWidth * 0.8f) / (maxLetterWidth * cos45);
                float scaleZ = (letterVoxelCountZ * 0.9f) / maxStencilHeight;

                float minConst = Math.Min(slotWidth, (float)grid.Height) / (2f * sin45);
                float scaleFit = float.MaxValue;
                for (int i = 0; i < cutStencils.Count; i++)
                {
                    int targetSlot = i + cutOffset;
                    float pairScaleFit = (minConst - 1f) / ((extrudeStencils[targetSlot].Width + cutStencils[i].Width) / 2f);
                    if (pairScaleFit < scaleFit) scaleFit = pairScaleFit;
                }

                if (scaleFit <= 0) throw new Exception(
                    "Text can't physically fit — reduce character count or increase the X dimension.");

                float uniformScale = Math.Min(Math.Min(scaleX, scaleZ), scaleFit);

                // Warning only makes sense interactively, not during background preview rebuild —
                // suppress it here; Export button path can re-check if needed.

                List<float> slotCenters = new List<float>();
                for (int i = 0; i < totalSlots; i++)
                    slotCenters.Add(slotWidth * (i + 0.5f));

                var extrudeBoundsList = new List<VoxelGrid.LetterBounds>();
                for (int i = 0; i < extrudeStencils.Count; i++)
                {
                    var bounds = grid.ComputeLetterBounds(extrudeStencils[i], -45.0f, uniformScale, slotWidth);
                    extrudeBoundsList.Add(bounds);
                    grid.ApplyTextStencil(extrudeStencils[i], -45.0f, CarveOperation.Extrude,
                        slotCenters[i], uniformScale, slotWidth, bounds.HalfWidthX, baseThicknessVoxels);
                }
                for (int i = 0; i < cutStencils.Count; i++)
                {
                    int targetSlot = i + cutOffset;
                    grid.ApplyTextStencil(cutStencils[i], 45.0f, CarveOperation.Intersect,
                        slotCenters[targetSlot], uniformScale, slotWidth,
                        extrudeBoundsList[targetSlot].HalfWidthX, baseThicknessVoxels);
                }
            }
        }

        // ─── EXPORT ───────────────────────────────────────────────────────────────

        private static int CountGlyphs(string s)
        {
            int n = 0;
            foreach (char c in s) if (!char.IsWhiteSpace(c)) n++;
            return n;
        }

        private float GetSelectedVoxelSize()
        {
            if (rbDensityVeryLow.Checked) return 10.0f;
            if (rbDensityLow.Checked) return 5.0f;
            if (rbDensityMedium.Checked) return 2.5f;
            if (rbDensityHigh.Checked) return 1.25f;
            if (rbDensityVeryHigh.Checked) return 0.625f;
            if (rbDensityUltra.Checked) return 0.3125f;
            return 0f;
        }

        private async void btnExport_Click(object sender, EventArgs e)
        {
            float voxelSizeMm = GetSelectedVoxelSize();
            if (voxelSizeMm == 0f)
            {
                MessageBox.Show("Please select a Voxel Density before exporting.", "Density Not Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool isDualImageMode = (tabModeSelector.SelectedTab == tabDualImage);
            float targetSizeXMm = (float)nudSizeX.Value;
            float targetSizeYMm = (float)nudSizeY.Value;
            float targetSizeZMm = (float)nudSizeZ.Value;

            int voxelCountX = (int)Math.Floor(targetSizeXMm / voxelSizeMm);
            int voxelCountY = (int)Math.Floor(targetSizeYMm / voxelSizeMm);
            int letterVoxelCountZ = (int)Math.Floor(targetSizeZMm / voxelSizeMm);

            if (voxelCountX < 1 || voxelCountY < 1 || letterVoxelCountZ < 1)
            {
                MessageBox.Show("Dimensions are too small for the selected voxel density.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int baseThicknessVoxels = 0;
            if (!isDualImageMode)
            {
                float baseThicknessMm = Math.Max(3.0f, Math.Min(7.0f, targetSizeZMm * 0.05f));
                baseThicknessVoxels = Math.Max(1, (int)Math.Ceiling(baseThicknessMm / voxelSizeMm));
            }

            int voxelCountZ = letterVoxelCountZ + baseThicknessVoxels;
            long totalVoxels = (long)voxelCountX * voxelCountY * voxelCountZ;

            if (totalVoxels > 2_000_000_000L)
            {
                long ramMB = totalVoxels / 8 / 1024 / 1024;
                if (MessageBox.Show(
                    $"The requested model requires {totalVoxels:N0} voxels (~{ramMB} MB RAM).\n" +
                    "This may crash 3D printer slicers.\n\nDo you wish to continue?",
                    "Extreme Size Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                    return;
            }

            // Capture image mode UI state
            CarveOperation op1 = CarveOperation.Extrude, op2 = CarveOperation.Cut;
            bool stretch1 = false, stretch2 = false;
            float pad1 = 0, pad2 = 0, offX1 = 0, offY1 = 0, offX2 = 0, offY2 = 0;
            Stencil localStencil1 = null, localStencil2 = null;
            string text1 = "", text2 = "";
            Font localFont1 = null, localFont2 = null;

            if (isDualImageMode)
            {
                if (cbAction1.SelectedItem?.ToString() == cbAction2.SelectedItem?.ToString())
                {
                    MessageBox.Show("Actions cannot be the same. One must Extrude and one must Cut.", "Action Conflict", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (_stencil1 == null || _stencil2 == null)
                {
                    MessageBox.Show("Please load both images.", "Missing Images", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                op1 = cbAction1.SelectedItem?.ToString() == "Extrude" ? CarveOperation.Extrude : CarveOperation.Cut;
                op2 = cbAction2.SelectedItem?.ToString() == "Extrude" ? CarveOperation.Extrude : CarveOperation.Cut;
                stretch1 = chkStretch1.Checked; stretch2 = chkStretch2.Checked;
                pad1 = (float)nudPad1.Value; pad2 = (float)nudPad2.Value;
                offX1 = (float)nudOffX1.Value; offY1 = (float)nudOffY1.Value;
                offX2 = (float)nudOffX2.Value; offY2 = (float)nudOffY2.Value;
                localStencil1 = _stencil1; localStencil2 = _stencil2;
            }
            else
            {
                text1 = txtText1.Text;
                text2 = txtText2.Text;
                localFont1 = _font1;
                localFont2 = _font2;
                if (CountGlyphs(text2) > CountGlyphs(text1))
                {
                    (text1, text2) = (text2, text1);
                    (localFont1, localFont2) = (localFont2, localFont1);
                }
            }

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
                        // Reuse the preview grid if it's fresh (same settings, just finished)
                        // otherwise build it now.
                        VoxelGrid grid = _lastPreviewGrid;

                        await Task.Run(() =>
                        {
                            if (grid == null)
                            {
                                grid = new VoxelGrid(voxelCountX, voxelCountY, voxelCountZ, voxelSizeMm);
                                BuildGrid(grid, isDualImageMode,
                                    text1, text2, localFont1, localFont2,
                                    localStencil1, localStencil2,
                                    op1, op2,
                                    stretch1, stretch2,
                                    pad1, pad2,
                                    offX1, offY1, offX2, offY2,
                                    letterVoxelCountZ, baseThicknessVoxels);
                            }

                            MeshData mesh = VoxelMesher.Generate(grid, isoLevel: 0.5f);
                            VoxelToStlExporter.ExportMeshToStl(mesh, filePath);
                        });

                        MessageBox.Show("Successfully exported to STL!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    finally
                    {
                        btnExport.Text = "Export to STL";
                        btnExport.Enabled = true;
                    }
                }
            }
        }

        // ─── IMAGE LOAD BUTTONS ───────────────────────────────────────────────────

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
                        OnPreviewSettingChanged(sender, e);
                    }
                    catch (AntiAliasingException ex)
                    {
                        if (MessageBox.Show(ex.Message, "Anti-Aliasing Detected", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                        {
                            _stencil1 = StencilManager.CreateFromImage(ofd.FileName, autoFix: true);
                            lblImg1Status.Text = $"Loaded (Auto-Fixed): {_stencil1.Width}x{_stencil1.Height}";
                            OnPreviewSettingChanged(sender, e);
                        }
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                }
            }
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
                        OnPreviewSettingChanged(sender, e);
                    }
                    catch (AntiAliasingException ex)
                    {
                        if (MessageBox.Show(ex.Message, "Anti-Aliasing Detected", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                        {
                            _stencil2 = StencilManager.CreateFromImage(ofd.FileName, autoFix: true);
                            lblImg2Status.Text = $"Loaded (Auto-Fixed): {_stencil2.Width}x{_stencil2.Height}";
                            OnPreviewSettingChanged(sender, e);
                        }
                    }
                    catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                }
            }
        }

        // ─── FONT BUTTONS ─────────────────────────────────────────────────────────

        private void btnFont1_Click(object sender, EventArgs e)
        {
            using (FontDialog fd = new FontDialog())
            {
                fd.Font = _font1;
                if (fd.ShowDialog() == DialogResult.OK)
                {
                    _font1 = fd.Font;
                    lblFont1.Text = $"{_font1.Name} ({_font1.Size})";
                    OnPreviewSettingChanged(sender, e);
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
                    OnPreviewSettingChanged(sender, e);
                }
            }
        }

        // ─── DEAD DESIGNER EVENT STUBS ────────────────────────────────────────────

        private void numericUpDown1_ValueChanged(object sender, EventArgs e) { }
        private void radioButton1_CheckedChanged(object sender, EventArgs e) { }
        private void label1_Click(object sender, EventArgs e) { }
        private void label1_Click_1(object sender, EventArgs e) { }
        private void numericUpDown3_ValueChanged(object sender, EventArgs e) { }
        private void cbAction1_SelectedIndexChanged(object sender, EventArgs e) { }
        private void groupBox2_Enter(object sender, EventArgs e) { }
        private void label3_Click(object sender, EventArgs e) { }
        private void txtText1_TextChanged(object sender, EventArgs e) { }
        private void txtText2_TextChanged(object sender, EventArgs e) { }
        private void labelText1_Click(object sender, EventArgs e) { }
        private void tabDualText_Click(object sender, EventArgs e) { }
        private void groupBox1_Enter(object sender, EventArgs e) { }
    }
}