using System;
using System.Collections.Generic;

namespace WpfApp1.Models;

public partial class Продукция
{
    public int IdПродукта { get; set; }

    public string Название { get; set; } = null!;

    public decimal НакладныеРасходы { get; set; }

    public decimal Себестоимость { get; set; }

    public virtual ICollection<СоставПродукции> СоставПродукцииs { get; set; } = new List<СоставПродукции>();

    public virtual ICollection<Трудозатраты> Трудозатратыs { get; set; } = new List<Трудозатраты>();
}
