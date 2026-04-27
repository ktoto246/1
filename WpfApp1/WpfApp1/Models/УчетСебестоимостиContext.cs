using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WpfApp1.Models;

public partial class УчетСебестоимостиContext : DbContext
{
    public УчетСебестоимостиContext()
    {
    }

    public УчетСебестоимостиContext(DbContextOptions<УчетСебестоимостиContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Материалы> Материалыs { get; set; }

    public virtual DbSet<Продукция> Продукцияs { get; set; }

    public virtual DbSet<СоставПродукции> СоставПродукцииs { get; set; }

    public virtual DbSet<Трудозатраты> Трудозатратыs { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Server=PC;Database=УчетСебестоимости;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Материалы>(entity =>
        {
            entity.HasKey(e => e.IdМатериала).HasName("PK__Материал__7A6D886D");

            entity.ToTable("Материалы");

            entity.Property(e => e.IdМатериала).HasColumnName("ID_Материала");
            entity.Property(e => e.ЕдиницаИзмерения)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("Единица_Измерения");
            entity.Property(e => e.Название)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.ЦенаЗаЕдиницу)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("Цена_За_Единицу");
        });

        modelBuilder.Entity<Продукция>(entity =>
        {
            entity.HasKey(e => e.IdПродукта).HasName("PK__Продукци__189736B0");

            entity.ToTable("Продукция");

            entity.Property(e => e.IdПродукта).HasColumnName("ID_Продукта");
            entity.Property(e => e.НакладныеРасходы)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("Накладные_Расходы");
            entity.Property(e => e.Название)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Себестоимость)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(12, 2)");
        });

        modelBuilder.Entity<СоставПродукции>(entity =>
        {
            entity.HasKey(e => new { e.IdПродукта, e.IdМатериала }).HasName("PK__Состав_П__69C9523F");

            entity.ToTable("Состав_Продукции");

            entity.Property(e => e.IdПродукта).HasColumnName("ID_Продукта");
            entity.Property(e => e.IdМатериала).HasColumnName("ID_Материала");
            entity.Property(e => e.Количество).HasColumnType("decimal(10, 4)");

            entity.HasOne(d => d.IdМатериалаNavigation).WithMany(p => p.СоставПродукцииs)
                .HasForeignKey(d => d.IdМатериала)
                .HasConstraintName("FK__Состав_Пр__ID_Ма__403A8C7D");

            entity.HasOne(d => d.IdПродуктаNavigation).WithMany(p => p.СоставПродукцииs)
                .HasForeignKey(d => d.IdПродукта)
                .HasConstraintName("FK__Состав_Пр__ID_Пр__3F466844");
        });

        modelBuilder.Entity<Трудозатраты>(entity =>
        {
            entity.HasKey(e => e.IdЗатраты).HasName("PK__Трудозат__0B6E6A1B");

            entity.ToTable("Трудозатраты");

            entity.Property(e => e.IdЗатраты).HasColumnName("ID_Затраты");
            entity.Property(e => e.IdПродукта).HasColumnName("ID_Продукта");
            entity.Property(e => e.НазваниеРаботы)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("Название_Работы");
            entity.Property(e => e.СтоимостьРаботы)
                .HasColumnType("decimal(12, 2)")
                .HasColumnName("Стоимость_Работы");

            entity.HasOne(d => d.IdПродуктаNavigation).WithMany(p => p.Трудозатратыs)
                .HasForeignKey(d => d.IdПродукта)
                .HasConstraintName("FK__Трудозатр__ID_Пр__4316F928");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
