using System;
using System.Collections.Generic;
using System.Text;

namespace DualIllusionGenerator
{
    public static class DocumentationText
    {
        public const string FullText =
        @"DUAL ILLUSION GENERATOR — USER GUIDE

        ────────────────────────────────────
        WHAT THIS APP DOES
        ────────────────────────────────────
        Generates 3D-printable dual-illusion models: shapes that read as one
        thing from the front and a different thing from the top,
        or as one text from the front and a different text from the side,
        using two intersecting 45° stencil projections carved
        into a shared voxel volume.

        ────────────────────────────────────
        DUAL TEXT MODE
        ────────────────────────────────────
        Type two words. Each is broken into individual letters and rendered
        as image stencils using your chosen font.

        - The longer word's letters are extruded at -45°, one per slot along
          the model's width, each sized to its own physical footprint.
        - The shorter word's letters are cut/intersected at +45° into the
          matching slots, automatically centered if the word is shorter.
        - You can use any combination of upper and lowercase.
          Non-uniform height scaling ensures a short letter (like 'm') cutting
          into a tall one (like 'T') stretches to the full height, so no
          detail is lost off the top.
        - Letter Spacing adds extra breathing room between letters, as a
          percentage of their default tightest legal packing.
        - A base plate is added automatically underneath so physically
          separate letters print as one connected object.

        ────────────────────────────────────
        DUAL IMAGE MODE
        ────────────────────────────────────
        Load two images (PNG/BMP/JPG with a transparent background), or type
        text to be rendered as a whole-string image instead.

        - Image 1 projects from the Front plane; Image 2 projects from the
          Top plane.
        - Each can independently be set to Extrude or Cut.
        - Stretch to Fill ignores aspect ratio and fills the target area
          exactly; otherwise the image is scaled to fit while preserving
          proportions.
        - Padding shrinks a Cut stencil's target area, leaving a margin.
        - X/Y Offset nudges the image's placement within its plane.
        - A live thumbnail preview shows the currently loaded image in each
          slot.

        ────────────────────────────────────
        VOXEL DENSITY
        ────────────────────────────────────
        Controls the resolution of the 3D grid, from Very Low (10mm voxels,
        fast, blocky) to Ultra (0.3125mm voxels, slow, highly detailed).
        Higher density dramatically increases memory use and export time —
        extreme sizes trigger a warning before proceeding.

        ────────────────────────────────────
        TEXT RESOLUTION
        ────────────────────────────────────
        Controls the pixel resolution used when rendering letters/text to
        stencils before voxelization, from Very Low (32px) to Ultra (1024px).
        Higher values give smoother curves on large text; lower values are
        faster for quick previews.

        ────────────────────────────────────
        MESH SMOOTHING
        ────────────────────────────────────
        An optional export-time pass that relaxes the exported mesh surface
        using Taubin smoothing, reducing the staircase look of voxel edges
        without shrinking the model. Adjustable iteration count — higher
        values smooth more but can soften fine detail on small text. On by
        default; the raw voxel-face mesh is used when disabled.

        ────────────────────────────────────
        LIVE PREVIEW
        ────────────────────────────────────
        The 3D viewport updates automatically shortly after any setting
        changes. The front-facing surface and the reverse side are shown in
        different colors so you can distinguish viewing direction while
        orbiting the model.

        -Previews that are too large to generate will be skipped.

        ────────────────────────────────────
        EXPORTING
        ────────────────────────────────────
        Export to STL produces a binary STL file sized to your specified X/Y/Z
        dimensions in millimeters at your chosen voxel density, ready to open
        directly in your slicer.
        ";
    }
}
