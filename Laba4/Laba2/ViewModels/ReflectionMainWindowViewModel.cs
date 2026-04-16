using CommunityToolkit.Mvvm.ComponentModel;
using PluginContracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Runtime.CompilerServices;

namespace Laba2.ViewModels;

public sealed class ReflectionMainWindowViewModel : ViewModelBase
{
    private readonly CommunityToolkit.Mvvm.Input.RelayCommand _loadAssemblyCommand;
    private readonly CommunityToolkit.Mvvm.Input.RelayCommand _executeCommand;

    private string _assemblyPath = string.Empty;
    private PluginTypeViewModel? _selectedPlugin;
    private ConstructorViewModel? _selectedConstructor;
    private MethodViewModel? _selectedMethod;

    public ReflectionMainWindowViewModel()
    {
        Plugins = new ObservableCollection<PluginTypeViewModel>();
        Constructors = new ObservableCollection<ConstructorViewModel>();
        Methods = new ObservableCollection<MethodViewModel>();
        ConstructorParameters = new ObservableCollection<ParameterInputViewModel>();
        MethodParameters = new ObservableCollection<ParameterInputViewModel>();

        StatusMessage = "Введите путь к DLL и нажмите «Загрузить».";

        _loadAssemblyCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(LoadAssembly, CanLoadAssembly);
        _executeCommand = new CommunityToolkit.Mvvm.Input.RelayCommand(ExecuteSelectedMethod, CanExecuteSelectedMethod);
    }

    public string AssemblyPath
    {
        get => _assemblyPath;
        set
        {
            if (SetProperty(ref _assemblyPath, value))
            {
                _loadAssemblyCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public ObservableCollection<PluginTypeViewModel> Plugins { get; }

    public PluginTypeViewModel? SelectedPlugin
    {
        get => _selectedPlugin;
        set
        {
            if (SetProperty(ref _selectedPlugin, value))
            {
                UpdateForSelectedPlugin();
                _executeCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public ObservableCollection<ConstructorViewModel> Constructors { get; }

    public ConstructorViewModel? SelectedConstructor
    {
        get => _selectedConstructor;
        set
        {
            if (SetProperty(ref _selectedConstructor, value))
            {
                UpdateConstructorParameters();
                _executeCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public ObservableCollection<MethodViewModel> Methods { get; }

    public MethodViewModel? SelectedMethod
    {
        get => _selectedMethod;
        set
        {
            if (SetProperty(ref _selectedMethod, value))
            {
                UpdateMethodParameters();
                _executeCommand.NotifyCanExecuteChanged();
            }
        }
    }

    public ObservableCollection<ParameterInputViewModel> ConstructorParameters { get; }

    public ObservableCollection<ParameterInputViewModel> MethodParameters { get; }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public CommunityToolkit.Mvvm.Input.RelayCommand LoadAssemblyCommand => _loadAssemblyCommand;
    public CommunityToolkit.Mvvm.Input.RelayCommand ExecuteCommand => _executeCommand;

    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(t => t != null)!;
        }
    }

    private bool CanLoadAssembly()
    {
        return !string.IsNullOrWhiteSpace(AssemblyPath);
    }

    private void LoadAssembly()
    {
        try
        {
            Plugins.Clear();
            Constructors.Clear();
            Methods.Clear();
            ConstructorParameters.Clear();
            MethodParameters.Clear();
            SelectedPlugin = null;
            SelectedConstructor = null;
            SelectedMethod = null;

            if (string.IsNullOrWhiteSpace(AssemblyPath))
            {
                StatusMessage = "Путь к DLL пустой.";
                return;
            }

            var fullPath = Path.GetFullPath(AssemblyPath);
            if (!File.Exists(fullPath))
            {
                StatusMessage = $"Файл не найден: {fullPath}";
                return;
            }

            var loadContext = AssemblyLoadContext.Default;
            var assembly = loadContext.LoadFromAssemblyPath(fullPath);

            var pluginInterface = typeof(IReflectivePlugin);
            var pluginTypes = SafeGetTypes(assembly)
                .Where(t => t.IsClass && !t.IsAbstract && pluginInterface.IsAssignableFrom(t))
                .OrderBy(t => t.FullName)
                .ToList();

            foreach (var type in pluginTypes)
            {
                Plugins.Add(new PluginTypeViewModel(type));
            }

            StatusMessage = pluginTypes.Count == 0
                ? "Подходящих классов не найдено."
                : $"Загружено: {pluginTypes.Count} классов, реализующих {pluginInterface.Name}.";

            SelectedPlugin = Plugins.FirstOrDefault();
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка загрузки DLL: {ex.Message}";
        }
        finally
        {
            _loadAssemblyCommand.NotifyCanExecuteChanged();
            _executeCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanExecuteSelectedMethod()
    {
        return SelectedPlugin != null && SelectedMethod != null;
    }

    private void UpdateForSelectedPlugin()
    {
        Constructors.Clear();
        Methods.Clear();
        ConstructorParameters.Clear();
        MethodParameters.Clear();

        SelectedConstructor = null;
        SelectedMethod = null;

        if (SelectedPlugin == null)
        {
            return;
        }

        var type = SelectedPlugin.PluginType;

        // Публичные конструкторы
        var ctors = type
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .OrderByDescending(c => c.GetParameters().Length)
            .ThenBy(c => c.ToString())
            .ToList();

        foreach (var ctor in ctors)
        {
            Constructors.Add(new ConstructorViewModel(ctor));
        }

        SelectedConstructor = Constructors.FirstOrDefault();

        // Публичные методы, объявленные в классе
        var methods = type
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => !m.IsSpecialName)
            .Where(m => !m.ContainsGenericParameters)
            .Where(m => m.GetParameters().All(p => !p.ParameterType.IsByRef))
            .OrderBy(m => m.Name)
            .ThenBy(m => m.GetParameters().Length)
            .ToList();

        foreach (var method in methods)
        {
            Methods.Add(new MethodViewModel(method));
        }

        SelectedMethod = Methods.FirstOrDefault();
    }

    private void UpdateConstructorParameters()
    {
        ConstructorParameters.Clear();

        if (SelectedConstructor == null)
        {
            return;
        }

        foreach (var parameter in SelectedConstructor.Constructor.GetParameters())
        {
            ConstructorParameters.Add(ParameterInputViewModel.FromParameter(parameter));
        }
    }

    private void UpdateMethodParameters()
    {
        MethodParameters.Clear();

        if (SelectedMethod == null)
        {
            return;
        }

        foreach (var parameter in SelectedMethod.Method.GetParameters())
        {
            MethodParameters.Add(ParameterInputViewModel.FromParameter(parameter));
        }
    }

    private void ExecuteSelectedMethod()
    {
        try
        {
            if (SelectedPlugin == null || SelectedMethod == null)
            {
                StatusMessage = "Не выбран класс/метод.";
                return;
            }

            object instance = CreateInstance();

            var methodArgs = ParseArguments(MethodParameters, "метода");
            if (methodArgs == null)
            {
                return;
            }

            var result = SelectedMethod.Method.Invoke(instance, methodArgs);

            if (SelectedMethod.Method.ReturnType == typeof(void))
            {
                StatusMessage = "Выполнено успешно (void).";
            }
            else
            {
                StatusMessage = result == null ? "Выполнено успешно (null)." : $"Результат: {result}";
            }
        }
        catch (TargetInvocationException ex)
        {
            var inner = ex.InnerException?.Message ?? ex.Message;
            StatusMessage = $"Ошибка выполнения метода: {inner}";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Ошибка выполнения: {ex.Message}";
        }
        finally
        {
            _executeCommand.NotifyCanExecuteChanged();
        }
    }

    private object CreateInstance()
    {
        if (SelectedConstructor != null)
        {
            var ctorArgs = ParseArguments(ConstructorParameters, "конструктора");
            if (ctorArgs == null)
            {
                throw new InvalidOperationException("Аргументы конструктора не удалось распарсить.");
            }

            return SelectedConstructor.Constructor.Invoke(ctorArgs);
        }

        // На всякий случай: если выбранный конструктор отсутствует, пытаемся параметless
        return Activator.CreateInstance(SelectedPlugin!.PluginType)
               ?? throw new InvalidOperationException("Не удалось создать экземпляр класса.");
    }

    private object[]? ParseArguments(
        IReadOnlyCollection<ParameterInputViewModel> inputs,
        string whatFor)
    {
        if (inputs.Count == 0)
        {
            return Array.Empty<object>();
        }

        var args = new object[inputs.Count];
        int i = 0;

        foreach (var input in inputs)
        {
            if (!TryParse(input.ValueText, input.ParameterType, out var value))
            {
                StatusMessage = $"Невозможно преобразовать аргумент [{input.ParameterName}] для {whatFor}: тип {input.ParameterTypeName}.";
                return null;
            }

            args[i++] = value!;
        }

        return args;
    }

    private static bool TryParse(string? valueText, Type targetType, out object? value)
    {
        value = null;

        var text = valueText?.Trim();
        var nonNullableType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        // null/пусто для nullable
        if (nonNullableType != targetType)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                value = null;
                return true;
            }
        }

        if (nonNullableType == typeof(string))
        {
            value = text ?? string.Empty;
            return true;
        }

        if (nonNullableType == typeof(int))
        {
            var normalized = text?.Replace(',', '.');
            return int.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) && Assign(v, out value);
        }

        if (nonNullableType == typeof(long))
        {
            var normalized = text?.Replace(',', '.');
            return long.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) && Assign(v, out value);
        }

        if (nonNullableType == typeof(short))
        {
            var normalized = text?.Replace(',', '.');
            return short.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) && Assign(v, out value);
        }

        if (nonNullableType == typeof(byte))
        {
            var normalized = text?.Replace(',', '.');
            return byte.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) && Assign(v, out value);
        }

        if (nonNullableType == typeof(uint))
        {
            var normalized = text?.Replace(',', '.');
            return uint.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) && Assign(v, out value);
        }

        if (nonNullableType == typeof(ulong))
        {
            var normalized = text?.Replace(',', '.');
            return ulong.TryParse(normalized, NumberStyles.Integer, CultureInfo.InvariantCulture, out var v) && Assign(v, out value);
        }

        if (nonNullableType == typeof(float))
        {
            var normalized = text?.Replace(',', '.');
            return float.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) && Assign(v, out value);
        }

        if (nonNullableType == typeof(double))
        {
            var normalized = text?.Replace(',', '.');
            return double.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) && Assign(v, out value);
        }

        if (nonNullableType == typeof(decimal))
        {
            var normalized = text?.Replace(',', '.');
            return decimal.TryParse(normalized, NumberStyles.Float, CultureInfo.InvariantCulture, out var v) && Assign(v, out value);
        }

        if (nonNullableType == typeof(bool))
        {
            if (string.Equals(text, "1", StringComparison.Ordinal))
            {
                value = true;
                return true;
            }

            if (string.Equals(text, "0", StringComparison.Ordinal))
            {
                value = false;
                return true;
            }

            if (bool.TryParse(text, out var v))
            {
                value = v;
                return true;
            }

            return false;
        }

        if (nonNullableType == typeof(char))
        {
            if (!string.IsNullOrWhiteSpace(text) && text!.Length == 1)
            {
                value = text[0];
                return true;
            }

            return false;
        }

        if (nonNullableType == typeof(DateTime))
        {
            return DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var v) && Assign(v, out value);
        }

        if (nonNullableType == typeof(Guid))
        {
            return Guid.TryParse(text, out var v) && Assign(v, out value);
        }

        if (nonNullableType.IsEnum)
        {
            return Enum.TryParse(nonNullableType, text, ignoreCase: true, out var v) && Assign(v, out value);
        }

        if (nonNullableType == typeof(object))
        {
            value = text ?? string.Empty;
            return true;
        }

        return false;

        static bool Assign<T>(T parsedValue, out object? outValue)
        {
            outValue = parsedValue!;
            return true;
        }
    }
}

public sealed class PluginTypeViewModel
{
    public PluginTypeViewModel(Type pluginType)
    {
        PluginType = pluginType;
        DisplayName = pluginType.Name;
    }

    public Type PluginType { get; }
    public string DisplayName { get; }

    public override string ToString() => DisplayName;
}

public sealed class ConstructorViewModel
{
    public ConstructorViewModel(ConstructorInfo constructor)
    {
        Constructor = constructor;
        Signature = BuildSignature(constructor);
    }

    public ConstructorInfo Constructor { get; }
    public string Signature { get; }

    private static string BuildSignature(ConstructorInfo ctor)
    {
        var parameters = ctor.GetParameters()
            .Select(p => $"{p.ParameterType.Name} {p.Name}");

        return $"({string.Join(", ", parameters)})";
    }

    public override string ToString() => Signature;
}

public sealed class MethodViewModel
{
    public MethodViewModel(MethodInfo method)
    {
        Method = method;
        Signature = BuildSignature(method);
    }

    public MethodInfo Method { get; }
    public string Signature { get; }

    private static string BuildSignature(MethodInfo method)
    {
        var parameters = method.GetParameters()
            .Select(p => p.ParameterType.Name);

        return $"{method.Name}({string.Join(", ", parameters)}) : {method.ReturnType.Name}";
    }

    public override string ToString() => Signature;
}

public sealed class ParameterInputViewModel : ObservableObject
{
    private string _valueText = string.Empty;

    private ParameterInputViewModel(ParameterInfo parameter)
    {
        Parameter = parameter;
        ParameterName = parameter.Name ?? "param";
        ParameterType = parameter.ParameterType;
        ParameterTypeName = ParameterType.Name;
        ParameterLabel = $"{ParameterName}: {ParameterTypeName}";
        ValueText = parameter.HasDefaultValue && parameter.DefaultValue != null
            ? Convert.ToString(parameter.DefaultValue, CultureInfo.InvariantCulture) ?? string.Empty
            : string.Empty;
    }

    public static ParameterInputViewModel FromParameter(ParameterInfo parameter) => new(parameter);

    public ParameterInfo Parameter { get; }
    public string ParameterName { get; }
    public string ParameterTypeName { get; }
    public string ParameterLabel { get; }
    public Type ParameterType { get; }

    public string ValueText
    {
        get => _valueText;
        set => SetProperty(ref _valueText, value);
    }
}

