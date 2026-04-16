Задание 4. Рефлексия — динамический вызов методов
Оконное приложение (MVVM), которое позволяет загрузить внешнюю DLL, найти все классы, реализующие нужный интерфейс, показать список классов, а затем при выборе — динамически отобразить конструкторы и методы выбранного класса с полями ввода параметров. По кнопке «ВЫПОЛНИТЬ» создаётся объект и вызывается выбранный метод.

1) Общая структура

<img width="651" height="192" alt="image" src="https://github.com/user-attachments/assets/6d9134fc-3259-4957-9257-2292ef53171f" />


2) Контракт и тестовые классы
PluginContracts
IReflectivePlugin — маркерный интерфейс для классов, которые нужно находить в загруженной DLL.
PluginLibrary (пример реализации)
Классы реализуют IReflectivePlugin и имеют конструкторы с параметрами + методы с параметрами:

MathOperations(int offset, double multiplier)
Add(int a, int b) : int
Divide(double a, double b) : double
StringTools(string prefix)
Concat(string a, string b) : string
Repeat(string text, int count) : string
Length(string text) : int
ColorTools(SampleColor baseColor)
DescribeColor(SampleColor color) : string
SampleColor — enum Red/Green/Blue
Инициализация значений делается через параметры конструкторов, как и требуется по условию.

Сборка и запуск

Сборка:
dotnet build "D:\tasks\cSharp\Laba4\Laba2\Laba2.csproj"

Перед запуском убедитесь, что есть DLL тестового модуля
