﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace WpfValidationRules
{
  public class ViewModel
  {
    public Model Model { get; } = new Model();

    public IList<Model> Models { get; }

    public ViewModel()
    {
      Models = new List<Model> { Model };
    }
  }
}
