using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PDV.UI.WinUI3.Controls
{
    public sealed partial class EmptyStateControl : UserControl
    {
        public EmptyStateControl()
        {
            this.InitializeComponent();
            this.Loaded += EmptyStateControl_Loaded;
        }

        private void EmptyStateControl_Loaded(object sender, RoutedEventArgs e)
        {
            // No WinUI 3, a propriedade AutoPlay controla a reprodução automática
            // Se você precisar iniciar manualmente, pode usar:
            Player.Source = new Animations.Empty_state();
        }

        // Propriedades personalizáveis para o texto
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(
                nameof(Message),
                typeof(string),
                typeof(EmptyStateControl),
                new PropertyMetadata("Nenhum registro encontrado")
            );

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register(
                nameof(Description),
                typeof(string),
                typeof(EmptyStateControl),
                new PropertyMetadata("Tente ajustar os filtros ou adicione um novo registro")
            );

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }
    }
}
