﻿using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using WindowsFormsAero.InteropServices;
using System.Drawing;
using System.Security.Permissions;

namespace WindowsFormsAero
{
    [System.ComponentModel.DesignerCategory("Code")]
    public partial class AeroRichEdit : RichTextBox
    {
        private IRichEditOle _ole;
        private RichTextDocument _document;

        private static readonly object EventZoomFactorChanged = new object();

        internal bool _allowProtectedModifications;

        public void BeginUpdate()
        {
            SetRedraw(false);
        }

        public void EndUpdate()
        {
            SetRedraw(true);
            Invalidate();
        }

        public void InsertControl(Int32 charIndex, Control control)
        {
            if (control == null)
            {
                throw new ArgumentNullException("control");
            }

            var clientSite = RichEditOle.GetClientSite();

            try
            {
                RichEditOle.InsertObject(RichEditObject.FromControl(charIndex, clientSite, control));
            }
            finally
            {
                Marshal.ReleaseComObject(clientSite);
            }
        }

        public void InsertBitmap(Int32 charIndex, Bitmap bitmap)
        {
            if (bitmap == null)
            {
                throw new ArgumentNullException("bitmap");
            }

            var clientSite = RichEditOle.GetClientSite();

            try
            {
                RichEditOle.InsertObject(RichEditObject.FromBitmap(charIndex, clientSite, bitmap));
            }
            finally
            {
                Marshal.ReleaseComObject(clientSite);
            }
        }

        public new int FontHeight
        {
            get { return base.FontHeight; }
        }

        public bool InUnprotectedScope
        {
            get { return _allowProtectedModifications; }
        }

        public int FirstVisibleLine
        {
            get
            {
                return SendMessage(WindowMessages.EM_GETFIRSTVISIBLELINE, IntPtr.Zero, IntPtr.Zero).ToInt32();
            }
        }

        public int FirstVisibleCharIndex
        {
            get { return GetFirstCharIndexFromLine(FirstVisibleLine); }
        }

        public int LastVisibleLine
        {
            get
            {
                var frect = FormattingRect;

                var pt = new Point(frect.Left + 1, frect.Bottom - 2);
                var ch = GetCharIndexFromPosition(pt);

                return GetLineFromCharIndex(ch);
            }
        }

        public int LastVisibleCharIndex
        {
            get
            {
                var ch = GetFirstCharIndexFromLine(LastVisibleLine + 1);

                if (ch >= 0)
                {
                    return ch - 1;
                }

                return TextLength - 1;
            }
        }

        internal IRichEditOle RichEditOle
        {
            get
            {
                if (_ole == null)
                {
                    object o;
                    SendMessage(WindowMessages.EM_GETOLEINTERFACE, IntPtr.Zero, out o);

                    _ole = o as IRichEditOle;
                }

                return _ole;
            }
        }

        public RichTextDocument TextDocument
        {
            get
            {
                if (_document == null)
                {
                    _document = new RichTextDocument(this, RichEditOle);
                }

                return _document;
            }
        }

        public event EventHandler ZoomFactorChanged
        {
            add { Events.AddHandler(EventZoomFactorChanged, value); }
            remove { Events.RemoveHandler(EventZoomFactorChanged, value); }
        }

        public AeroRichEditUnprotectedScope AllowProtectedUpdates()
        {
            return new AeroRichEditUnprotectedScope(this);
        }

        protected override void Dispose(bool disposing)
        {
            if (_ole != null)
            {
                Marshal.ReleaseComObject(_ole);
            }

            _ole = null;
            _document = null;

            base.Dispose(disposing);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            EventMask |= RichEditEvent.Protected;

            base.OnHandleCreated(e);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            if (!_allowProtectedModifications)
            {
                base.OnTextChanged(e);
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if ((ModifierKeys & Keys.Control) == Keys.Control)
            {
                if (e.Delta > 0)
                {
                    ZoomFactor += 0.1f;
                }
                else
                {
                    ZoomFactor -= 0.1f;
                }
            }
        }

        protected virtual void OnZoomFactorChanged(EventArgs e)
        {
            var handler = Events[EventZoomFactorChanged] as EventHandler;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WindowMessages.WM_REFLECT + WindowMessages.WM_NOTIFY)
            {
                var nmhdr = (NMHDR)Marshal.PtrToStructure(m.LParam, typeof(NMHDR));

                if (nmhdr.code == WindowNotifications.EN_PROTECTED)
                {
                    if (_allowProtectedModifications)
                    {
                        m.Result = IntPtr.Zero;
                    }
                    else
                    {
                        m.Result = new IntPtr(-1);
                    }

                    return;
                }
            }
            
            base.WndProc(ref m);

            if (m.Msg == WindowMessages.EM_SETZOOM)
            {
                OnZoomFactorChanged(EventArgs.Empty);
            }
        }

        private RECT FormattingRect
        {
            get
            {
                var result = new RECT();
                SendMessage(WindowMessages.EM_GETRECT, IntPtr.Zero, ref result);

                return result;
            }
        }

        private RichEditEvent EventMask
        {
            get { return (RichEditEvent)SendMessage(WindowMessages.EM_GETEVENTMASK, IntPtr.Zero, IntPtr.Zero).ToInt32(); }
            set { SendMessage(WindowMessages.EM_SETEVENTMASK, IntPtr.Zero, new IntPtr((Int32)(value))); }
        }

        private void SetRedraw(bool redraw)
        {
            SendMessage(
                WindowMessages.WM_SETREDRAW,
                redraw ? new IntPtr(1) : IntPtr.Zero,
                IntPtr.Zero);
        }

        private IntPtr SendMessage(uint msg, IntPtr wParam, IntPtr lParam)
        {
            return NativeMethods.SendMessage(new HandleRef(this, Handle), msg, wParam, lParam);
        }

        private IntPtr SendMessage(uint msg, IntPtr wParam, ref RECT lParam)
        {
            return NativeMethods.SendMessage(new HandleRef(this, Handle), msg, wParam, ref lParam);
        }

        private IntPtr SendMessage(uint msg, IntPtr wParam, out object lParam)
        {
            return NativeMethods.SendMessage(new HandleRef(this, Handle), msg, wParam, out lParam);
        }

        public static int PixelsToPointsX(int pixels)
        {
            return PixelsToPoints(pixels, LogPixelsX);
        }

        public static int PixelsToPointsY(int pixels)
        {
            return PixelsToPoints(pixels, LogPixelsY);
        }

        private static int PixelsToPoints(int pixels, double logPixels)
        {
            return (int)(((pixels / logPixels) * 72.0));
        }

        private static void SetupLogPixels()
        {
            IntPtr hDesktopDC = NativeMethods.GetDC(new HandleRef());

            if (hDesktopDC != IntPtr.Zero)
            {
                try
                {
                    _logPixelsX = NativeMethods.GetDeviceCaps(hDesktopDC, DeviceCapability.LogPixelsX);
                    _logPixelsY = NativeMethods.GetDeviceCaps(hDesktopDC, DeviceCapability.LogPixelsY);
                }
                finally
                {
                    NativeMethods.ReleaseDC(new HandleRef(), hDesktopDC);
                }
            }
        }

        private static int LogPixelsX
        {
            get
            {
                if (_logPixelsX == 0)
                {
                    SetupLogPixels();
                }

                return _logPixelsX;
            }
        }

        private static int LogPixelsY
        {
            get
            {
                if (_logPixelsY == 0)
                {
                    SetupLogPixels();
                }

                return _logPixelsY;
            }
        }

        private static int _logPixelsX;
        private static int _logPixelsY;
    }
}
