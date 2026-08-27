// ViewModels/ViewModelBase.cs
using CommunityToolkit.Mvvm.ComponentModel;

namespace Libris.ViewModels;

/// <summary>
/// Базовый класс для всех ViewModel приложения Libris.
/// Предоставляет поддержку уведомлений об изменении свойств.
/// </summary>
public abstract class ViewModelBase : ObservableObject
{
}