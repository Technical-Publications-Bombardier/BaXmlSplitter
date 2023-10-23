using System.ComponentModel;
using System.Windows.Forms.Design;

namespace BaXmlSplitter
{
    [Designer(typeof(GlyphableCheckboxDesigner))]
    [ToolboxBitmap(typeof(CheckBox))]
    public class GlyphableCheckbox(Image @checked, Image @unchecked) : CheckBox
    {
        internal const int DefaultImageSize = 16;

        public override string Text
        {
            get => string.Empty;
            set { /* Do nothing */ }
        }

        public Image CheckmarkGlyph
        {
            get => @checked;
            set
            {
                @checked = value;
                Invalidate();
                UpdateSize();
            }
        }

        public Image UnCheckmarkGlyph
        {
            get => @unchecked;
            set
            {
                @unchecked = value;
                Invalidate();
                UpdateSize();
            }
        }

        protected override void OnPaint(PaintEventArgs? pevent)
        {
            if (pevent is null)
            {
                return;
            }
            //clear the previous image
            pevent.Graphics.Clear(Parent?.BackColor ?? BackColor);
            pevent.Graphics.DrawImage(Checked ? CheckmarkGlyph : UnCheckmarkGlyph, new Rectangle(0, 0, Width, Height));
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;
            @checked.Dispose();
            @unchecked.Dispose();
            CheckmarkGlyph.Dispose();
            UnCheckmarkGlyph.Dispose();
        }

        private void UpdateSize()
        {
            Width = Math.Max(CheckmarkGlyph?.Width ?? DefaultImageSize, UnCheckmarkGlyph?.Width ?? DefaultImageSize);
            Height = Math.Max(CheckmarkGlyph?.Height ?? DefaultImageSize, UnCheckmarkGlyph?.Height ?? DefaultImageSize);
            Size = new Size(Width, Height);
        }
    }

    public class GlyphableCheckboxDesigner : ControlDesigner
    {
        protected override void OnPaintAdornments(PaintEventArgs pe)
        {
            base.OnPaintAdornments(pe);
            var checkbox = (GlyphableCheckbox)Control;
            checkbox.MinimumSize = new Size(GlyphableCheckbox.DefaultImageSize, GlyphableCheckbox.DefaultImageSize);
            ArgumentNullException.ThrowIfNull(pe, nameof(pe));
            if (checkbox.Checked)
            {
                pe.Graphics.DrawImage(checkbox.CheckmarkGlyph, new Rectangle(checkbox.Width - checkbox.CheckmarkGlyph.Width, 0, checkbox.CheckmarkGlyph.Width, checkbox.CheckmarkGlyph.Height));
            }
            else
            {
                pe.Graphics.DrawImage(checkbox.UnCheckmarkGlyph, new Rectangle(checkbox.Width - checkbox.UnCheckmarkGlyph.Width, 0, checkbox.UnCheckmarkGlyph.Width, checkbox.UnCheckmarkGlyph.Height));
            }
        }
    }

    public class ShowPasswordCheckbox() : GlyphableCheckbox(Properties.Resources.EyeOpen, Properties.Resources.EyeClosed);
}
