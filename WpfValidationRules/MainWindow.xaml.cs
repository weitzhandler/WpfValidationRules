using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfValidationRules
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      InitializeComponent();
    }

    private void DataGrid_PreparingCellForEdit(object sender, DataGridPreparingCellForEditEventArgs e)
    {
      if (!(e.Column is DataGridTextColumn && e.EditingElement is TextBox textBox))
        return;      
      var style = new Style(typeof(TextBox), textBox.Style);
      style.Setters.Add(new Setter { Property = ForegroundProperty, Value = Brushes.Red });
      textBox.Style = style;
    }
  }
}
