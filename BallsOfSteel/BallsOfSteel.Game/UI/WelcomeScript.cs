﻿using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.UI;
using SiliconStudio.Xenko.UI.Controls;
using SiliconStudio.Xenko.UI.Events;

namespace BallsOfSteel.UI
{
    public class WelcomeScript : StartupScript
    {
        public UIComponent UI { get; set; }

        private EditText ipAddressEditText;
        private Button connectButton;

        public override void Start()
        {
            UI = UI ?? Entity.Get<UIComponent>();

            var page = UI.Page;
            ipAddressEditText = page.RootElement.FindVisualChildOfType<EditText>("IpAdress");
            connectButton = page.RootElement.FindVisualChildOfType<Button>("Connect");
            connectButton.Click += OnConnectButonClicked;
        }

        /// <inheritdoc />
        public override void Cancel()
        {
            if (connectButton != null)
                connectButton.Click -= OnConnectButonClicked;
        }

        private void OnConnectButonClicked(object sender, RoutedEventArgs e)
        {
            var ipAddress = ipAddressEditText.Text?.Replace("-", ".");
            // TODO connect
        }
    }
}
