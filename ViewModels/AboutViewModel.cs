using System.Collections.ObjectModel;

public class AboutViewModel : ContentPage
{
    public ObservableCollection<Developer> Developers { get; } = new();

    public AboutViewModel()
    {
        LoadDevelopers();
    }

    private void LoadDevelopers()
    {
        Developers.Add(new Developer
        {
            Photo = "lena_photo.jpg",
            Name = "Лена Бибизян",
            Role = "Designer"
        });

        Developers.Add(new Developer
        {
            Photo = "zabus_photo.jpg",
            Name = "Алена Забус",
            Role = "Designer"
        });

        Developers.Add(new Developer
        {
            Photo = "max_photo.jpg",
            Name = "Макс Гослинг",
            Role = "Team Lead / .NET Developer"
        });

        Developers.Add(new Developer
        {
            Photo = "denis_photo.jpg",
            Name = "Денис Опосс",
            Role = "Backend Developer"
        });

        Developers.Add(new Developer
        {
            Photo = "misha_photo.jpg",
            Name = "Миша Лоск",
            Role = "Backend Developer"
        });
    }
}

public class Developer
{
    public string Photo { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}