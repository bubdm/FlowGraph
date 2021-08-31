﻿using System.Windows;
using System.ComponentModel;
using FlowGraphBase.Script;

namespace FlowSimulator.UI
{
    /// <summary>
    /// Interaction logic for NewScriptWindow.xaml
    /// </summary>
    public partial class NewScriptWindow : Window
    {
		#region Fields

        public delegate bool IsValidInputNameDelegate(string name_);

        private bool _DialogResult = false;

		#endregion //Fields
	
		#region Properties

        /// <summary>
        /// 
        /// </summary>
        public string InputName
        {
            get => textBoxName.Text;
            set => textBoxName.Text = value;
        }

        /// <summary>
        /// 
        /// </summary>
        public IsValidInputNameDelegate IsValidInputNameCallback
        {
            get;
            set;
        }

		#endregion //Properties
	
		#region Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scriptElement"></param>
        public NewScriptWindow(ScriptElement scriptElement = null)
        {
            InitializeComponent();

            if (scriptElement == null)
            {
                Title = "New script";
            }
            else
            {
                InputName = scriptElement.Name;
            }
            
            Closing += OnClosing;
        }

		#endregion //Constructors
	
		#region Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnClosing(object sender, CancelEventArgs e)
        {
            DialogResult = _DialogResult;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            if (IsValidInputNameCallback == null
                || (IsValidInputNameCallback != null
                    && IsValidInputNameCallback.Invoke(InputName)))
            {
                _DialogResult = true;
                Close();
            }
            else
            {
                _DialogResult = false;
                labelError.Content = "'" + InputName + "' is not a valid name. Please enter a valid name.";
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            _DialogResult = false;
            Close();
        }

		#endregion //Methods
    }
}
