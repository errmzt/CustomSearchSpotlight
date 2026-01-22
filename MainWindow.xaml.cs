public void ToggleVisibility()
{
    if (IsVisible)
    {
        Hide();
    }
    else
    {
        Show();
        Activate(); // Przenieś na wierzch
        SearchTextBox.Focus(); // Ustaw kursor w polu wyszukiwania
    }
}
<local:GlassWindow x:Class="CustomSearchApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:CustomSearchApp.Windows"
        mc:Ignorable="d"
        Title="SearchSpotlight" Height="600" Width="800"
        Topmost="True" ShowInTaskbar="False">

    <Window.Resources>
        <!-- Styl dla przezroczystego TextBox -->
        <Style TargetType="TextBox">
            <Setter Property="Background" Value="#20FFFFFF"/>
            <Setter Property="Foreground" Value="White"/>
            <Setter Property="BorderBrush" Value="#40FFFFFF"/>
            <Setter Property="BorderThickness" Value="1"/>
            <Setter Property="Padding" Value="15"/>
            <Setter Property="FontSize" Value="16"/>
            <Setter Property="CaretBrush" Value="White"/>
        </Style>
    </Window.Resources>

    <Grid>
        <!-- Tło z rozmyciem -->
        <Border Background="#80111111" CornerRadius="15" Margin="20">
            <Border.Effect>
                <BlurEffect Radius="15"/>
            </Border.Effect>
        </Border>

        <!-- Główna zawartość -->
        <StackPanel Margin="40" VerticalAlignment="Top">
            <!-- Pole wyszukiwania -->
            <TextBox x:Name="SearchBox" 
                     Text="{Binding SearchQuery, UpdateSourceTrigger=PropertyChanged}"
                     Height="55" VerticalContentAlignment="Center">
                <TextBox.InputBindings>
                    <KeyBinding Key="Esc" Command="{Binding ClearSearchCommand}"/>
                </TextBox.InputBindings>
            </TextBox>

            <!-- Lista wyników -->
            <ListView ItemsSource="{Binding SearchResults}" 
                      SelectedItem="{Binding SelectedResult}"
                      Margin="0,20,0,0"
                      Background="Transparent"
                      BorderThickness="0"
                      MaxHeight="400">
                <ListView.ItemTemplate>
                    <DataTemplate>
                        <Border Padding="10" Margin="2" Background="#10000000" 
                                CornerRadius="5" MouseDown="Border_MouseDown">
                            <Grid>
                                <Grid.ColumnDefinitions>
                                    <ColumnDefinition Width="Auto"/>
                                    <ColumnDefinition Width="*"/>
                                </Grid.ColumnDefinitions>
                                
                                <TextBlock Grid.Column="0" Text="{Binding Icon}" 
                                           FontSize="20" Margin="0,0,10,0"/>
                                
                                <StackPanel Grid.Column="1">
                                    <TextBlock Text="{Binding Title}" 
                                               FontWeight="Bold" Foreground="White"/>
                                    <TextBlock Text="{Binding Path}" 
                                               Foreground="#CCCCCC" FontSize="11"
                                               TextTrimming="CharacterEllipsis"/>
                                </StackPanel>
                            </Grid>
                        </Border>
                    </DataTemplate>
                </ListView.ItemTemplate>
            </ListView>

            <!-- Pasek postępu -->
            <ProgressBar Height="3" Margin="0,10,0,0" 
                         IsIndeterminate="{Binding IsSearching}"
                         Visibility="{Binding IsSearching, Converter={StaticResource BooleanToVisibilityConverter}}"/>
        </StackPanel>
    </Grid>
</local:GlassWindow>
using CustomSearchApp.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace CustomSearchApp
{
    public partial class MainWindow : GlassWindow
    {
        private readonly MainViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
        }

        // Obsługa kliknięcia na wynik
        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && sender is FrameworkElement element)
            {
                _viewModel.OpenResultCommand.Execute(element.DataContext);
            }
        }

        // Ukrywanie okna po naciśnięciu Esc
        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.Hide();
            }
            base.OnKeyDown(e);
        }
    }
}
