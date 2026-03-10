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
            _context = Mazina_GLAZKIEntities1.GetContext();
           // MessageBox.Show($"ID агента: {(agent != null ? agent.ID.ToString() : "null")}");

            if (agent != null && agent.ID != 0)
            {
                // Загружаем существующего агента
                currentAgents = _context.Agent.Include("ProductSale.Product")
                                            .FirstOrDefault(a => a.ID == agent.ID);
                

                if (currentAgents == null)
                {
                    // Если агент не найден в базе
                    currentAgents = new Agent();
                    currentAgents.ProductSale = new List<ProductSale>();
                    DeleteBtn.Visibility = Visibility.Hidden;
                    
                    _context.Agent.Add(currentAgents);
                }
                else
                {
                    
                    ComboType.SelectedIndex = currentAgents.AgentTypeID - 1;
                    DeleteBtn.Visibility = Visibility.Visible;

                    if (!string.IsNullOrEmpty(currentAgents.Logo))
                    {
                        string projectPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\.."));
                        string imagePath = System.IO.Path.Combine(projectPath, currentAgents.Logo);

                        if (File.Exists(imagePath))
                        {
                            LogoImage.Source = new BitmapImage(new Uri(imagePath));
                        }
                    }
                }
            }
            else
            {
                // нов агент
                currentAgents = new Agent();
                currentAgents.ProductSale = new List<ProductSale>();
                DeleteBtn.Visibility = Visibility.Hidden;

                _context.Agent.Add(currentAgents);
            }

            // м контекст данных  привязка
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
                if (string.IsNullOrWhiteSpace(currentAgents.Phone))
                    errors.AppendLine("Укажите телефон агента");
                else
                {
                    string ph = currentAgents.Phone
                        .Replace("+", "")
                        .Replace("(", "")
                        .Replace(")", "")
                        .Replace("-", "")
                        .Replace(" ", "");

                    if (ph.Length != 11)
                        errors.AppendLine("Телефон должен содержать 11 цифр");
                    else if (ph[0] != '7' && ph[0] != '8')
                        errors.AppendLine("Телефон должен начинаться с 7 или 8");
                }
            }
            if (string.IsNullOrWhiteSpace(currentAgents.Email))
                errors.AppendLine("Укажите почту агента"); 
            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }
            currentAgents.AgentTypeID = ComboType.SelectedIndex + 1;
           
            try
            {
                _context.SaveChanges();
                MessageBox.Show("Информация сохранена");
                Manager.MainFrame.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void DeleteBtn_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // загр агента
                var agentForDelete = _context.Agent
                    .Include("ProductSale")
                    .FirstOrDefault(a => a.ID == currentAgents.ID);

                if (agentForDelete == null)
                {
                    MessageBox.Show("Агент не найден в базе");
                    return;
                }

                // Проверка на наличие продаж
                if (agentForDelete.ProductSale != null && agentForDelete.ProductSale.Count > 0)
                {
                    MessageBox.Show("Нельзя удалить агента, с продажами");
                    return;
                }

                var result = MessageBox.Show("Удалить агента?", "Внимание",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _context.Agent.Remove(agentForDelete);
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
            OpenFileDialog myOpenFileDialog = new OpenFileDialog();
            myOpenFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg|All files (*.*)|*.*";

            if (myOpenFileDialog.ShowDialog() == true)
            {
                try
                {
                    // Копируем файл в папку проекта
                    string fileName = System.IO.Path.GetFileName(myOpenFileDialog.FileName);
                    string destPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Imgs", "agents", fileName);

                    // Создаем папку, если её нет
                    Directory.CreateDirectory(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Imgs", "agents"));

                    // Копируем файл (перезаписываем если существует)
                    File.Copy(myOpenFileDialog.FileName, destPath, true);

                    // Устанавливаем изображение
                    LogoImage.Source = new BitmapImage(new Uri(destPath));

                    // ИЗМЕНИ ЭТУ СТРОКУ - сохраняем с путём к папке
                    currentAgents.Logo = $@"\agents\{fileName}";  // ← вот так
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке изображения: {ex.Message}");
                }
            }
        }
    }
   
}
