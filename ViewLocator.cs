// ViewLocator.cs
using System;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Libris.ViewModels;

namespace Libris;

/// <summary>
/// Определяет соответствующее представление (<see cref="Control"/>)
/// для переданной модели представления (<see cref="ViewModelBase"/>).
/// Использует соглашение об именовании для автоматического поиска View.
/// </summary>
/// <remarks>
/// Например, <c>LibraryViewModel</c> преобразуется в <c>LibraryView</c>.
/// </remarks>
[RequiresUnreferencedCode(
    "Default implementation of ViewLocator involves reflection which may be trimmed away.",
    Url = "https://docs.avaloniaui.net/docs/concepts/view-locator")]
public class ViewLocator : IDataTemplate
{
    /// <summary>
    /// Создаёт представление, соответствующее переданной ViewModel.
    /// </summary>
    /// <param name="param">
    /// Экземпляр ViewModel, для которого необходимо найти View.
    /// </param>
    /// <returns>
    /// Созданное представление или <see langword="null"/>, если параметр отсутствует.
    /// Если соответствующий тип не найден, возвращается сообщение об ошибке.
    /// </returns>
    public Control? Build(object? param)
    {
        if (param is null)
            return null;

        var name = param.GetType()
            .FullName!
            .Replace(
                "ViewModel",
                "View",
                StringComparison.Ordinal);

        var type = Type.GetType(name);

        if (type is not null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        return new TextBlock
        {
            Text = "Not Found: " + name
        };
    }

    /// <summary>
    /// Определяет, может ли данный объект быть обработан этим шаблоном.
    /// </summary>
    /// <param name="data">Объект, для которого выполняется проверка.</param>
    /// <returns>
    /// <see langword="true"/>, если объект является экземпляром
    /// <see cref="ViewModelBase"/>; иначе <see langword="false"/>.
    /// </returns>
    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}