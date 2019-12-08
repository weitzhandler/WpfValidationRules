namespace WpfValidationRules
{
  using System;
  using System.ComponentModel.DataAnnotations;
  using System.Globalization;
  using System.Linq;
  using System.Windows;
  using System.Windows.Controls;

  using DaValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;
  using WinValidationResult = System.Windows.Controls.ValidationResult;

  public sealed class DataAnnotationsBehavior
  {
    public static bool GetValidateDataAnnotations(DependencyObject obj) =>
      (bool)obj.GetValue(ValidateDataAnnotationsProperty);
    public static void SetValidateDataAnnotations(DependencyObject obj, bool value) =>
      obj.SetValue(ValidateDataAnnotationsProperty, value);

    public static readonly DependencyProperty ValidateDataAnnotationsProperty =
        DependencyProperty.RegisterAttached("ValidateDataAnnotations", typeof(bool), typeof(DataAnnotationsBehavior), new PropertyMetadata(false, OnPropertyChanged));

    private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var boolean = (bool)e.NewValue;
      if (!(d is TextBox textBox))
        throw new NotSupportedException($"The behavior '{typeof(DataAnnotationsBehavior)}' can only be applied on elements of type '{typeof(TextBox)}'.");

      var bindingExpression =
        textBox.GetBindingExpression(TextBox.TextProperty);

      if (boolean)
      {
        var dataItem = bindingExpression.DataItem;
        if (bindingExpression.DataItem == null)
          return;

        var type = dataItem.GetType();
        var prop = type.GetProperty(bindingExpression.ResolvedSourcePropertyName);
        if (prop == null)
          return;

        var allAttributes = prop.GetCustomAttributes(typeof(ValidationAttribute), true);
        foreach (var validationAttr in allAttributes.OfType<ValidationAttribute>())
        {
          var context = new ValidationContext(dataItem, null, null) { MemberName = bindingExpression.ResolvedSourcePropertyName };
          bindingExpression.ParentBinding.ValidationRules.Add(new AttributesValidationRule(context, validationAttr));
        }
      }
      else
      {
        var das =
          bindingExpression
            .ParentBinding
            .ValidationRules
            .OfType<AttributesValidationRule>()
            .ToList();

        if (das != null)
          foreach (var da in das)
            bindingExpression.ParentBinding.ValidationRules.Remove(da);
      }
    }

    abstract class DaValidationRule : ValidationRule
    {
      public ValidationContext ValidationContext { get; }

      public DaValidationRule(ValidationContext validationContext)
      {
        ValidationContext = validationContext;
      }
    }

    class AttributesValidationRule : DaValidationRule
    {
      public ValidationAttribute ValidationAttribute { get; }

      public AttributesValidationRule(ValidationContext validationContext, ValidationAttribute attribute)
        : base(validationContext)
      {
        ValidationAttribute = attribute;
      }

      public override WinValidationResult Validate(object value, CultureInfo cultureInfo)
      {
        var result = ValidationAttribute.GetValidationResult(value, ValidationContext);

        return result == DaValidationResult.Success
          ? WinValidationResult.ValidResult
          : new WinValidationResult(false, result.ErrorMessage);
      }
    }
  }
}
