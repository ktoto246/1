using System;
using System.Collections.Generic;

namespace WpfApp1.Models;

public partial class Трудозатраты
{
    public int IdЗатраты { get; set; }

    public int IdПродукта { get; set; }

    public string НазваниеРаботы { get; set; } = null!;

    public decimal СтоимостьРаботы { get; set; }

    public virtual Продукция IdПродуктаNavigation { get; set; } = null!;
}
