using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Knapcode.TheQuickerSnipper.Views
{
    public class SelectionEventArgs : EventArgs
    {
        public SelectionEventArgs(Rect displaySelection, Int32Rect imageSelection)
        {
            DisplaySelection = displaySelection;
            ImageSelection = imageSelection;
        }

        public Rect DisplaySelection { get; private set; }

        public Int32Rect ImageSelection { get; private set; }
    }

    public partial class SelectionArea
    {
        public delegate void SelectionMadeHandler(object sender, SelectionEventArgs e);

        private Point _startPoint;

        public SelectionArea()
        {
            InitializeComponent();

            Drop += (sender, args) =>
            {
                if (args.Data.GetDataPresent(DataFormats.FileDrop))
                {
                    var filePaths = (string[]) args.Data.GetData(DataFormats.FileDrop);
                    FilePath = filePaths.FirstOrDefault();
                    if (FilePath != null)
                    {
                        Image.Source = new BitmapImage(new Uri(FilePath));
                    }
                }
            };
        }

        public BitmapSource ImageSource
        {
            get { return Image.Source as BitmapSource; }
            set { Image.Source = value; }
        }

        public string FilePath { get; private set; }

        public event SelectionMadeHandler SelectionMade;

        protected virtual void OnSelectionMade(SelectionEventArgs e)
        {
            SelectionMadeHandler handler = SelectionMade;
            if (handler != null) handler(this, e);
        }

        private void SelectionStart(object sender, MouseButtonEventArgs e)
        {
            if (!Grid.IsMouseCaptured)
            {
                _startPoint = e.GetPosition(this);
                SetSelection(e);
                Selection.Visibility = Visibility.Visible;
                Mouse.Capture(Grid);
            }
        }

        private void SelectionEnd(object sender, MouseButtonEventArgs e)
        {
            if (Grid.IsMouseCaptured)
            {
                Grid.ReleaseMouseCapture();
                Selection.Visibility = Visibility.Hidden;
                Rect displayRect = GetDisplayRect(e);
                Int32Rect imageRect = GetImageRect(displayRect);
                OnSelectionMade(new SelectionEventArgs(displayRect, imageRect));
            }
        }

        private Int32Rect GetImageRect(Rect displayRect)
        {
            double displayWidth = Image.ActualWidth;
            double displayHeight = Image.ActualHeight;

            double widthRatio;
            double heightRatio;
            var bitmapSource = (BitmapSource) Image.Source;
            if (bitmapSource != null)
            {
                widthRatio = bitmapSource.PixelWidth/displayWidth;
                heightRatio = bitmapSource.PixelHeight/displayHeight;
            }
            else
            {
                widthRatio = 1;
                heightRatio = 1;
            }

            Point topLeft = TranslatePoint(displayRect.TopLeft, Image);
            var x1 = (int) Math.Round(Math.Max(0, topLeft.X)*widthRatio);
            var y1 = (int) Math.Round(Math.Max(0, topLeft.Y)*heightRatio);

            Point bottomRight = TranslatePoint(displayRect.BottomRight, Image);
            var x2 = (int) Math.Round(Math.Min(displayWidth, bottomRight.X)*widthRatio);
            var y2 = (int) Math.Round(Math.Min(displayHeight, bottomRight.Y)*heightRatio);

            return new Int32Rect(x1, y1, x2 - x1, y2 - y1);
        }

        private void SelectionResize(object sender, MouseEventArgs e)
        {
            if (Grid.IsMouseCaptured)
            {
                SetSelection(e);
            }
        }

        private void SetSelection(MouseEventArgs e)
        {
            Rect rect = GetDisplayRect(e);

            Canvas.SetLeft(Selection, rect.X);
            Canvas.SetTop(Selection, rect.Y);
            Selection.Width = rect.Width;
            Selection.Height = rect.Height;
        }

        private Rect GetDisplayRect(MouseEventArgs e)
        {
            Point endPoint = e.GetPosition(this);

            // get the dimensions of the selection
            double x, width;
            if (_startPoint.X < endPoint.X)
            {
                x = _startPoint.X;
                width = endPoint.X - x;
            }
            else
            {
                x = endPoint.X;
                width = _startPoint.X - x;
            }

            double y, height;
            if (_startPoint.Y < endPoint.Y)
            {
                y = _startPoint.Y;
                height = endPoint.Y - y;
            }
            else
            {
                y = endPoint.Y;
                height = _startPoint.Y - y;
            }

            // limit the bounds of the selection
            if (x < 0)
            {
                width += x;
                x = 0;
            }
            if (y < 0)
            {
                height += y;
                y = 0;
            }
            if (x + width > ActualWidth)
            {
                width = ActualWidth - x;
            }
            if (y + height > ActualHeight)
            {
                height = ActualHeight - y;
            }

            return new Rect(x, y, width, height);
        }
    }
}