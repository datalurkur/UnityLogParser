using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;

namespace UnityLogParser
{
    public partial class MainWindow
    {
        public MainWindow() { InitializeComponent(); }

        public void OpenLogfile(object sender, RoutedEventArgs e)
        {
            LogListView.Items.Clear();
            OpenFileDialog dialog = new OpenFileDialog();
            if (dialog.ShowDialog() != true) { return; }

            List<string> logText = File.ReadAllLines(dialog.FileName).ToList();
            int startOfLog = 0;
            int beginOfStack = -1;
            for (int i = 0; i < logText.Count; ++i)
            {
                if (beginOfStack == -1 && (
                        logText[i].StartsWith("UnityEngine.Logger:Log") ||
                        logText[i].StartsWith("BPKKFEHPPEJ:Log") ||
                        logText[i].StartsWith("RigidbodyEx:set_parent")))
                {
                    beginOfStack = i;
                    continue;
                }
                if (string.IsNullOrWhiteSpace(logText[i]) && beginOfStack != -1)
                {
                    int endOfStack = i++;
                    string filename = logText[i++];
                    if (filename.StartsWith("[") && logText[i].StartsWith("(Filename: "))
                    {
                        filename += "\n" + logText[i++];
                    }
                    else if (!filename.StartsWith("(Filename: "))
                    {
                        return;
                    }

                    List<string> loglines = logText.GetRange(startOfLog, beginOfStack - startOfLog);
                    List<string> stacktrace = logText.GetRange(beginOfStack, endOfStack - beginOfStack - 1);

                    string joinedLogLines = string.Join("\n", loglines);
                    string joinedStacktrace = string.Join("\n", stacktrace);

                    bool isErrorOrException = joinedStacktrace.Contains("Error") || stacktrace.Contains("Exception");
                    bool isWarning = joinedStacktrace.Contains("Warning");

                    startOfLog = ++i;
                    beginOfStack = -1;

                    TextBlock textBlock = new TextBlock();
                    textBlock.PreviewMouseDown += (o, args) => OnLogDataClicked(joinedStacktrace, filename);
                    textBlock.Text = joinedLogLines;
                    if (isErrorOrException)
                    {
                        textBlock.Background = new SolidColorBrush(Colors.DarkRed);
                        textBlock.Foreground = new SolidColorBrush(Colors.White);
                    }
                    else if (isWarning)
                    {
                        textBlock.Background = new SolidColorBrush(Colors.Yellow);
                    }
                    LogListView.Items.Add(textBlock);
                }
            }
        }

        private void OnLogDataClicked(string stackTrace, string filename)
        {
            StackTraceBox.Text = stackTrace;
            FileNameBox.Text = filename;
        }
    }
}