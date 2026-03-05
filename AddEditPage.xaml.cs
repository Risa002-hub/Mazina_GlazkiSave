using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Логика взаимодействия для AddEditPage.xaml
    /// </summary>
    public partial class AddEditPage : Page
    {
        Agent currentAgents;
        private Mazina_GLAZKIEntities1 _context;

        public AddEditPage(Agent agent)
        {
            InitializeComponent();
            _context = new Mazina_GLAZKIEntities1();

            if (agent != null && agent.ID != 0)
            {
                currentAgents = _context.Agent.Include("ProductSale.Product")
                                            .FirstOrDefault(a => a.ID == agent.ID);

                if (currentAgents == null)
                {
                    currentAgents = new Agent();
                    currentAgents.ProductSale = new List<ProductSale>();
                    DeleteBtn.Visibility = Visibility.Hidden;
                }
                else
                {
                    ComboType.SelectedIndex = currentAgents.AgentTypeID - 1;
                    DeleteBtn.Visibility = Visibility.Visible;

                    if (!string.IsNullOrEmpty(currentAgents.Logo))
                    {
                        string imagePath = AppDomain.CurrentDomain.BaseDirectory + "agents\\" + currentAgents.Logo; if (File.Exists(imagePath))
                        {
                            LogoImage.Source = new BitmapImage(new Uri(imagePath));
                        }
                    }

                }
            }
            else
            { 
                currentAgents = new Agent(); 
                DeleteBtn.Visibility = Visibility.Hidden;
            }
            DataContext = currentAgents;
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
           StringBuilder errors = new StringBuilder();
            if (string.IsNullOrWhiteSpace(currentAgents.Title))
                errors.AppendLine("Укажите наименование");
            if (string.IsNullOrWhiteSpace(currentAgents.Address))
                errors.AppendLine("Укажите адрес агента");
            if (string.IsNullOrWhiteSpace(currentAgents.DirectorName))
                errors.AppendLine("Укажите ФИО директора");
            if (ComboType.SelectedItem == null)
                errors.AppendLine("Укажите тип агента");
            if (string.IsNullOrWhiteSpace(currentAgents.Priority.ToString()))
                errors.AppendLine("Укажите приоритет агента");
            if (currentAgents.Priority <= 0)
                errors.AppendLine("Укажите положительный приоритет агента");
            if (string.IsNullOrWhiteSpace(currentAgents.INN))
                errors.AppendLine("Укажите ИНН агента");
            if (string.IsNullOrWhiteSpace(currentAgents.KPP))
                errors.AppendLine("Укажите КПП агента");
            if (string.IsNullOrWhiteSpace(currentAgents.Phone))
                errors.AppendLine("Укажите телефон агента");
            else
            {
                string ph = currentAgents.Phone.Replace("(", "").Replace("-", "").Replace("+", "");
                if (((ph[1] == '9' || ph[1] == '4' || ph[1] == '8') && ph.Length != 11) || (ph[1] == '3' && ph.Length != 12))
                    errors.AppendLine("Укажите телефон корректно");
            }
            if (string.IsNullOrWhiteSpace(currentAgents.Email))
                errors.AppendLine("Укажите почту агента");
            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }
            // добавить в контекст текущие значения новой услуги 
            if (currentAgents.ID ==0) 
                Mazina_GLAZKIEntities1.GetContext().SaveChanges();
            // сохр изм, если нет ошибок.
            try
            {
                Mazina_GLAZKIEntities1.GetContext().SaveChanges();
                MessageBox.Show("Информация сохранена");
                Manager.MainFrame.GoBack();
            }
            catch(Exception ex) { MessageBox.Show(ex.Message.ToString()); }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Проверка на наличие продаж
                if (currentAgents.ProductSale.Count > 0)
                {
                    MessageBox.Show("Нельзя удалить агента, у которого есть продажи!");
                    return;
                }

                var result = MessageBox.Show("Вы точно хотите удалить агента?", "Внимание",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _context.Agent.Attach(currentAgents);
                    _context.Agent.Remove(currentAgents);
                    _context.SaveChanges();
                    MessageBox.Show("Агент удален");
                    Manager.MainFrame.GoBack();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}");
            }
        }

        private void ChangePicBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog OpenFile = new OpenFileDialog();
            OpenFile.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";
            if (OpenFile.ShowDialog() == true)
            {
                try
                {
                    // Копируем файл в папку проекта
                    string fileName = System.IO.Path.GetFileName(OpenFile.FileName);
                    string destPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory,  "agents", fileName);

                    // Создаем папку, если её нет
                    Directory.CreateDirectory(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "agents"));

                    // Копируем файл (перезаписываем если существует)
                    File.Copy(OpenFile.FileName, destPath, true);

                    // Устанавливаем изображение
                    LogoImage.Source = new BitmapImage(new Uri(destPath));

                    // Сохраняем только имя файла в БД
                    currentAgents.Logo = fileName;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}");
                }
            }


        }
    }
}
