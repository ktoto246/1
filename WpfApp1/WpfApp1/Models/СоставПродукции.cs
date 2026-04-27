using System;
using System.Collections.Generic;

namespace WpfApp1.Models;

public partial class СоставПродукции
{
    public int IdПродукта { get; set; }

    public int IdМатериала { get; set; }

    public decimal Количество { get; set; }

    public virtual Материалы IdМатериалаNavigation { get; set; } = null!;

    public virtual Продукция IdПродуктаNavigation { get; set; } = null!;
}
