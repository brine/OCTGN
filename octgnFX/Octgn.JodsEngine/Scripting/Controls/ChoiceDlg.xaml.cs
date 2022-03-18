using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Linq;

namespace Octgn.Scripting.Controls
{
    public class ChoiceButton
    {
        public string Label;
        public string Background;
        public string Foreground;
        public int Height;
        public int Size;
        public bool IsEnabled = true;
    }
    public partial class ChoiceDlg
    {
        public int? SelectedControl;
        public int[] SelectedChoices;

        public string TitleBar;
        public string Text;
        public List<ChoiceButton> Choices = new List<ChoiceButton>();
        public List<ChoiceButton> Controls = new List<ChoiceButton>();
        public bool Multi;


        // deprecated format
        public ChoiceDlg(string title, string text, List<string> choices, List<string> colors, List<string> buttons)
        {
            InitializeComponent();
            //fix MAINWINDOW bug
            Owner = WindowManager.PlayWindow;
            TitleBar = title;
            Text = text;
            Multi = false;

            for (int i = 0; i < choices.Count; i++)
            {
                var button = new ChoiceButton
                {
                    Label = choices[i],
                    Background = colors[i]
                };
                Choices.Add(button);
            }
            foreach (var control in buttons)
            {
                var button = new ChoiceButton
                {
                    Label = control
                };
                Controls.Add(button);
            }
            Populate();
        }

        public ChoiceDlg(string title, string text, bool multi, List<string> labels, List<string> backgrounds,
            List<string> foregrounds, List<int> sizes, List<int> heights, List<bool> enableds, List<bool> ischoices)
        {
            InitializeComponent();
            //fix MAINWINDOW bug
            Owner = WindowManager.PlayWindow;
            TitleBar = title;
            Text = text;
            Multi = multi;
            for (int i = 0; i < labels.Count; i++)
            {
                var button = new ChoiceButton
                {
                    Label = labels[i],
                    Background = backgrounds[i],
                    Foreground = foregrounds[i],
                    Height = heights[i],
                    Size = sizes[i],
                    IsEnabled = enableds[i],
                };
                if (ischoices[i])
                {
                    Choices.Add(button);
                }
                else
                    Controls.Add(button);
            }
            Populate();
        }

        public void Populate()
        {
            Title = TitleBar;
            promptLbl.Text = Text;

            int choiceId = 0;

            foreach (var choice in Choices)
            {
                TextBlock buttonText = new TextBlock();
                buttonText.Margin = new Thickness(10, 5, 10, 5);
                buttonText.TextWrapping = TextWrapping.Wrap;
                buttonText.Text = choice.Label;
                if (choice.Foreground != null)
                {
                    var converter = new BrushConverter();
                    buttonText.Foreground = (Brush)converter.ConvertFromString(choice.Foreground);
                }
                if (choice.Size > 0)
                    buttonText.FontSize = choice.Size;
                if (choice.Height > 0)
                    buttonText.MinHeight = choice.Height;

                ToggleButton button = new ToggleButton();
                button.Content = buttonText;
                button.IsEnabled = choice.IsEnabled;
                if (choice.Background != null)
                {
                    var converter = new BrushConverter();
                    button.Background = (Brush)converter.ConvertFromString(choice.Background);
                }
                button.Click += Choice_Click;

                button.Uid = choiceId.ToString();
                button.Margin = new Thickness(5, 1, 5, 1);
                choicesField.Children.Add(button);
                choiceId++;
            }

            int controlId = 0;
            foreach (var control in Controls)
            {
                TextBlock buttonText = new TextBlock();
                buttonText.Margin = new Thickness(10, 5, 10, 5);
                buttonText.TextWrapping = TextWrapping.Wrap;
                buttonText.Text = control.Label;
                if (control.Foreground != null)
                {
                    var converter = new BrushConverter();
                    buttonText.Foreground = (Brush)converter.ConvertFromString(control.Foreground);
                }
                if (control.Size > 0)
                    buttonText.FontSize = control.Size;

                Button button = new Button();
                button.Content = buttonText;
                button.IsEnabled = control.IsEnabled;
                if (control.Background != null)
                {
                    var converter = new BrushConverter();
                    button.Background = (Brush)converter.ConvertFromString(control.Background);
                }
                button.Click += Control_Click;

                button.Uid = controlId.ToString();
                button.Margin = new Thickness(8);
                button.HorizontalAlignment = HorizontalAlignment.Center;
                button.MinWidth = 50;
                controlsField.Children.Add(button);
                controlId++;
            }
        }

        public int GetChoice()
        {
            ShowDialog();
            if (SelectedChoices != null && SelectedChoices.Length > 0)
            {
                return SelectedChoices[0] + 1;
            }
            if (SelectedControl != null)
            {
                return ((int)SelectedControl + 1) * -1;
            }
            return 0;
        }

        private void Choice_Click(object sender, RoutedEventArgs e)
        {
            string intresult = ((ToggleButton)sender).Uid;
            SelectedControl = null;
            SelectedChoices = new int[] { Convert.ToInt32(intresult) };
            if (!Multi)
                DialogResult = true;
        }
        private void Control_Click(object sender, RoutedEventArgs e)
        {
            string intresult = ((Button)sender).Uid;
            SelectedControl = (int)Convert.ToInt32(intresult);

            SelectedChoices = (from ToggleButton button in choicesField.Children
                         where button.IsChecked == true
                         select (int)Convert.ToInt32(button.Uid)).ToArray();
            DialogResult = true;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            }
        }
    }
