CREATE DATABASE УчетСебестоимости;
GO

USE УчетСебестоимости;
GO

CREATE TABLE Материалы (
    ID_Материала INT IDENTITY(1,1) PRIMARY KEY,
    Название VARCHAR(255) NOT NULL,
    Единица_Измерения VARCHAR(50) NOT NULL,
    Цена_За_Единицу DECIMAL(12, 2) NOT NULL
);

CREATE TABLE Продукция (
    ID_Продукта INT IDENTITY(1,1) PRIMARY KEY,
    Название VARCHAR(255) NOT NULL,
    Накладные_Расходы DECIMAL(12, 2) DEFAULT 0,
    Себестоимость DECIMAL(12, 2) DEFAULT 0
);

CREATE TABLE Состав_Продукции (
    ID_Продукта INT REFERENCES Продукция(ID_Продукта) ON DELETE CASCADE,
    ID_Материала INT REFERENCES Материалы(ID_Материала) ON DELETE CASCADE,
    Количество DECIMAL(10, 4) NOT NULL,
    PRIMARY KEY (ID_Продукта, ID_Материала)
);

CREATE TABLE Трудозатраты (
    ID_Затраты INT IDENTITY(1,1) PRIMARY KEY,
    ID_Продукта INT REFERENCES Продукция(ID_Продукта) ON DELETE CASCADE,
    Название_Работы VARCHAR(255) NOT NULL,
    Стоимость_Работы DECIMAL(12, 2) NOT NULL
);

INSERT INTO Материалы (Название, Единица_Измерения, Цена_За_Единицу) VALUES
('Лист стали 2мм', 'м2', 1500.00),
('Краска черная', 'л', 450.00),
('Болт М8', 'шт', 15.50),
('Доска дубовая', 'м3', 45000.00);

INSERT INTO Продукция (Название, Накладные_Расходы) VALUES
('Стол лофт', 500.00),
('Стеллаж складской', 300.00);

INSERT INTO Состав_Продукции (ID_Продукта, ID_Материала, Количество) VALUES
(1, 4, 0.05),
(1, 1, 1.2), 
(1, 2, 0.5);

INSERT INTO Состав_Продукции (ID_Продукта, ID_Материала, Количество) VALUES
(2, 1, 3.5),
(2, 3, 20);

INSERT INTO Трудозатраты (ID_Продукта, Название_Работы, Стоимость_Работы) VALUES
(1, 'Распил дерева', 800.00),
(1, 'Сварка каркаса', 1200.00),
(2, 'Гибка металла', 600.00),
(2, 'Сборка', 400.00);
