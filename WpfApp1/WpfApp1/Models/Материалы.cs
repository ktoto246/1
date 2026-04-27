using System;
using System.Collections.Generic;

namespace WpfApp1.Models;

public partial class Материалы
{
    public int IdМатериала { get; set; }

    public string Название { get; set; } = null!;

    public string ЕдиницаИзмерения { get; set; } = null!;

    public decimal ЦенаЗаЕдиницу { get; set; }

    public virtual ICollection<СоставПродукции> СоставПродукцииs { get; set; } = new List<СоставПродукции>();
}
