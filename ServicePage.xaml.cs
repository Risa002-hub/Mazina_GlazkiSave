using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mazina_GlazkiSave
{
    /// <summary>
    /// Логика взаимодействия для ServicePage.xaml
    /// </summary>
    public partial class ServicePage : Page
    {
        public ServicePage()
        {
            InitializeComponent();
            var currentServices = Mazina_GLAZKIEntities1.GetContext().Agent.ToList();
            ServiceListView.ItemsSource = currentServices;
            ComboType.SelectedIndex = 0;
            ComboSort.SelectedIndex = 0;
            UpdateService();
            TableList = currentServices;
            ChangePage(0, 0);
        }
        int CountRecords; // записи
        int CountPage;
        int CurrentPage = 0; // текущ стр 

        List<Agent> CurrentPageList = new List<Agent>();
        List<Agent> TableList;
        int AgentCount = 10;
        private void ChangePage(int direction, int? selectedPage)

        {
            if (PageListBox == null) return;// смотрим наличие данных

            CountRecords = TableList.Count; // общее колво данных
            CountPage = (int)Math.Ceiling((double)CountRecords / AgentCount); //кол-во записей на  агентов стр
            if (selectedPage.HasValue) //текущая страница
            {
                CurrentPage = selectedPage.Value;
            }
            else
            {
                switch (direction)
                {
                    case 1: CurrentPage--; break;
                    case 2: CurrentPage++; break;
                }
            }

            //ограничение
            if (CurrentPage < 0)
            {
                CurrentPage = 0;
            }
            if (CurrentPage >= CountPage)
            {
                CurrentPage = CountPage - 1;
            }

            CurrentPageList = TableList
                .Skip(CurrentPage * AgentCount)
                .Take(AgentCount) // берем 10 новых агентов
                .ToList(); // на страницу
            PageListBox.Items.Clear(); //обновление списка на странице
            for (int i = 1; i <= CountPage; i++)
            {
                PageListBox.Items.Add(i);
            }
            PageListBox.SelectedIndex = CurrentPage;
            ServiceListView.ItemsSource = CurrentPageList;
            ServiceListView.Items.Refresh();
        }
        private void UpdateService()
        {
            try
            {
                var currentServices = Mazina_GLAZKIEntities1.GetContext().Agent.ToList();
                if (ComboType.SelectedItem != null)
                {
                    string selectedType = (ComboType.SelectedItem as TextBlock).Text;

                    if (selectedType != "Все типы")
                    {
                        currentServices = currentServices.Where(p => p.AgentTypeTitle == selectedType).ToList();
                    }
                }
                if (!string.IsNullOrEmpty(TBSearch.Text))
                {
                    string searchText = TBSearch.Text.ToLower();
                    string cleanedSearchPhone = searchText
                        .Replace("+", "")
                        .Replace("(", "")
                        .Replace(")", "")
                        .Replace("-", "")
                        .Replace(" ", "");
                    //.Replace("8", "7");

                    currentServices = currentServices.Where(p =>
                        // Поиск по названию
                        (p.Title != null && p.Title.ToLower().Contains(searchText)) ||

                        // Поиск по email
                        (p.Email != null && p.Email.ToLower().Contains(searchText)) ||

                        // Поиск по телефону
                        (p.Phone != null && p.Phone
                        .Replace("+", "")
                        .Replace("(", "")
                        .Replace(")", "")
                        .Replace("-", "")
                        .Replace(" ", "").
                        //.Replace("8", "7").
                        Contains(cleanedSearchPhone))
                    ).ToList();
                }
                if (ComboSort.SelectedIndex == 0)
                {

                }
                if (ComboSort.SelectedIndex == 2)
                {
                    currentServices = currentServices.OrderBy(p => p.Title).ToList();
                }
                if (ComboSort.SelectedIndex == 2)
                {
                    currentServices = currentServices.OrderByDescending(p => p.Title).ToList();
                }
                if (ComboSort.SelectedIndex == 3)
                {
                    currentServices = currentServices.OrderBy(p => p.Priority).ToList();
                }
                if (ComboSort.SelectedIndex == 4)
                {
                    currentServices = currentServices.OrderByDescending(p => p.Priority).ToList();
                }
                if (ComboSort.SelectedIndex == 5)
                {
                    currentServices = currentServices.OrderBy(p => p.Discount).ToList();
                }
                if (ComboSort.SelectedIndex == 6)
                {
                    currentServices = currentServices.OrderByDescending(p => p.Discount).ToList();
                }
                // Сохраняем отфильтрованный список для пагинации
                TableList = currentServices;

                // Показываем результат
                ServiceListView.ItemsSource = currentServices;
                ServiceListView.ItemsSource = currentServices;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }

        }

        private void TBSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateService();
        }

        private void ComboSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateService();
        }

        private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateService();
        }

        private void LeftDirBtn_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(1, null);
        }

        private void RightDirBtn_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(2, null);
        }

        private void PageListBox_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (PageListBox.SelectedItem != null)
            {
                if (int.TryParse(PageListBox.SelectedItem.ToString(), out int pageNumber))
                {
                    ChangePage(0, pageNumber - 1);
                }
            }
            //  ChangePage(0, Convert.ToInt32(PageListBox.SelectedItem.ToString()) - 1);
        }

        private void AddBtn_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage(null));
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                // Сбрасываем кэш контекста
                var context = Mazina_GLAZKIEntities1.GetContext();

                // Обновляем список с фильтрами
                UpdateService();

                // Для корректной работы пагинации берём отфильтрованный список из ItemsSource
                TableList = ServiceListView.ItemsSource.Cast<Agent>().ToList();

                // Переходим на первую страницу
                ChangePage(0, 0);
            }
        }

        private void EditMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var selectedAgent = ServiceListView.SelectedItem as Agent;

            if (selectedAgent != null)
            {
               
                Manager.MainFrame.Navigate(new AddEditPage(selectedAgent));
            }
            else
            {
                MessageBox.Show("Выберите агента для редактирования");
            }
        }
    }

}
