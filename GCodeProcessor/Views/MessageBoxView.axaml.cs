using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace GCodeProcessor.Views
{
    public partial class MessageBoxView : UserControl
    {
        public MessageBoxView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Find the parent window and close it if it's a modal dialog.
        /// </summary>
        private void Close(object sender, RoutedEventArgs args)
        {
            Window? parentWindow = this.GetVisualRoot() as Window;
            parentWindow?.Close();
        }
    }
}
