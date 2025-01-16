using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.RightsManagement;
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

namespace Virus_Simulator_Intepface.UserControls
{
    /// <summary>
    /// Interaction logic for NumericField.xaml
    /// </summary>
    public partial class NumericField : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private string inputValue;
        private int position;

        public string InputValue
        { 
            get
            { 
                return inputValue;
            }

            set
            {  
                inputValue = value;
                OnPropertyChanged();
            }
        }

        public int Position
        {
            get
            {
                return position;
            } 
            
            set
            {
                position = value;
                OnPropertyChanged();
            }
        }



        public NumericField()
        {
            DataContext = this;
            InitializeComponent();
        }

        private void InputField_TextChanged(object sender, TextChangedEventArgs e)
        {
            RemoveNonNumerics(InputField);
            CheckInRange(InputField);
        }

        private void RemoveNonNumerics(TextBox tb)
        {
            //Checks if the input contains characters other han numbers or is a negative number
            if ((!int.TryParse(InputValue, out _)) || (InputValue.Contains('-')))
            {
                var sd = new StringBuilder();

                //Loops through the input and adds any number to the string builder
                foreach (var character in InputValue)
                {
                    if (char.IsDigit(character))
                        sd.Append(character);
                }

                InputValue = sd.ToString();
                tb.SelectionStart = InputValue.Length;
            }
        }

        private void CheckInRange(TextBox tb)
        {
            if ((InputValue == null) || (InputValue == "")) return;
            if (int.Parse(InputValue) <= 100) return;

            //Limits the max value of the input to 100 by removing the last character in the string if it exceeds 100
            int totalLength = InputValue.Length;
            InputValue = InputValue.Remove(totalLength - 1);
            tb.SelectionStart = InputValue.Length;
        }

        private void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
