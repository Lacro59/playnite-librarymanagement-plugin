using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using CroppingImageLibrary.Services.State;
using CroppingImageLibrary.Services.Tools;

namespace CroppingImageLibrary.Services
{
    public class CropArea
    {
        public readonly Size OriginalSize;
        public readonly Rect CroppedRectAbsolute;

        public CropArea(Size originalSize, Rect croppedRectAbsolute)
        {
            OriginalSize = originalSize;
            CroppedRectAbsolute = croppedRectAbsolute;
        }
    }

    public class CropService
    {
        private CropAdorner _cropAdorner;
        private Canvas _canvas;
        private Tools.CropTool _cropTool;

        private IToolState _currentToolState;
        private IToolState _createState;
        private IToolState _dragState;
        private IToolState _completeState;

        public Adorner Adorner => _cropAdorner;

        private enum TouchPoint
        {
            OutsideRectangle,
            InsideRectangle
        }

        public CropService(FrameworkElement adornedElement)
        {
            FrameworkElement el = (FrameworkElement)adornedElement.FindName("SourceImage");

            _canvas = new Canvas
            {
                Height = el.ActualHeight,
                Width = el.ActualWidth
            };
            _cropAdorner = new CropAdorner(el, _canvas);
            var adornerLayer = AdornerLayer.GetAdornerLayer(el);
            Debug.Assert(adornerLayer != null, nameof(adornerLayer) + " != null");
            adornerLayer.Add(_cropAdorner);

            var cropShape = new CropShape(
                new Rectangle
                {
                    Height = 0,
                    Width = 0,
                    Stroke = Brushes.Black,
                    StrokeThickness = 1.5
                },
                new Rectangle
                {
                    Stroke = Brushes.White,
                    StrokeDashArray = new DoubleCollection(new double[] { 4, 4 })
                }
            );
            _cropTool = new CropTool(_canvas);
            _createState = new CreateState(_cropTool, _canvas);
            _completeState = new CompleteState();
            _dragState = new DragState(_cropTool, _canvas);
            _currentToolState = _completeState;

            _cropAdorner.MouseLeftButtonDown += AdornerOnMouseLeftButtonDown;
            _cropAdorner.MouseMove += AdornerOnMouseMove;
            _cropAdorner.MouseLeftButtonUp += AdornerOnMouseLeftButtonUp;

            _cropTool.Redraw(0, 0, 0, 0);
        }

        public void ShowText(bool isVisible)
        {
            _cropTool.ShowText(isVisible);
        }

        public CropArea GetCroppedArea() =>
            new CropArea(
                _cropAdorner.RenderSize,
                new Rect(_cropTool.TopLeftX, _cropTool.TopLeftY, _cropTool.Width, _cropTool.Height)
            );

        private void AdornerOnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _canvas.ReleaseMouseCapture();
            _currentToolState = _completeState;
        }

        private void AdornerOnMouseMove(object sender, MouseEventArgs e)
        {
            var point = e.GetPosition(_canvas);
            var newPosition = _currentToolState.OnMouseMove(point);
            if (newPosition.HasValue)
            {
                _cropTool.Redraw(newPosition.Value.Left, newPosition.Value.Top, (int)newPosition.Value.Width, (int)newPosition.Value.Height);
            }
        }

        public void SetKeepRatio(bool KeepRatio)
        {
            _cropTool.SetKeepRatio(KeepRatio);
            _cropTool.SetRatio((float)_cropTool.Width / (float)_cropTool.Height);
        }

        public void SetRatio(float Ratio)
        {
            _cropTool.SetRatio(Ratio);
        }

        public void SetCropArea(double Width, double Height)
        {
            _cropTool.Redraw(0, 0, Width, Height);
        }

        public void SetCropAreaRatio(double Width, double Height, int rWidth, int rHeight)
        {
            float ratio = (float)rWidth / (float)rHeight;

            SetRatio(ratio);

            double NewWidth = Width;
            double NewHeight = Height;

            if (ratio == 1)
            {
                if (Width >= Height)
                {
                    NewWidth = Height;
                }
                else
                {
                    NewHeight = Width;
                }
            }
            else if (ratio > 1)
            {
                NewHeight = (int)(1f * Width / ratio);

                if (NewHeight > Height)
                {
                    NewHeight = Width;
                    NewWidth = (int)(1f * Height * ratio);
                }
            }
            else
            {
                NewWidth = (int)(1f * Height * ratio);

                if (NewWidth > Width)
                {
                    NewWidth = Width;
                    NewHeight = (int)(1f * Width / ratio);
                }
            }

            _cropTool.Redraw(0, 0, (int)NewWidth, (int)NewHeight);
        }

        private void AdornerOnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _canvas.CaptureMouse();
            var point = e.GetPosition(_canvas);
            var touch = GetTouchPoint(point);
            if (touch == TouchPoint.OutsideRectangle)
            {
                _currentToolState = _createState;
            }
            else if (touch == TouchPoint.InsideRectangle)
            {
                _currentToolState = _dragState;
            }
            _currentToolState.OnMouseDown(point);
        }

        private TouchPoint GetTouchPoint(Point mousePoint)
        {
            //left
            if (mousePoint.X < _cropTool.TopLeftX)
                return TouchPoint.OutsideRectangle;
            //right
            if (mousePoint.X > _cropTool.BottomRightX)
                return TouchPoint.OutsideRectangle;
            //top
            if (mousePoint.Y < _cropTool.TopLeftY)
                return TouchPoint.OutsideRectangle;
            //bottom
            if (mousePoint.Y > _cropTool.BottomRightY)
                return TouchPoint.OutsideRectangle;

            return TouchPoint.InsideRectangle;
        }
    }
}
