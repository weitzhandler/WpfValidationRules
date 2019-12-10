namespace System.Windows.Controls
{
  using System;
  using System.Collections;
  using System.ComponentModel.DataAnnotations;
  using System.Globalization;
  using System.Linq;
  using System.Windows;
  using System.Windows.Controls;

  using DaValidationResult = System.ComponentModel.DataAnnotations.ValidationResult;
  using WinValidationResult = System.Windows.Controls.ValidationResult;

  public static class DataAnnotationsBehavior
  {
    public static readonly DependencyProperty ValidateDataAnnotationsProperty =
        DependencyProperty.RegisterAttached(
          "ValidateDataAnnotations",
          typeof(bool),
          typeof(DataAnnotationsBehavior),
          new PropertyMetadata(false, OnValidateDataAnnotationsPropertyChanged));
    public static bool GetValidateDataAnnotations(DependencyObject obj) =>
      (bool)obj.GetValue(ValidateDataAnnotationsProperty);
    public static void SetValidateDataAnnotations(DependencyObject obj, bool value) =>
      obj.SetValue(ValidateDataAnnotationsProperty, value);
    static void OnValidateDataAnnotationsPropertyChanged(DependencyObject d,
      DependencyPropertyChangedEventArgs e)
    {
      var behaviorEnabled = (bool)e.NewValue;
      if (!(d is TextBox textBox))
        throw new NotSupportedException($"The behavior '{typeof(DataAnnotationsBehavior)}' "
          + "can only be applied on elements of type '{typeof(TextBox)}'.");

      var bindingExpression =
        textBox.GetBindingExpression(TextBox.TextProperty);
      var parent = bindingExpression.ParentBinding;

      if (behaviorEnabled)
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
          var context = new ValidationContext(dataItem, null, null)
          {
            MemberName = bindingExpression.ResolvedSourcePropertyName
          };
          parent.ValidationRules.Add(new AttributesValidationRule(context, validationAttr));
        }
      }
      else
      {
        var das =
          parent
            .ValidationRules
            .OfType<AttributesValidationRule>()
            .ToList();

        if (das != null)
          foreach (var da in das)
            parent.ValidationRules.Remove(da);
      }
    }


    public static readonly DependencyProperty ValidateTextBoxesDataAnnotationsProperty =
      DependencyProperty.RegisterAttached(
        "ValidateTextBoxesDataAnnotations",
        typeof(bool),
        typeof(DataAnnotationsBehavior),
        new PropertyMetadata(false, OnValidateTextBoxesDataAnnotationsPropertyChanged));

    public static bool GetValidateTextBoxesDataAnnotations(DependencyObject obj) =>
      (bool)obj.GetValue(ValidateTextBoxesDataAnnotationsProperty);
    public static void SetValidateTextBoxesDataAnnotations(DependencyObject obj, bool value) =>
      obj.SetValue(ValidateTextBoxesDataAnnotationsProperty, value);
    private static void OnValidateTextBoxesDataAnnotationsPropertyChanged(DependencyObject d,
      DependencyPropertyChangedEventArgs e)
    {
      var behaviorEnabled = (bool)e.NewValue;
      if (!(d is DataGrid dataGrid))
        throw new NotSupportedException($"The behavior '{typeof(DataAnnotationsBehavior)}' "
          + "can only be applied on elements of type '{typeof(DataGrid)}'.");

      if (behaviorEnabled)
        dataGrid.PreparingCellForEdit += preparingCellForEdit;
      else
        dataGrid.PreparingCellForEdit -= preparingCellForEdit;

      void preparingCellForEdit(object? sender, DataGridPreparingCellForEditEventArgs e)
      {
        if (!(e.Column is DataGridTextColumn && e.EditingElement is TextBox textBox))
          return;

        var tbType = typeof(TextBox);
        var resourcesStyle = Application
          .Current
          .Resources
          .Cast<DictionaryEntry>()
          .Where(de => de.Value is Style && de.Key is Type styleType && styleType == tbType)
          .Select(de => (Style)de.Value!)
          .FirstOrDefault();

        var style = new Style(typeof(TextBox), resourcesStyle);
        foreach (var setter in textBox.Style.Setters)
          style.Setters.Add(setter);

        textBox.Style = style;
        SetValidateDataAnnotations(textBox, true);
      }

    }

    abstract class DaValidationRule : ValidationRule
    {
      public ValidationContext ValidationContext { get; }

      public DaValidationRule(ValidationContext validationContext) =>
        ValidationContext = validationContext;
    }

    class AttributesValidationRule : DaValidationRule
    {
      public ValidationAttribute ValidationAttribute { get; }

      public AttributesValidationRule(ValidationContext validationContext,
        ValidationAttribute attribute)
        : base(validationContext) =>
        ValidationAttribute = attribute;

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
