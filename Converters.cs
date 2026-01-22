// Dodaj do App.xaml.cs lub nowy plik Converters.cs
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (value is bool && (bool)value) ? Visibility.Visible : Visibility.Collapsed;
    }
    
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value is Visibility visibility && visibility == Visibility.Visible;
    }
}
public interface IAIService
{
    Task<string> AskQuestionAsync(string question);
    Task<bool> TestConnectionAsync();
}
