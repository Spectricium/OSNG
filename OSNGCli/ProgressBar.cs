using System;
using System.Text;
using System.Threading;

namespace OSNGCli
{
    /// <summary>
    /// An ASCII progress bar
    /// </summary>
    public class ProgressBar : IDisposable, IProgress<double>
    {
        private const int blockCount = 10;
        private double currentProgress = 2;
        private string currentText = string.Empty;
        private bool disposed = false;

        public ProgressBar()
        {
        }
        StringBuilder sb = new StringBuilder();
        public void Report(double value)
        {
            if (value == currentProgress)
                return;
            byte percent = (byte)(value * 100);
            currentProgress = value;
            //int progressBlockCount = (int)( currentProgress * blockCount );
            sb.Clear();
            /*
            sb.Append('[');
            sb.Append('#', progressBlockCount);
            sb.Append('-', blockCount - progressBlockCount);
            sb.Append(']');
            sb.Append(' ');*/
            sb.Append(percent);
            sb.Append('%');
            string text = sb.ToString();
            if (currentText == text)
                return;
            UpdateText(text);
        }
        public void UpdateText(string text)
        {
            // Get length of common portion
            int commonPrefixLength = 0;
            int commonLength = Math.Min(currentText.Length, text.Length);
            while (commonPrefixLength < commonLength && text[commonPrefixLength] == currentText[commonPrefixLength])
            {
                commonPrefixLength++;
            }
            // Backtrack to the first differing character
            StringBuilder outputBuilder = new StringBuilder();
            outputBuilder.Append('\b', currentText.Length - commonPrefixLength);

            // Output new suffix
            outputBuilder.Append(text.Substring(commonPrefixLength));

            // If the new text is shorter than the old one: delete overlapping characters
            int overlapCount = currentText.Length - text.Length;
            if (overlapCount > 0)
            {
                outputBuilder.Append(' ', overlapCount);
                outputBuilder.Append('\b', overlapCount);
            }

            Console.Write(outputBuilder);
            currentText = text;
        }

        public void Dispose()
        {
            disposed = true;
            UpdateText(string.Empty);
        }

    }
}
