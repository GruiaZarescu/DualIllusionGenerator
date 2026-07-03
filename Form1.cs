using HelixToolkit.Wpf;
using System.Windows.Forms.Integration;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using static VoxelGrid;

namespace DualIllusionGenerator
{
    public partial class Form1 : Form
    {
        private Stencil _stencil1;
        private Stencil _stencil2;
        private Font _font1 = new Font("Arial", 12, FontStyle.Regular);
        private Font _font2 = new Font("Arial", 12, FontStyle.Regular);
        private Font _imgFont1 = new Font("Arial", 12, FontStyle.Regular);
        private Font _imgFont2 = new Font("Arial", 12, FontStyle.Regular);
        private int smoothIterations = 3;

        private HelixViewport3D _viewport;
        private ModelVisual3D _lettersModel;
        private VoxelGrid _lastPreviewGrid;
        private System.Windows.Forms.Timer _previewDebounce;
        private CancellationTokenSource _previewCts;
        private float LetterSpacingPercent = 15f;

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

        public float GetTextResolution()
        {
            float textRes = 0f;
            if (rbTextResVeryLow.Checked) textRes = 32;
            else if (rbTextResLow.Checked) textRes = 64;
            else if (rbTextResMedium.Checked) textRes = 128;
            else if (rbTextResHigh.Checked) textRes = 256;
            else if (rbTextResVeryHigh.Checked) textRes = 512;
            else if (rbTextResUltra.Checked) textRes = 1024;
            return textRes;
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

        private async void _previewDebounce_Tick(object sender, EventArgs e)
        {
            _previewDebounce.Stop();
            await RegeneratePreviewAsync();
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

                CarveOperation op1 = ParseCarveOperation(cbAction1, CarveOperation.Extrude);
                CarveOperation op2 = ParseCarveOperation(cbAction2, CarveOperation.Cut);
                bool stretch1 = false, stretch2 = false;
                float pad1 = 0, pad2 = 0, offX1 = 0, offY1 = 0, offX2 = 0, offY2 = 0;
                Stencil stencil1 = null, stencil2 = null;
                if (isDualImageMode)
                {

                    stretch1 = chkStretch1.Checked;
                    stretch2 = chkStretch2.Checked;
                    pad1 = (float)nudPad1.Value;
                    pad2 = (float)nudPad2.Value;
                    offX1 = (float)nudOffX1.Value;
                    offY1 = (float)nudOffY1.Value;
                    offX2 = (float)nudOffX2.Value;
                    offY2 = (float)nudOffY2.Value;
                    // Determine Stencil 1
                    if (rbImg1Text.Checked)
                    {
                        if (!string.IsNullOrWhiteSpace(txtImg1Text.Text))
                            stencil1 = TextManager.CreateWholeTextStencil(txtImg1Text.Text, _imgFont1, GetTextResolution());
                    }
                    else
                    {
                        stencil1 = _stencil1;
                    }

                    // Determine Stencil 2
                    if (rbImg2Text.Checked)
                    {
                        if (!string.IsNullOrWhiteSpace(txtImg2Text.Text))
                            stencil2 = TextManager.CreateWholeTextStencil(txtImg2Text.Text, _imgFont2, GetTextResolution());
                    }
                    else
                    {
                        stencil2 = _stencil2;
                    }

                    // Validation
                    if (stencil1 == null && stencil2 == null)
                    {
                        // In preview, just return. In export, show a message box
                        return;
                    }
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

                // --- FIX: BUILD WPF MESH ON BACKGROUND THREAD AND FREEZE IT ---
                MeshGeometry3D wpfMesh = null;
                await Task.Run(() =>
                {
                    var positions = new Point3DCollection(faceData.Vertices.Count);
                    foreach (var v in faceData.Vertices) positions.Add(new Point3D(v.X, v.Y, v.Z));

                    var normals = new Vector3DCollection(faceData.Normals.Count);
                    foreach (var n in faceData.Normals) normals.Add(new Vector3D(n.X, n.Y, n.Z));

                    var indices = new Int32Collection(faceData.Triangles.Count);
                    foreach (var t in faceData.Triangles) indices.Add(t);

                    wpfMesh = new MeshGeometry3D
                    {
                        Positions = positions,
                        Normals = normals,
                        TriangleIndices = indices
                    };

                    // Freeze makes the object cross-thread accessible and stops UI notification spam
                    wpfMesh.Freeze();
                }, cts.Token);

                if (cts.Token.IsCancellationRequested) return;

                // Freeze materials too so they don't trigger UI updates
                var brush1 = new SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 180, 60));
                brush1.Freeze();
                var mat1 = new DiffuseMaterial(brush1);
                mat1.Freeze();

                var brush2 = new SolidColorBrush(System.Windows.Media.Color.FromRgb(180, 60, 60));
                brush2.Freeze();
                var mat2 = new DiffuseMaterial(brush2);
                mat2.Freeze();

                var model = new GeometryModel3D(wpfMesh, mat1) { BackMaterial = mat2 };

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
                // 1. Process all Extrude operations first so there is material to cut from
                if (stencil1 != null && op1 == CarveOperation.Extrude)
                    grid.ApplyStencil(stencil1, CarvePlane.Front, op1, stretch1, pad1, offX1, offY1);

                if (stencil2 != null && op2 == CarveOperation.Extrude)
                    grid.ApplyStencil(stencil2, CarvePlane.Top, op2, stretch2, pad2, offX2, offY2);

                // 2. Process Cut/Intersect operations second
                if (stencil1 != null && op1 != CarveOperation.Extrude)
                    grid.ApplyStencil(stencil1, CarvePlane.Front, op1, stretch1, pad1, offX1, offY1);

                if (stencil2 != null && op2 != CarveOperation.Extrude)
                    grid.ApplyStencil(stencil2, CarvePlane.Top, op2, stretch2, pad2, offX2, offY2);
            }
            else
            {
                if (baseThicknessVoxels > 0)
                    grid.AddBasePlate(baseThicknessVoxels);

                List<Stencil> stencils1 = TextManager.CreateStencilsFromText(text1, font1, GetTextResolution());
                List<Stencil> stencils2 = TextManager.CreateStencilsFromText(text2, font2, GetTextResolution());

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

                float trackingFactor = 1f + (LetterSpacingPercent / 100f);

                float minConst = Math.Min(slotWidth, (float)grid.Height) / (2f * sin45);
                float scaleFit = float.MaxValue;
                for (int i = 0; i < cutStencils.Count; i++)
                {
                    int targetSlot = i + cutOffset;
                    float pairWidth = (extrudeStencils[targetSlot].Width + cutStencils[i].Width) / 2f;
                    float pairScaleFit = (minConst - 1f) / (pairWidth * trackingFactor);
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

                    // Match the cut stencil's vertical mapping to its paired extrude letter's
                    // actual physical height, so a short letter (e.g. lowercase 'm') cutting
                    // into a tall one (e.g. 'T') stretches to cover the full height instead
                    // of leaving the top uncut.
                    Stencil pairedExtrude = extrudeStencils[targetSlot];
                    float cutHeightPx = cutStencils[i].TrueBottom > 0 ? cutStencils[i].TrueBottom : cutStencils[i].Height;
                    float extrudeHeightPx = pairedExtrude.TrueBottom > 0 ? pairedExtrude.TrueBottom : pairedExtrude.Height;
                    float cutHeightScale = uniformScale * (extrudeHeightPx / cutHeightPx);

                    grid.ApplyTextStencil(cutStencils[i], 45.0f, CarveOperation.Intersect,
                        slotCenters[targetSlot], uniformScale, slotWidth,
                        extrudeBoundsList[targetSlot].HalfWidthX, baseThicknessVoxels,
                        heightScale: cutHeightScale);
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

            CarveOperation op1 = ParseCarveOperation(cbAction1, CarveOperation.Extrude);
            CarveOperation op2 = ParseCarveOperation(cbAction2, CarveOperation.Cut);
            bool stretch1 = false, stretch2 = false;
            float pad1 = 0, pad2 = 0, offX1 = 0, offY1 = 0, offX2 = 0, offY2 = 0;
            Stencil localStencil1 = null, localStencil2 = null;
            string text1 = "", text2 = "";
            Font localFont1 = null, localFont2 = null;

            Stencil stencil1 = null, stencil2 = null;
            if (isDualImageMode)
            {

                stretch1 = chkStretch1.Checked;
                stretch2 = chkStretch2.Checked;
                pad1 = (float)nudPad1.Value;
                pad2 = (float)nudPad2.Value;
                offX1 = (float)nudOffX1.Value;
                offY1 = (float)nudOffY1.Value;
                offX2 = (float)nudOffX2.Value;
                offY2 = (float)nudOffY2.Value;
                // Determine Stencil 1
                if (rbImg1Text.Checked)
                {
                    if (!string.IsNullOrWhiteSpace(txtImg1Text.Text))
                        stencil1 = TextManager.CreateWholeTextStencil(txtImg1Text.Text, _imgFont1, GetTextResolution());
                }
                else
                {
                    stencil1 = _stencil1;
                }

                // Determine Stencil 2
                if (rbImg2Text.Checked)
                {
                    if (!string.IsNullOrWhiteSpace(txtImg2Text.Text))
                        stencil2 = TextManager.CreateWholeTextStencil(txtImg2Text.Text, _imgFont2, GetTextResolution());
                }
                else
                {
                    stencil2 = _stencil2;
                }

                // Validation
                if (stencil1 == null && stencil2 == null)//Let's allow using only one image. 
                {
                    return;
                }
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

                            if (checkBoxEnableSmoothing.Checked)
                            {
                                MeshData mesh = VoxelMesher.Generate(grid, isoLevel: 0.5f);
                                MeshWelder.Weld(mesh);
                                MeshSmoother.Smooth(mesh, smoothIterations);
                                VoxelToStlExporter.ExportMeshToStl(mesh, filePath);
                            }
                            else
                            {
                                VoxelToStlExporter.Export(grid, filePath);
                            }

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
            bool unloadMode = btnLoadImage1.Text == "Unload Image 2";

            if (!unloadMode)
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
                            UpdateLoadImage1Button();
                            OnPreviewSettingChanged(sender, e);
                        }
                        catch (AntiAliasingException ex)
                        {
                            if (MessageBox.Show(ex.Message, "Anti-Aliasing Detected", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                            {
                                _stencil1 = StencilManager.CreateFromImage(ofd.FileName, autoFix: true);
                                UpdateLoadImage1Button(autoFixed: true);
                                OnPreviewSettingChanged(sender, e);
                            }
                        }
                        catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                    }
                }
            }
            else
            {
                UpdateLoadImage1Button();
            }


            void UpdateLoadImage1Button(bool autoFixed = false)
            {
                if (unloadMode)
                {
                    lblImg1Status.Visible = false;
                    _stencil1 = null;
                    btnLoadImage1.Text = "Load Image 2";
                }
                else
                {
                    lblImg1Status.Visible = true;
                    lblImg1Status.Text = !autoFixed ? $"Loaded: {_stencil1.Width}x{_stencil1.Height}" : $"Loaded (Auto-Fixed): {_stencil1.Width}x{_stencil1.Height}";
                    btnLoadImage1.Text = "Unload Image 1";
                }
            }
        }

        private void btnLoadImage2_Click(object sender, EventArgs e)
        {

            bool unloadMode = btnLoadImage2.Text == "Unload Image 2";

            if (!unloadMode)
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
                            UpdateLoadImage2Button();
                            OnPreviewSettingChanged(sender, e);
                        }
                        catch (AntiAliasingException ex)
                        {
                            if (MessageBox.Show(ex.Message, "Anti-Aliasing Detected", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                            {
                                _stencil2 = StencilManager.CreateFromImage(ofd.FileName, autoFix: true);
                                UpdateLoadImage2Button(autoFixed: true);
                                OnPreviewSettingChanged(sender, e);
                            }
                        }
                        catch (Exception ex) { MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                    }
                }
            }
            else
            {
                UpdateLoadImage2Button();
            }


            void UpdateLoadImage2Button(bool autoFixed = false)
            {
                if (unloadMode)
                {
                    lblImg2Status.Visible = false;
                    _stencil2 = null;
                    btnLoadImage2.Text = "Load Image 2";
                }
                else
                {
                    lblImg2Status.Visible = true;
                    lblImg2Status.Text = !autoFixed ? $"Loaded: {_stencil2.Width}x{_stencil2.Height}" : $"Loaded (Auto-Fixed): {_stencil2.Width}x{_stencil2.Height}";
                    btnLoadImage2.Text = "Unload Image 2";
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

        private void rbImg1Image_CheckedChanged(object sender, EventArgs e)
        {
            bool isImage = rbImg1Image.Checked;
            if (isImage)
            {
                btnLoadImage1.Visible = true;
                btnImg1Font.Visible = false;
                txtImg1Text.Visible = false;
                lblImg1Font.Visible = false;
            }
            else
            {
                btnLoadImage1.Visible = false;
                btnImg1Font.Visible = true;
                txtImg1Text.Visible = true;
                if (lblImg1Font.Text != " ") lblImg1Font.Visible = true;
            }

            OnPreviewSettingChanged(sender, e);
        }

        private void rbImg2Image_CheckedChanged(object sender, EventArgs e)
        {
            bool isImage = rbImg2Image.Checked;
            if (isImage)
            {
                btnLoadImage2.Visible = true;
                btnImg2Font.Visible = false;
                txtImg2Text.Visible = false;
                lblImg2Font.Visible = false;
            }
            else
            {
                btnLoadImage2.Visible = false;
                btnImg2Font.Visible = true;
                txtImg2Text.Visible = true;
                if (lblImg2Font.Text != " ") lblImg2Font.Visible = true;
            }

            OnPreviewSettingChanged(sender, e);
        }

        private void btnImg1Font_Click(object sender, EventArgs e)
        {
            using (FontDialog fd = new FontDialog())
            {
                fd.Font = _imgFont1;
                if (fd.ShowDialog() == DialogResult.OK)
                {
                    _imgFont1 = fd.Font;
                    lblImg1Font.Visible = true;
                    lblImg1Font.Text = $"{_imgFont1.Name}";
                    OnPreviewSettingChanged(sender, e);
                }
            }
        }

        private void btnImg2Font_Click(object sender, EventArgs e)
        {
            using (FontDialog fd = new FontDialog())
            {
                fd.Font = _imgFont2;
                if (fd.ShowDialog() == DialogResult.OK)
                {
                    _imgFont2 = fd.Font;
                    lblImg2Font.Visible = true;
                    lblImg2Font.Text = $"{_imgFont2.Name}";
                    OnPreviewSettingChanged(sender, e);
                }
            }
        }

        private CarveOperation ParseCarveOperation(ComboBox cb, CarveOperation defaultOp)
        {
            if (cb == null || cb.SelectedItem == null) return defaultOp;
            return cb.SelectedItem.ToString() switch
            {
                "Extrude" => CarveOperation.Extrude,
                "Cut" => CarveOperation.Cut,
                "Intersect" => CarveOperation.Intersect,
                _ => defaultOp
            };
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

        private void rbImg2Text_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void chkStretch2_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter_1(object sender, EventArgs e)
        {

        }

        private void rbImg1Text_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void txtImg1Text_TextChanged(object sender, EventArgs e)
        {

        }

        private void lblImg2Status_Click(object sender, EventArgs e)
        {

        }

        private void nudPad2_ValueChanged(object sender, EventArgs e)
        {

        }

        private void nudOffX2_ValueChanged(object sender, EventArgs e)
        {

        }

        private void nudOffY2_ValueChanged(object sender, EventArgs e)
        {

        }

        private void nudPad1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void nudOffX1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void nudOffY1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void lblImg1Status_Click(object sender, EventArgs e)
        {

        }

        private void CubeDimensionsGroupBox_Enter(object sender, EventArgs e)
        {

        }

        private void nudSizeY_ValueChanged(object sender, EventArgs e)
        {

        }

        private void nudSizeX_ValueChanged(object sender, EventArgs e)
        {

        }

        private void panelPreview_Paint(object sender, PaintEventArgs e)
        {

        }

        private void rbDensityLow_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void rbDensityMedium_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void rbDensityHigh_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void rbDensityVeryHigh_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void rbDensityUltra_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void VoxelDensityGroupBox_Enter(object sender, EventArgs e)
        {

        }

        private void txtImg2Text_TextChanged(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void smoothTrackBar_Scroll(object sender, EventArgs e)
        {
            smoothIterations = smoothTrackBar.Value;
        }

        private void checkBoxEnableSmoothing_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxEnableSmoothing.Checked)
            {
                smoothTrackBar.Visible = true;
                lblSmoothAmount.Visible = true;
            }
            else
            {
                smoothTrackBar.Visible = false;
                lblSmoothAmount.Visible = false;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void lblSmoothAmount_Click(object sender, EventArgs e)
        {

        }

        private void label10_Click(object sender, EventArgs e)
        {

        }

        private void nudExtraLetterSpacing_ValueChanged(object sender, EventArgs e)
        {
            LetterSpacingPercent = (float)nudExtraLetterSpacing.Value;
            OnPreviewSettingChanged(sender, e);
        }
    }
}