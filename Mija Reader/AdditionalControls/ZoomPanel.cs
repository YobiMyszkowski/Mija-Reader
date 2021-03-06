﻿using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Mija_Reader.AdditionalControls
{
    public class ZoomPanel : Border
    {
        private UIElement child = null;
        private Point origin;
        private Point start;
        private TranslateTransform GetTranslateTransform(UIElement element)
        {
            return (TranslateTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is TranslateTransform);
        }
        private ScaleTransform GetScaleTransform(UIElement element)
        {
            return (ScaleTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is ScaleTransform);
        }
        public override UIElement Child
        {
            get { return base.Child; }
            set
            {
                if (value != null && value != this.Child)
                    this.Initialize(value);
                base.Child = value;
            }
        }

        public double currentScale = 0;

        public void Initialize(UIElement element)
        {
            this.child = element;
            if (child != null)
            {
                TransformGroup group = new TransformGroup();
                ScaleTransform st = new ScaleTransform();
                group.Children.Add(st);
                TranslateTransform tt = new TranslateTransform();
                group.Children.Add(tt);
                child.RenderTransform = group;
                child.RenderTransformOrigin = new Point(0.0, 0.0);
                this.MouseLeftButtonDown += ZoomPanel_MouseLeftButtonDown;
                this.MouseLeftButtonUp += ZoomPanel_MouseLeftButtonUp;
                this.MouseMove += ZoomPanel_MouseMove;

                this.MouseWheel += ZoomPanel_MouseWheel;
            }
        }
        public void Reset()
        {
            if (child != null)
            {
                currentScale = 0;
                // reset zoom
                var st = GetScaleTransform(child);
                st.ScaleX = 1.0;
                st.ScaleY = 1.0;

                // reset pan
                var tt = GetTranslateTransform(child);
                tt.X = 0.0;
                tt.Y = 0.0;
            }
        }
        #region Child Events
        public void ScaleUp(double maxZoom)
        {
            if (child != null)
            {
                var st = GetScaleTransform(child);
                var tt = GetTranslateTransform(child);

                double zoom = 0.1;

                if (st.ScaleX >= maxZoom + 1 || st.ScaleY >= maxZoom + 1)
                    return;

                if (st.ScaleX >= maxZoom + 1 || st.ScaleY >= maxZoom + 1)
                {
                    zoom = maxZoom;
                    currentScale = zoom;
                }
                if (zoom != maxZoom)
                {
                    Point relative = Mouse.GetPosition(child);
                    double abosuluteX;
                    double abosuluteY;

                    abosuluteX = relative.X * st.ScaleX + tt.X;
                    abosuluteY = relative.Y * st.ScaleY + tt.Y;

                    st.ScaleX += zoom;
                    st.ScaleY += zoom;

                    tt.X = abosuluteX - relative.X * st.ScaleX;
                    tt.Y = abosuluteY - relative.Y * st.ScaleY;
                }
            }
        }
        public void ScaleDown(double maxZoom)
        {
            if (child != null)
            {
                var st = GetScaleTransform(child);
                var tt = GetTranslateTransform(child);

                double zoom = -.1;

                if (st.ScaleX <= maxZoom || st.ScaleY <= maxZoom)
                    return;

                if (st.ScaleX <= maxZoom || st.ScaleY <= maxZoom)
                {
                    zoom = maxZoom;
                    currentScale = zoom;
                }
                if (zoom != maxZoom)
                {
                    Point relative = Mouse.GetPosition(child);
                    double abosuluteX;
                    double abosuluteY;

                    abosuluteX = relative.X * st.ScaleX + tt.X;
                    abosuluteY = relative.Y * st.ScaleY + tt.Y;

                    st.ScaleX += zoom;
                    st.ScaleY += zoom;

                    tt.X = abosuluteX - relative.X * st.ScaleX;
                    tt.Y = abosuluteY - relative.Y * st.ScaleY;
                }
            }
        }
        public void ZoomPanel_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (child != null)
            {
                double zoom = e.Delta > 0 ? .2 : -.2;

                if (zoom > 0)
                    ScaleUp(.5);
                if (zoom < 0)
                    ScaleDown(.5);
            }
        }
        private void ZoomPanel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (child != null)
            {
                var tt = GetTranslateTransform(child);
                start = e.GetPosition(this);
                origin = new Point(tt.X, tt.Y);
                this.Cursor = Cursors.Hand;
                child.CaptureMouse();
            }
        }
        private void ZoomPanel_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (child != null)
            {
                child.ReleaseMouseCapture();
                this.Cursor = Cursors.Arrow;
            }
        }
        private void ZoomPanel_MouseMove(object sender, MouseEventArgs e)
        {
            if (child != null)
            {
                if (child.IsMouseCaptured)
                {
                    var tt = GetTranslateTransform(child);
                    Vector v = start - e.GetPosition(this);
                    tt.X = origin.X - v.X;
                    tt.Y = origin.Y - v.Y;
                }
            }
        }
        private void ZoomPanel_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            this.Reset();
        }
        #endregion
    }
}
