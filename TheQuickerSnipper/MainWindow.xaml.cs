using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace PhotoSnipper
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            SuffixSeperator = "_";

            SelectionArea.SelectionMade += (sender, args) =>
            {
                // skip the selection if no file is loaded
                string filePath = SelectionArea.FilePath;
                if (SelectionArea.FilePath == null)
                {
                    return;
                }

                // skip the selection if there is no width or height
                if (args.ImageSelection.Width == 0 || args.ImageSelection.Height == 0)
                {
                    return;
                }


                var image = new CroppedBitmap(SelectionArea.ImageSource, args.ImageSelection);
                var encoder = new JpegBitmapEncoder();
                encoder.QualityLevel = 95;
                encoder.Frames.Add(BitmapFrame.Create(image));

                var memoryStream = new MemoryStream();
                encoder.Save(memoryStream);
                memoryStream.Position = 0;

                Task.Factory.StartNew(() =>
                {
                    // parse the old file name
                    string directory = Path.GetDirectoryName(filePath) ?? ".";
                    string fileName = Path.GetFileNameWithoutExtension(filePath) ?? string.Empty;
                    string extension = ".jpg";

                    // get highest number suffix
                    string[] otherPaths = Directory.GetFiles(directory, fileName + "*");
                    int maxSuffixInt = 0;
                    int prefixLength = fileName.Length + SuffixSeperator.Length;
                    foreach (string otherPath in otherPaths)
                    {
                        string otherFileName = Path.GetFileNameWithoutExtension(otherPath) ?? string.Empty;
                        if (otherFileName.Length < prefixLength)
                        {
                            continue;
                        }

                        string suffix = otherFileName.Substring(prefixLength);
                        int suffixInt;
                        if (int.TryParse(suffix, out suffixInt))
                        {
                        }
                        maxSuffixInt = Math.Max(maxSuffixInt, suffixInt);
                    }

                    // generate the new file name
                    string newFilePath = Path.Combine(directory, fileName + SuffixSeperator + (maxSuffixInt + 1) + extension);

                    // write the file
                    using (var stream = new FileStream(newFilePath, FileMode.Create))
                    {
                        memoryStream.CopyTo(stream);
                    }

                    memoryStream.Dispose();
                });
            };
        }

        public string SuffixSeperator { get; set; }
    }
}