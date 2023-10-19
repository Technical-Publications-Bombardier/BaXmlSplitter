using System.ComponentModel;
using System.Windows.Forms.Design;

namespace BaXmlSplitter
{
    [Designer(typeof(GlyphableCheckboxDesigner))]
    [ToolboxBitmap(typeof(CheckBox))]
    internal class GlyphableCheckbox : CheckBox
    {
        private Image checkmarkGlyph;
        public Image CheckmarkGlyph
        {
            get => checkmarkGlyph;
            set
            {
                checkmarkGlyph = value;
                Invalidate();
                UpdateSize();
            }
        }
        private Image unCheckmarkGlyph;
        public Image UnCheckmarkGlyph
        {
            get => unCheckmarkGlyph;
            set
            {
                unCheckmarkGlyph = value;
                Invalidate();
                UpdateSize();
            }
        }

        public GlyphableCheckbox(Image checkmark, Image uncheckmark)
        {
            checkmarkGlyph = CheckmarkGlyph = checkmark;
            unCheckmarkGlyph = UnCheckmarkGlyph = uncheckmark;
        }
        protected override void OnPaint(PaintEventArgs pevent)
        {
            //clear the previous image
            if (Parent is not null)
            {
                pevent.Graphics.Clear(Parent.BackColor);
            }
            if (Checked)
            {
                pevent.Graphics.DrawImage(CheckmarkGlyph, new Rectangle(0, 0, Width, Height));
            }
            else
            {
                pevent.Graphics.DrawImage(UnCheckmarkGlyph, new Rectangle(0, 0, Width, Height));
            }
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;
            CheckmarkGlyph.Dispose();
            UnCheckmarkGlyph.Dispose();
        }

        private void UpdateSize()
        {
            int width = Math.Max(CheckmarkGlyph?.Width ?? 0, UnCheckmarkGlyph?.Width ?? 0);
            int height = Math.Max(CheckmarkGlyph?.Height ?? 0, UnCheckmarkGlyph?.Height ?? 0);

            Size = new Size(width, height);
        }
    }

    public class GlyphableCheckboxDesigner : ControlDesigner
    {
        protected override void OnPaintAdornments(PaintEventArgs pe)
        {
            base.OnPaintAdornments(pe);
            GlyphableCheckbox checkbox = (GlyphableCheckbox)Control;
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
}
