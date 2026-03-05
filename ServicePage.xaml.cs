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
        }
        int CountRecords; // записи
        int CountPage; // страницы
        int CurrentPage = 0; // текущ стр 

        List<Agent> CurrentPageList = new List<Agent>();
        List<Agent> TableList;
        private void ChangePage(int direction, int?SelectedPage)
        {
            CurrentPageList.Clear();
            CountRecords = TableList.Count;
            if (CountRecords % 10 > 0)
            {
                CurrentPage = CountRecords / 10 + 1;
            }
            else 
            {
                CountPage = CountRecords / 10;  
            }

            Boolean Ifupdate = true;
            int min;
            if (SelectedPage.HasValue)
            {
                if (SelectedPage >= 0 && SelectedPage <= CountPage)
                {
                    CurrentPage = (int)SelectedPage;
                    min = CurrentPage * 10 + 10 < CountRecords ? CountPage * 10 + 10 : CountRecords;
                    for (int i = CurrentPage * 10; i < min; i++)
                    {
                        CurrentPageList.Add(TableList[i]);
                    }
                }
            }
            else
            {
                switch (direction)
                {
                    case 1:
                        if (CurrentPage>0)
                        {
                            CurrentPage--;
                            min = CurrentPage * 10 + 10 < CountRecords ? CurrentPage * 10 + 10 : CountRecords;
                            for (int i = CurrentPage * 10;i < min; i++)
                            {
                                CurrentPageList.Add(TableList[i]);
                            }
                        }
                        else
                        {
                            Ifupdate = false;
                        }
                        break;
                    case 2:
                        if(CurrentPage<CountPage-1)
                        {
                            CurrentPage++;
                            min = CurrentPage * 10 + 10 < CountRecords ? CurrentPage * 10 + 10 : CountRecords;
                            for(int i = CurrentPage * 10; i<= min; i++)
                            {
                                CurrentPageList.Add(TableList[i]);
                            }
                        }
                        else
                        {
                            Ifupdate = false;
                        }
                        break;
                       
                }
            }
            if (Ifupdate)
            {
                PageListBox.Items.Clear();
                for (int i = 1; i <= CountPage; i++)
                {
                    PageListBox.Items.Add(i);
                }
                PageListBox.SelectedIndex = CurrentPage;
                ServiceListView.ItemsSource = CurrentPageList;
                ServiceListView.Items.Refresh();
            }
        }
        private void UpdateService()
        {
            try
            {
                var currentServices = Mazina_GLAZKIEntities1.GetContext().Agent.ToList();
                if (ComboType.SelectedItem !=null)
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


                // Показываем результат
                ServiceListView.ItemsSource = currentServices;
                ServiceListView.ItemsSource = currentServices;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }

        }
        
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Manager.MainFrame.Navigate(new AddEditPage());
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
            ChangePage(1,null);
        }

        private void RightDirBtn_Click(object sender, RoutedEventArgs e)
        {
            ChangePage(2,null);
        }

        private void PageListBox_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(PageListBox.SelectedItem !=null)
            {
                if(int.TryParse(PageListBox.SelectedItem.ToString(),out int pageNumber))
                {
                    ChangePage(0,pageNumber-1);
                }
            }
          //  ChangePage(0, Convert.ToInt32(PageListBox.SelectedItem.ToString()) - 1);
        }
    }
}
