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
