using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WpfValidationRules
{
  public class Model : INotifyPropertyChanged
  {
    string? _Name;
    [Required]
    [Display(Name = "Nickname")]
    public string? Name
    {
      get => _Name;
      set
      {
        if (value == _Name)
          return;

        _Name = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
      }
    }


    public event PropertyChangedEventHandler? PropertyChanged;
  }
}
